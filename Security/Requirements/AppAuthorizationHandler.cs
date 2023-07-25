using EFWeb.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace EFWeb.Security.Requirements
{
    public class AppAuthorizationHandler : IAuthorizationHandler
    {
        private readonly ILogger<AppAuthorizationHandler> _logger;
        private readonly UserManager<AppUser> _userManager;

        public AppAuthorizationHandler(ILogger<AppAuthorizationHandler> logger, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            var requirements = context.PendingRequirements.ToList();

            foreach (var requirement in requirements)
            {
                if(requirement is GenZRequirements)
                {
                    if (IsGenZ(context.User, (GenZRequirements)requirement))
                        context.Succeed(requirement);
                }
                else if(requirement is ArticleUpdateRequirement)
                {
                    if(CanUpdateArticle(context.User, context.Resource, (ArticleUpdateRequirement)requirement))
                        context.Succeed(requirement);
                }
            }
            return Task.CompletedTask;
        }

        private bool IsGenZ(ClaimsPrincipal user, GenZRequirements requirement)
        {
            var appUserTask = _userManager.GetUserAsync(user);
            Task.WaitAll(appUserTask);
            var appUser = appUserTask.Result;

            if (appUser.BirthDate == null)
            {
                _logger.LogInformation($"{appUser.UserName} chưa nhập ngày sinh.");
                return false;
            }

            int year = appUser.BirthDate.Value.Year;

            return year >= requirement.FromYear && year <= requirement.ToYear;
        }

        private bool CanUpdateArticle(ClaimsPrincipal user, object resource, ArticleUpdateRequirement requirement)
        {
            if (user.IsInRole("Admin"))
            {
                _logger.LogInformation("Cập nhật với vai trò Admin ! ");
                return true;
            }

            var article = resource as Article;
            var dateCreated = article.Created;
            var dateCanUpdate = requirement.ExpirationDate;

            if (dateCreated < dateCanUpdate)
            {
                _logger.LogInformation("Quá ngày cập nhật");
                return false;
            }
            return true;
        }
    }
}
