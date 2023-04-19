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

namespace EFWeb.Pages_Blog
{
    public class EditModel : PageModel
    {
        private readonly MyBlogContext _context;

        public EditModel(MyBlogContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Article Article { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public QueryHttps queryHttps { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (_context.articles == null)
                return Content("Error to get data!");

            var article = await _context.articles.FirstOrDefaultAsync(m => m.Id == id);

            if (article == null)
                return Content("Not found this article!");

            Article = article;
            return Page();
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
                new { search = queryHttps.SearchBlog, p = queryHttps.CurrentPage, id = Article.Id});
        }

        private bool ArticleExists(int id)
            => (_context.articles?.Any(e => e.Id == id)).GetValueOrDefault();
    }
}
