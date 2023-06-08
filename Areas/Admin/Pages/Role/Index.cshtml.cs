using EFWeb.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EFWeb.Areas.Admin.Pages.Role
{
    public class IndexModel : RolePageModel
    {
        public IndexModel(RoleManager<IdentityRole> roleManager, MyBlogContext myBlogContext) 
            : base(roleManager, myBlogContext)
        {
        }
        
        public List<IdentityRole> roles { get; set; }
        public async Task OnGet()
        {
            roles = await _roleManager.Roles.OrderBy(role => role.Name).ToListAsync();
        }

        public void OnPost() => RedirectToPage();
    }
}
