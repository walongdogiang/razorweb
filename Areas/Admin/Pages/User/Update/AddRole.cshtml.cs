using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EFWeb.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EFWeb.Areas.Admin.Pages.User.Update
{
    public class AddRoleModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AddRoleModel(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public AppUser user { get; set; }

        [BindProperty]
        [DisplayName("Các roles của người dùng")]
        public string[]? userRoles { get; set; }

        public SelectList allRoles { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound("Không tìm thấy tài khoản với id rỗng.");

            user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{id}'.");
            }

            userRoles = (await _userManager.GetRolesAsync(user)).ToArray();

            List<string> rolesName = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            allRoles = new SelectList(rolesName);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (string.IsNullOrEmpty(id)) return NotFound("Không thể tìm tải khoản với id bỏ trống.");

            user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound($"Không tìm thấy tài khoản có ID '{id}'.");
            }

            var oldRoles = (await _userManager.GetRolesAsync(user)).ToArray();
            var deleteRoles = oldRoles.Where(r => !userRoles.Contains(r));
            var addRoles = userRoles.Where(r => !oldRoles.Contains(r));

            List<string> rolesName = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            allRoles = new SelectList(rolesName);

            var resultDelete = await _userManager.RemoveFromRolesAsync(user, deleteRoles);
            if (!resultDelete.Succeeded)
            {
                resultDelete.Errors.ToList().ForEach(e => ModelState.AddModelError(string.Empty, e.Description));
                return Page();
            }

            var resultAdd = await _userManager.AddToRolesAsync(user, addRoles);
            if (!resultAdd.Succeeded)
            {
                resultDelete.Errors.ToList().ForEach(e => ModelState.AddModelError(string.Empty, e.Description));
                return Page();
            }

            StatusMessage = $"Tài khoản {user.UserName} vừa được cập nhật Role thành công.";
            return RedirectToPage("../Index", new { id=user.Id});
        }
    }
}
