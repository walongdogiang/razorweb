using EFWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EFWeb.Areas.Admin.Pages.User
{
    [Authorize]
    public class UserPageModel : PageModel
    {
        protected readonly UserManager<AppUser> _userManager;
        public UserPageModel(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public string transNameGender(string? gender)
            => gender == "male" ? "nam" : gender == "female" ? "nữ" : "không xác định";
    }
}
