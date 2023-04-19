using Bogus.DataSets;
using Microsoft.AspNetCore.Mvc;

namespace EFWeb.Helpers
{
    public class QueryHttps
    {
        [BindProperty(Name = "p")] // Tìm theo "p"
        public int CurrentPage { get; set; }

        [BindProperty(Name = "search")]
        public string? SearchBlog { get; set; }
    }
}
