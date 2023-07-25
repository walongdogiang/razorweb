
using System.ComponentModel.DataAnnotations;
using EFWeb.Models;
using EFWeb.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EFWeb.Areas.Admin.Pages.User.Update
{
    [Authorize]
    public class IndexModel : UserPageModel
    {
        public IndexModel(UserManager<AppUser> userManager) : base(userManager) { }


        [TempData]
        public string StatusMessage { get; set; }


        public AppUser user { get; set; }

        public async Task<IActionResult> OnGetAsync(string userid)
        {
            if (string.IsNullOrEmpty(userid)) return NotFound("Không tìm thấy người dùng.");

            user = await _userManager.FindByIdAsync(userid);
            if (user == null)
            {
                return NotFound($"Không thể tải lên tài khoản với ID '{userid}'.");
            }

            return Page();
        }

        public void OnPost()
        {
        }

    }
}
