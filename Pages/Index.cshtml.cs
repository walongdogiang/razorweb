using EFWeb.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EFWeb.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        private readonly MyBlogContext _myBlogContext;

        public IndexModel(ILogger<IndexModel> logger, MyBlogContext myBlogContext)
        {
            _logger = logger;
            _myBlogContext = myBlogContext;
        }

        public void OnGet()
        {
            var posts = _myBlogContext.articles.OrderByDescending(a => a.Created).ToList();
            ViewData["posts"] = posts;
        }
    }
}