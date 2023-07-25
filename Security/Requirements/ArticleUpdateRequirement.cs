using Microsoft.AspNetCore.Authorization;

namespace EFWeb.Security.Requirements
{
    public class ArticleUpdateRequirement : IAuthorizationRequirement
    {
        public DateTime ExpirationDate { get; set; }

        public ArticleUpdateRequirement()
        {
            ExpirationDate = new DateTime(2021, 06, 01);
        }

    }
}
