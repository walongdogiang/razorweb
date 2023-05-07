// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using EFWeb.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace EFWeb.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserStore<AppUser> _userStore;
        private readonly IUserEmailStore<AppUser> _emailStore;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager,
            IUserStore<AppUser> userStore,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ProviderDisplayName { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public IActionResult OnGet() => RedirectToPage("./Login");

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            // Tạo ra cửa sổ dịch vụ ngoài và cho phép ng dùng truy cập vào thông tin của dvu này
            // Khi ng dùng cho phép hê thống truy cập vào tk dvu ngoài thì dvu ngoài gửi mã token 
            // đến ứng dụng của cta thông qua đ/c CallBackPath: /dang-nhap-tu-google
            // Từ d/c này nó sẽ lấy đc mã token và tự dộng truy cập thông tin kết nối với tk của user
            // Sau khi lấy đc thông tin của User, d/c này sẽ chuyển hướng về trang redirectUrl
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Lỗi từ dịch vụ ngoài: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Không tìm thấy thông tin đăng nhập từ dịch vụ ngoài.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            // Nếu dvu ngoài này đã dc đc liên kết với tk local => cho login ngay
            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} đã đăng nhập với dịch vụ {LoginProvider}.",
                    info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut) // Nếu tk ngoài này đang bị khóa
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                // TH1: Tìm thấy tk local có Email trùng với dvu ngoài, nhưng chưa liên kết dvu ngoài
                //      => liên kết tk local vs dvu ngoài
                // TH2: Ko tìm thấy tk local có email trùng dvu ngoài
                //      => Tự động tạo tk local cho dvu ngoài và lket với dvu này
                ReturnUrl = returnUrl;
                ProviderDisplayName = info.ProviderDisplayName;
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    Input = new InputModel { Email = info.Principal.FindFirstValue(ClaimTypes.Email)};
                }
                return Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Lỗi lấy thông tin từ dịch vụ ngoài.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                // Tìm tk local có Email == Email dvu ngoài này
                var registerUser = await _userManager.FindByEmailAsync(Input.Email);

                if (registerUser != null)
                {
                    // Liên kết tk local với Email dvu ngoài này và đăng nhập
                    var resultLink = await _userManager.AddLoginAsync(registerUser, info);
                    if (resultLink.Succeeded)
                    {
                        await _signInManager.SignInAsync(registerUser, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                else // Nếu ko tìm thấy tk local có Email == Email dvu ngoài này thì tạo tk cho email này
                {
                    var newUser = new AppUser()
                    {
                        UserName = Input.Email,
                        Email = Input.Email,
                    };

                    // Tạo tk user cho Email dvu ngoài này
                    var resultCreate = await _userManager.CreateAsync(newUser);

                    // Nếu tạo thành công
                    if(resultCreate.Succeeded)
                    {
                        // Thêm dvu ngoài này vào tk mới tạo
                        var resultLink = await _userManager.AddLoginAsync(newUser, info);

                        // Tạo 1 mã token xác thực Email và hệ thống tự động xác thực luôn
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                        await _userManager.ConfirmEmailAsync(newUser, code);

                        if (resultLink.Succeeded)
                        {
                            // Chuyển tk này vào trạng thái đăng nhập
                            await _signInManager.SignInAsync(newUser, isPersistent: false);
                            return LocalRedirect(returnUrl);
                        }
                    }
                    else // Nếu tạo tk thất bại
                    {
                        ModelState.AddModelError(String.Empty, "Tạo tài khoản thất bại! Vui lòng thử lại.");
                        return Page();
                    }
                }

                return Content("Dừng lại ở đây.");


                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code },
                            protocol: Request.Scheme);

                        await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        // If account confirmation is required, we need to show the link if we don't have a real email sender
                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
                        }

                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }

        private AppUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<AppUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(AppUser)}'. " +
                    $"Ensure that '{nameof(AppUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the external login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
            }
        }

        private IUserEmailStore<AppUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<AppUser>)_userStore;
        }
    }
}
