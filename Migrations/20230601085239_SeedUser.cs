using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFWeb.Migrations
{
    public partial class SeedUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            for (int i = 1; i <= 150; i++)
            {
                migrationBuilder.InsertData(
                    "Users",
                    columns: new[]
                    {
                        "Id","HomeAddress","UserName","NormalizedUserName","Email","NormalizedEmail","EmailConfirmed",
                        "PasswordHash","SecurityStamp","PhoneNumberConfirmed","TwoFactorEnabled",
                        "LockoutEnabled","AccessFailedCount"
                    },
                    values: new object[]
                    {
                        Guid.NewGuid().ToString(),
                        "...@#%...",
                        "user-"+i.ToString("D3"),
                        $"user-{i.ToString("D3")}".ToUpper(),
                        $"email{i.ToString("D3")}@gmail.com",
                        $"email{i.ToString("D3")}@gmail.com".ToUpper(),
                        true,
                        "123456",
                        Guid.NewGuid().ToString(),
                        false,
                        false,
                        false,
                        0
                    }
                );
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
