// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using EFWeb.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace EFWeb.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserStore<AppUser> _userStore;
        private readonly IUserEmailStore<AppUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<AppUser> userManager,
            IUserStore<AppUser> userStore,
            SignInManager<AppUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }
        public string ReturnUrl { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "{0} không được để trống.")]
            [EmailAddress(ErrorMessage = "Email sai định dạng.")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "{0} không được để trống.")]
            [StringLength(100, ErrorMessage = "Độ dài phải nằm trong khoảng từ {2} đến {1} ký tự", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu lặp lại")]
            [Compare("Password", ErrorMessage = "Mật khẩu lặp lại không đúng!")]
            public string ConfirmPassword { get; set; }

            [Required(ErrorMessage = "{0} không được để trống.")]
            [Display(Name = "Tên tài khoản")]
            [StringLength(100, ErrorMessage = "Độ dài phải nằm trong khoảng từ {2} đến {1} ký tự", MinimumLength = 6)]
            public string UserName { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                // Gán user.UserName = Input.UserName
                await _userStore.SetUserNameAsync(user, Input.UserName, CancellationToken.None);

                // Gán user.Email = Input.Email
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                // Tạo 1 User vào database với dự liệu là user kèm password = Input.Password
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Tạo tài khoản thành công!.");

                    var userId = await _userManager.GetUserIdAsync(user);

                    // Phát sinh Token - giống mã OTP để xác nhận Email
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    // /Identity/Account/ConfirmEmail?UserId=Id...
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    // Gửi Email: tới đ/c Input.Email, Tiêu đề, Nội dung Email
                    await _emailSender.SendEmailAsync(Input.Email, 
                        "Xác nhận Email cho tài khoản Razor Web",
                        $"Bạn đang đăng ký tài khoản trên Razor web." +
                        $" Hãy <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>bấm vào đây</a> để kích hoạt tài khoản.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false); // Cho ng dùng login vào trang web
                        return LocalRedirect(returnUrl); //Quay lại trang localhost
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
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
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
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
