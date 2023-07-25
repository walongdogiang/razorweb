using Bogus.DataSets;
using EFWeb.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace EFWeb.Areas.Admin.Pages.User.Update
{
    public class UserClaimManageModel : PageModel
    {
        private readonly MyBlogContext _context;
        private readonly UserManager<AppUser> _userManager;

        public UserClaimManageModel(MyBlogContext myBlogContext, UserManager<AppUser> userManager)
        {
            _context = myBlogContext;
            _userManager = userManager;
        }

        [TempData]
        public string? StatusMessage { get; set; }

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

        public AppUser user { get; set; }

        public IdentityUserClaim<string>? userClaim { get; set; }

        public NotFoundObjectResult OnGet() => NotFound("Không được truy cập");
        

        public async Task<IActionResult> OnGetAddClaimAsync(string userid)
        {
            ViewData["Title"] = "Tạo Claim người dùng";
            user = await _userManager.FindByIdAsync(userid);

            if (user == null) return NotFound("Không tìm thấy người dùng");
            return Page();
        }

        public async Task<IActionResult> OnPostAddClaimAsync(string userid)
        {
            user = await _userManager.FindByIdAsync(userid);

            if (user == null) return NotFound("Không tìm thấy người dùng");
            if (!ModelState.IsValid) return Page();

            if (await HasClaim(user, Input.ClaimType, Input.ClaimValue))
            {
                ModelState.AddModelError(string.Empty, $"Claim [{Input.ClaimType} - {Input.ClaimValue}] này đã tồn tại trong tài khoản !");
                return Page();
            }

            await _userManager.AddClaimAsync(user, new(Input.ClaimType, Input.ClaimValue));

            StatusMessage = $"Bạn vừa tạo claim cho tài khoản này thành công !";
            return RedirectToPage("./AddRole", new { userid });
        }

        public async Task<IActionResult> OnGetEditClaimAsync(int? claimid)
        {
            ViewData["Title"] = "Cập nhật Claim người dùng";

            if (claimid == null) return NotFound("Lỗi: Claim Id rỗng");
            userClaim = _context.UserClaims.Where(c => c.Id == claimid).FirstOrDefault();

            if (userClaim == null) return NotFound("Không tìm thấy Claim người dùng này");
            user = await _userManager.FindByIdAsync(userClaim.UserId);
            if (user == null) return NotFound("Không tìm thấy user này");

            Input = new InputModel()
            {
                ClaimType = userClaim.ClaimType,
                ClaimValue = userClaim.ClaimValue,
            };

            return Page();
        }

        public async Task<IActionResult> OnPostEditClaimAsync(int? claimid)
        {
            if (claimid == null) return NotFound("Lỗi: Claim Id rỗng");
            userClaim = _context.UserClaims.Where(c => c.Id == claimid).FirstOrDefault();

            if (userClaim == null) return NotFound("Không tìm thấy Claim người dùng này");
            user = await _userManager.FindByIdAsync(userClaim.UserId);
            if (user == null) return NotFound("Không tìm thấy user này");

            if (!ModelState.IsValid) return Page();

            if ((userClaim.ClaimType, userClaim.ClaimValue) == (Input.ClaimType, Input.ClaimValue)) return RedirectToPage("./AddRole", new { userid = user.Id });


            var b = _context.UserClaims.Any(c => c.UserId == user.Id && c.ClaimType == Input.ClaimType && c.ClaimValue == Input.ClaimValue);

            var a = (await _userManager.GetClaimsAsync(user)).Any(c => c.Type == Input.ClaimType && c.Value == Input.ClaimValue);

            if (await HasClaim(user, Input.ClaimType, Input.ClaimValue))
            {
                ModelState.AddModelError(string.Empty, $"Claim [{Input.ClaimType} - {Input.ClaimValue}] này đã tồn tại trong tài khoản này !");
                return Page();
            }

            userClaim.ClaimType = Input.ClaimType;
            userClaim.ClaimValue = Input.ClaimValue;

            await _context.SaveChangesAsync();
            StatusMessage = $"Bạn vừa cập nhập claim cho tài khoản {user.UserName} .";

            return RedirectToPage("./AddRole", new { userid = user.Id });
        }

        public async Task<IActionResult> OnPostDeleteClaimAsync(int? claimid)
        {
            if (claimid == null) return NotFound("Lỗi: Claim id rỗng !");
            userClaim = _context.UserClaims.Where(c => c.Id == claimid).FirstOrDefault();
            if (userClaim == null) return NotFound("Không tìm thấy Claim này");

            user = await _userManager.FindByIdAsync(userClaim.UserId);
            if (user == null) return NotFound($"Không tìm thấy người dùng !");

            var result = await _userManager.RemoveClaimAsync(user, new Claim(userClaim.ClaimType, userClaim.ClaimValue));
            if (!result.Succeeded)
            {
                result.Errors.ToList().ForEach(error =>
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                });
                return Page();
            }
            StatusMessage = $"Bạn đã xóa Claim: {userClaim.ClaimType} - {userClaim.ClaimValue} khỏi tài khoản này thành công !";
            return RedirectToPage("./AddRole", new { userid = user.Id });
        }

        public async Task<bool> HasClaim(AppUser user, string type, string value)
            => (await _userManager.GetClaimsAsync(user)).Any(c => (c.Type, c.Value) == (type, value));
    }
}
