using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using EFWeb.Model;
using Microsoft.AspNetCore.Authorization;
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
        private readonly AppDbContext _context;

        public AddRoleModel(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public AppUser user { get; set; }

        [BindProperty]
        [DisplayName("Các roles của người dùng")]
        public string[] userRolesName { get; set; }

        public SelectList allRoles { get; set; }
        public List<IdentityRoleClaim<string>> roleClaims { get; set; }
        public List<IdentityUserClaim<string>> userClaims { get; set; }

        public async Task<IActionResult> OnGetAsync(string userid)
        {
            if (string.IsNullOrEmpty(userid)) return NotFound("Không tìm thấy tài khoản với id rỗng.");

            user = await _userManager.FindByIdAsync(userid);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userid}'.");
            }

            userRolesName = (await _userManager.GetRolesAsync(user)).ToArray();

            List<string> rolesName = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            allRoles = new SelectList(rolesName);

            await GetUserClaims(userid);

            return Page();
        }

        public async Task GetUserClaims(string userid)
        {
            // lấy ra các đối tượng Roles của user này
            // != biến userRoles ở trên chỉ lấy mỗi Name của Role, còn biến này lấy 1 đối tượng Roles hoàn chỉnh - tất cả info của Roles
            var objUserRoles = _context.Roles.Join(_context.UserRoles.Where(ur => ur.UserId == userid), r => r.Id, ur => ur.RoleId, (r, ur) => r);

            // Lấy ra các claims mà user này có (user có các claims của các roles đã đc gán)
            roleClaims = await _context.RoleClaims.Join(objUserRoles, rc => rc.RoleId, ur => ur.Id, (rc, ur) => rc).ToListAsync();
            
            userClaims = await _context.UserClaims.Where(c => c.UserId == userid).ToListAsync();

        }

        public async Task<IActionResult> OnPostAsync(string userid)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (string.IsNullOrEmpty(userid)) return NotFound("Không thể tìm tải khoản với id bỏ trống.");

            user = await _userManager.FindByIdAsync(userid);
            if (user == null)
            {
                return NotFound($"Không tìm thấy tài khoản có ID '{userid}'.");
            }

            var oldRoles = (await _userManager.GetRolesAsync(user)).ToArray();

            var deleteRoles = oldRoles.Where(r => !userRolesName.Contains(r));
            var addRoles = userRolesName.Where(r => !oldRoles.Contains(r));

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
            return RedirectToPage("../Index", new { userid = user.Id});
        }
    }
}
