
using System.ComponentModel.DataAnnotations;
using EFWeb.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace EFWeb.Areas.Admin.Pages.User.Update
{
    [Authorize(Roles = "Editor, Member")]
    [Authorize(Roles = "Admin")]
    public class SetPasswordModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;

        public SetPasswordModel(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "{0} không được bỏ trống.")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu mới")]
            public string? NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Nhập lại mật khẩu mới")]
            [Compare("NewPassword", ErrorMessage = "{0} không chính xác.")]
            public string? ConfirmPassword { get; set; }
        }

        public AppUser user { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound("Không tìm thấy tài khoản với id rỗng.");

            user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{id}'.");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (string.IsNullOrEmpty(id)) return NotFound("Không thể tìm tải khoản với id bỏ trống.");

            user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound($"Không tìm thấy tài khoản có ID '{id}'.");
            }

            await _userManager.RemovePasswordAsync(user);
            var addPasswordResult = await _userManager.AddPasswordAsync(user, Input.NewPassword);

            if (!addPasswordResult.Succeeded)
            {
                foreach (var error in addPasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            StatusMessage = $"Mật khẩu của tài khoản {user.UserName} vừa được đổi thành công.";

            return RedirectToPage("./Index", new { id = user.Id });
        }
    }
}
