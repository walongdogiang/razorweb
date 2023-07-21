using Bogus.DataSets;
using EFWeb.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace EFWeb.Areas.Admin.Pages.Role
{
    public class EditRoleClaimModel : RolePageModel
    {
        public EditRoleClaimModel(RoleManager<IdentityRole> roleManager, MyBlogContext myBlogContext)
             : base(roleManager, myBlogContext) { }

        public class InputModel
        {
            [Display(Name = "Kiểu Claim")]
            [Required(ErrorMessage = "{0} không được để trống")]
            [StringLength(256, MinimumLength = 3, ErrorMessage = "{0} phải dài từ {2} đến {1} kí tự.")]
            public string ClaimType { get; set; }

            [Display(Name = "Giá trị")]

            [Required(ErrorMessage = "{0} không được để trống")]
            [StringLength(256, MinimumLength = 3, ErrorMessage = "{0} phải dài từ {2} đến {1} kí tự.")]
            public string ClaimValue { get; set; }
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IdentityRole role { get; set; }

        public IdentityRoleClaim<string>? claim;

        public async Task<IActionResult> OnGet(int? claimid)
        {
            if (claimid == null) return NotFound("Không thể tìm Claim với id rỗng !");
            claim = _context.RoleClaims.Where(c => c.Id == claimid).FirstOrDefault();
            if (claim == null) return NotFound("Không tìm thấy Claim này");

            role = await _roleManager.FindByIdAsync(claim.RoleId);
            if (role == null) return NotFound("Không tìm thấy Role này");

            Input = new InputModel()
            {
                ClaimType = claim.ClaimType,
                ClaimValue = claim.ClaimValue
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? claimid)
        {
            if (claimid == null) return NotFound("Không thể tìm Claim với id rỗng !");
            claim = await _context.RoleClaims.Where(c => c.Id == claimid).FirstOrDefaultAsync();
            if (claim == null) return NotFound("Không tìm thấy Claim này");

            role = await _roleManager.FindByIdAsync(claim.RoleId);
            if (role == null) return NotFound("Không tìm thấy role");

            if (!ModelState.IsValid) return Page();
            if ((claim.ClaimType, claim.ClaimValue) == (Input.ClaimType, Input.ClaimValue)) return RedirectToPage("./Edit", new { roleid = role.Id });

            //if ((await _roleManager.GetClaimsAsync(role)).Any(c => c.Type == Input.ClaimType && c.Value == Input.ClaimValue))
            //{
            //    ModelState.AddModelError(string.Empty, "Claim này đã tồn tại trong role, vui lòng nhập 1 Claim khác !");
            //    return Page();
            //}

            if (_context.RoleClaims.Any(c => c.ClaimType == Input.ClaimType && c.ClaimValue == Input.ClaimValue && c.RoleId == role.Id))
            {
                ModelState.AddModelError(string.Empty, "Claim này đã tồn tại trong role, vui lòng nhập 1 Claim khác !");
                return Page();
            }

            claim.ClaimType = Input.ClaimType;
            claim.ClaimValue = Input.ClaimValue;

            await _context.SaveChangesAsync();
            StatusMessage = "Cập nhật claim cho Role thành công !";

            return RedirectToPage("./Edit", new { roleid = role.Id });
        }
    }
}
