using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using EFWeb.Model;
using EFWeb.Helpers;
using Microsoft.EntityFrameworkCore;

namespace EFWeb.Pages_Blog
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;

        public CreateModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Article Article { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public QueryHttps queryHttps { get; set; }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || _context.articles == null || Article == null)
                return Page();
            
            _context.Add(Article);  //_context.articles.Add(Article);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Details",
                new { search = queryHttps.SearchBlog, p = queryHttps.CurrentPage, id = Article.Id });
        }
    }
}
