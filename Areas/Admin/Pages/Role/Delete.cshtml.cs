using Bogus.DataSets;
using EFWeb.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace EFWeb.Areas.Admin.Pages.Role
{
    public class DeleteModel : RolePageModel
    {
        public DeleteModel(RoleManager<IdentityRole> roleManager, MyBlogContext myBlogContext)
            : base(roleManager, myBlogContext) 
        {}

        [BindProperty]
        public IdentityRole role { get; set; }

        public async Task<IActionResult> OnGetAsync(string roleid)
        {
            if (roleid == null) return NotFound($"Role {roleid} không tồn tại");

            role = await _roleManager.FindByIdAsync(roleid);

            if (role != null)
            {
                return Page();
            }
            return NotFound($"Role {roleid} không tồn tại");
        }

        public async Task<IActionResult> OnPostAsync(string roleid)
        {
            if (roleid == null) return NotFound($"Role {roleid} không tồn tại");
            role = await _roleManager.FindByIdAsync(roleid);
            if (role == null) return NotFound($"Role {roleid} không tồn tại");

            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded)
            {
                StatusMessage = $"Bạn vừa xóa role {role.Name} thành công.";
                return RedirectToPage("./Index");
            }
            else
            {
                result.Errors.ToList().ForEach(error =>
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                });
            }
            return Page();
        }
    }
}
