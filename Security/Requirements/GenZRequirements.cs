using Microsoft.AspNetCore.Authorization;

namespace EFWeb.Security.Requirements
{
    public class GenZRequirements : IAuthorizationRequirement
    {
        public int FromYear { get; set; }
        public int ToYear { get; set; }

        public GenZRequirements(int fromYear = 1997, int toYear = 2012)
        {
            FromYear = fromYear;
            ToYear = toYear;
        }
    }
}
