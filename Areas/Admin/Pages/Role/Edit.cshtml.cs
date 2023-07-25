using Bogus.DataSets;
using EFWeb.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using static EFWeb.Areas.Admin.Pages.Role.IndexModel;

namespace EFWeb.Areas.Admin.Pages.Role
{
    public class EditModel : RolePageModel
    {
        public EditModel(RoleManager<IdentityRole> roleManager, AppDbContext myBlogContext) 
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

        public List<IdentityRoleClaim<string>> Claims { get; set; }


        [BindProperty]
        public IdentityRole Role { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(string roleid)
        {
            if (roleid == null) return NotFound($"Role {roleid} không tồn tại");

            Role = await _roleManager.FindByIdAsync(roleid);

            if (Role != null)
            {
                Input = new InputModel
                {
                    Name = Role.Name,
                };
                Claims = await _context.RoleClaims.Where(rc => rc.RoleId == Role.Id).ToListAsync();

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
            Role = await _roleManager.FindByIdAsync(roleid);
            if (Role == null) return NotFound($"Role {roleid} không tồn tại");

            if(Role.Name == Input.Name) return RedirectToPage("./Index");

            string oldName = Role.Name;
            Role.Name = Input.Name;
            var result = await _roleManager.UpdateAsync(Role);

            if (result.Succeeded)
            {
                StatusMessage = $"Bạn đã đổi tên role {oldName} thành {Input.Name} thành công.";
                return RedirectToPage("./Index");
            }
            else
            {
                result.Errors.ToList().ForEach(error =>
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                });
            }
            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostDeleteClaimAsync(int? claimid)
        {
            if (claimid == null) return NotFound("Không thể tìm Claim với id rỗng !");
            var claim = _context.RoleClaims.Where(c => c.Id == claimid).FirstOrDefault();
            if (claim == null) return NotFound("Không tìm thấy Claim này");

            Role = await _roleManager.FindByIdAsync(claim.RoleId);
            if (Role == null) return NotFound($"Role {claim.RoleId} không tồn tại");

            var result = await _roleManager.RemoveClaimAsync(Role, new Claim(claim.ClaimType, claim.ClaimValue));
            if (!result.Succeeded)
            {
                result.Errors.ToList().ForEach(error =>
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                });
                return Page();
            }
            StatusMessage = $"Bạn đã xóa thành công Claim: {claim.ClaimType} - {claim.ClaimValue} khỏi Role này.";
            return RedirectToPage("./Edit", new {roleid = Role.Id});
        }
    }
}
