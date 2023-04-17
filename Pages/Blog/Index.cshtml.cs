using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EFWeb.Model;

namespace EFWeb.Pages_Blog
{
    public class IndexModel : PageModel
    {
        private readonly MyBlogContext _context;

        public IndexModel(MyBlogContext context)
        {
            _context = context;
        }

        public IList<Article> Articles { get; set; } = default!;
        public const int ITEMS_PER_PAGE = 10;
        public int STT { get; set; }

        [BindProperty(SupportsGet = true, Name = "p")] // Tìm theo "p"
        public int currentPage { get; set; }
        public int countPage { get; set; } = 0;

        public async Task OnGetAsync(string searchBlog)
        {
            if (searchBlog != null)
            {
                Articles = await _context.articles.Where(x => x.Title.Contains(searchBlog))
                    .ToListAsync();

            }
            else if (_context.articles != null)
            {
                Articles = await _context.articles.OrderByDescending(x => x.Created)
                    .ToListAsync();
            }

            if (Articles != null)
            {
                countPage = (int)Math.Ceiling((double)Articles.Count() / ITEMS_PER_PAGE);

                // check if currentPage is out of range(1,currentPage)
                currentPage = currentPage < 1 ? 1 : currentPage > countPage ? countPage : currentPage;
                STT = (currentPage - 1) * ITEMS_PER_PAGE + 1;

                Articles = await Task.FromResult(Articles.Skip(STT - 1)
                    .Take(ITEMS_PER_PAGE).ToList());
            }
        }
    }
}
