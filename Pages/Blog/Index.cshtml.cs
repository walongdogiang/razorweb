using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFWeb.Helpers;
using EFWeb.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;


namespace EFWeb.Pages_Blog
{
    [Authorize(Policy = "AllowEditRole")]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<Article> Articles { get; set; }
        public const int ITEMS_PER_PAGE = 10;
        public int STT { get; set; }

        [BindProperty(SupportsGet = true)]
        public QueryHttps queryHttps { get; set; }

        public int countPage { get; set; } = 0;

        public async Task OnGetAsync()
        {
            if (queryHttps.SearchBlog != null)
            {
                Articles = await _context.articles.Where(x => x.Title.Contains(queryHttps.SearchBlog))
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
                queryHttps.CurrentPage = queryHttps.CurrentPage < 1 ? 1 
                    : queryHttps.CurrentPage > countPage ? countPage : queryHttps.CurrentPage;

                STT = (queryHttps.CurrentPage - 1) * ITEMS_PER_PAGE + 1;

                Articles = await Task.FromResult(Articles.Skip(STT - 1)
                    .Take(ITEMS_PER_PAGE).ToList());
            }
        }
    }
}
