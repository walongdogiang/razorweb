// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using EFWeb.Model;
using EFWeb.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EFWeb.Areas.Admin.Pages.User.Update
{
    [Authorize]
    public class IndexModel : UserPageModel
    {
        public IndexModel(UserManager<AppUser> userManager) : base(userManager) { }


        [TempData]
        public string StatusMessage { get; set; }


        public AppUser user { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound("Không tìm thấy người dùng.");

            user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound($"Không thể tải lên tài khoản với ID '{id}'.");
            }

            return Page();
        }

        public void OnPost()
        {
        }

    }
}
