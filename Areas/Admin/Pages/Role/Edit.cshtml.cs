using Bogus.DataSets;
using EFWeb.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EFWeb.Areas.Admin.Pages.Role
{
    public class EditModel : RolePageModel
    {
        public EditModel(RoleManager<IdentityRole> roleManager, MyBlogContext myBlogContext) 
            : base(roleManager, myBlogContext){}

        public class InputModel
        {
            [Display(Name = "Tên Role")]
            [Required(ErrorMessage = "{0} không được để trống")]
            [StringLength(256, MinimumLength = 3, ErrorMessage = "{0} phải dài từ {2} đến {1} kí tự.")]
            public string? Name { get; set; }
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [BindProperty]
        public IdentityRole role { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(string roleid)
        {
            if (roleid == null) return NotFound($"Role {roleid} không tồn tại");

            role = await _roleManager.FindByIdAsync(roleid);

            if (role != null)
            {
                Input = new InputModel()
                {
                    Name = role.Name
                };
                return Page();
            }
            return NotFound($"Role {roleid} không tồn tại");
        }

        public async Task<IActionResult> OnPostAsync(string roleid)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (roleid == null) return NotFound($"Role {roleid} không tồn tại");
            role = await _roleManager.FindByIdAsync(roleid);
            if (role == null) return NotFound($"Role {roleid} không tồn tại");

            if(role.Name == Input.Name) return RedirectToPage("./Index");

            role.Name = Input.Name;
            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                StatusMessage = $"Bạn đã đổi tên role {Input.Name} thành công.";
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
