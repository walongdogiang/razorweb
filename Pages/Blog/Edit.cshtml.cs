using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EFWeb.Model;
using EFWeb.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace EFWeb.Pages_Blog
{
    public class EditModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IAuthorizationService _authorizationService;

        public EditModel(AppDbContext context, IAuthorizationService authorizationService)
        {
            _context = context;
            _authorizationService = authorizationService;
        }

        [BindProperty]
        public Article Article { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public QueryHttps queryHttps { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (_context.articles == null) return Content("Lỗi: Article Id rỗng!");
            var article = await _context.articles.FirstOrDefaultAsync(m => m.Id == id);
            if (article == null)
                return Content("Không tìm thấy Article này !");

            Article = article;

            var canupdate = await _authorizationService.AuthorizeAsync(User, Article, "CanUpdateArticle");

            if (canupdate.Succeeded)
                return Page();
            else
                return RedirectToPage("/AccessDenied");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            _context.Attach(Article).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArticleExists(Article.Id))
                    return Content("Not found this article!");
                else
                    throw;
            }
            return RedirectToPage("./Details",
                new { search = queryHttps.SearchBlog, p = queryHttps.CurrentPage, id = Article.Id });
        }

        private bool ArticleExists(int id)
            => (_context.articles?.Any(e => e.Id == id)).GetValueOrDefault();
    }
}
