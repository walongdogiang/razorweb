using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EFWeb.Models;
using EFWeb.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace EFWeb.Pages_Blog
{
    [Authorize(Policy = "InGenZ")]
    public class DetailsModel : PageModel
    {
        private readonly AppDbContext _context;

        [BindProperty(SupportsGet = true)]
        public QueryHttps queryHttps { get; set; }

        public DetailsModel(AppDbContext context)
        {
            _context = context;
        }

      public Article Article { get; set; } = default!; 

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.articles == null)
            {
                return NotFound();
            }

            var article = await _context.articles.FirstOrDefaultAsync(m => m.Id == id);
            if (article == null)
            {
                return NotFound();
            }
            else 
            {
                Article = article;
            }
            return Page();
        }
    }
}
