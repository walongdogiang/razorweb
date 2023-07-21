using System.ComponentModel.DataAnnotations;
using EFWeb.Model;
using EFWeb.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EFWeb.Areas.Admin.Pages.User.Update
{
    [Authorize]
    public class EditModel : UserPageModel
    {

        public EditModel(UserManager<AppUser> userManager) : base(userManager) { }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Phone(ErrorMessage = "{0} không hợp lệ")]
            [Display(Name = "Số điện thoại")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Địa chỉ")]
            [StringLength(400)]
            public string HomeAddress { get; set; }

            [BirthDateValidation(1940, 2010)]
            [Display(Name = "Ngày sinh")]
            public DateTime? BirthDate { get; set; }

            [Display(Name = "Giới tính")]
            public string Gender { get; set; }
        }

        public string[] Genders { get; set; }


        public AppUser user { get; set; }

        private async Task LoadAsync(AppUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            Genders = new[] { "male", "female", "unspecified" };

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                HomeAddress = user.HomeAddress,
                BirthDate = user.BirthDate,
                Gender = user.Gender,
            };
        }

        public async Task<IActionResult> OnGetAsync(string userid)
        {
            if (string.IsNullOrEmpty(userid)) return NotFound("Không tìm thấy người dùng.");

            user = await _userManager.FindByIdAsync(userid);
            if (user == null)
            {
                return NotFound($"Không thể tải lên tài khoản với ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string userid)
        {
            if (string.IsNullOrEmpty(userid)) return NotFound("Không tìm thấy người dùng.");

            user = await _userManager.FindByIdAsync(userid);

            if (user == null)
            {
                return NotFound($"Không thể tải lên tài khoản với ID '{userid}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            user.PhoneNumber = Input.PhoneNumber;
            user.HomeAddress = Input.HomeAddress;
            user.BirthDate = Input.BirthDate;
            user.Gender = Input.Gender;

            await _userManager.UpdateAsync(user);

            StatusMessage = $"Hồ sơ của tài khoản {user.UserName} đã được cập nhật.";
            return RedirectToPage("./Index", new { userid = user.Id});
        }
    }
}
