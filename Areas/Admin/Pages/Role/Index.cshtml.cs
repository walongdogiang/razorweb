using EFWeb.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EFWeb.Areas.Admin.Pages.Role
{
    public class IndexModel : RolePageModel
    {
        public IndexModel(RoleManager<IdentityRole> roleManager, AppDbContext myBlogContext) 
            : base(roleManager, myBlogContext){}

        public class RoleModel : IdentityRole
        {
            public string[] Claims { get; set; }
        }

        public List<RoleModel> roles { get; set; }

        public async Task OnGet()
        {
            var rs = await _roleManager.Roles.OrderBy(role => role.Name).ToListAsync();
            roles = new List<RoleModel>();
            foreach (var r in rs)
            {
                var claims = await _roleManager.GetClaimsAsync(r);
                var claimsString = claims.Select(c => $"{c.Type} - {c.Value}");

                var rm = new RoleModel()
                {
                    Name = r.Name,
                    Id = r.Id,
                    Claims = claimsString.ToArray()
                };
                roles.Add(rm);
            }
        }

        public void OnPost() => RedirectToPage();
    }
}
