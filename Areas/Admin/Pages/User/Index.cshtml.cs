using EFWeb.Helpers;
using EFWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EFWeb.Areas.Admin.Pages.User
{
    public class IndexModel : UserPageModel
    {
        public IndexModel(UserManager<AppUser> userManager) : base(userManager){}

        public class UserAndRole : AppUser
        {
            public string? RoleNames { get; set; }
        }

        [TempData]
        public string StatusMessage { get; set; }

        public List<UserAndRole> users { get; set; }

        public const int ITEMS_PER_PAGE = 10;
        public int STT { get; set; }

        [BindProperty(SupportsGet = true)]
        public QueryHttps queryHttps { get; set; }

        public int countPage { get; set; } = 0;

        public async Task<IActionResult> OnGet()
        {
            if (_userManager.Users == null) return Page();

            var queryUsers = _userManager.Users;

            if (queryHttps.SearchBlog != null)
            {
                queryUsers = queryUsers.Where(x => x.UserName.Contains(queryHttps.SearchBlog));
            }
            else
            {
                queryUsers = queryUsers.OrderBy(u => u.UserName);
            }

            if (queryUsers != null)
            {
                countPage = (int)Math.Ceiling((double)queryUsers.Count() / ITEMS_PER_PAGE);

                // check if currentPage is out of range(1,currentPage)
                queryHttps.CurrentPage = queryHttps.CurrentPage < 1 ? 1
                    : queryHttps.CurrentPage > countPage ? countPage : queryHttps.CurrentPage;

                STT = (queryHttps.CurrentPage - 1) * ITEMS_PER_PAGE + 1;

                users = await queryUsers.Skip(STT - 1)
                    .Take(ITEMS_PER_PAGE).Select(u => new UserAndRole()
                    {
                        Id = u.Id,
                        UserName= u.UserName,

                    }).ToListAsync();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    user.RoleNames = string.Join(", ", roles);
                }
            }
            return Page();
        }

        public void OnPost() => RedirectToPage();
    }
}
