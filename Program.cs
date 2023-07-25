using EFWeb.Areas.Admin.Pages.Role;
using EFWeb.Model;
using EFWeb.Security.Requirements;
using EFWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Security.Policy;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddOptions();
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddSingleton<IEmailSender, SendMailService>();

builder.Services.AddRazorPages();
builder.Services.AddDbContext<MyBlogContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyBlogContext"));
});

builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<MyBlogContext>()
    .AddDefaultTokenProviders();


builder.Services.Configure<IdentityOptions>(options =>
{
    // Thiết lập về Password
    options.Password.RequireDigit = false; // Không bắt phải có số
    options.Password.RequireLowercase = false; // Không bắt phải có chữ thường
    options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
    options.Password.RequireUppercase = false; // Không bắt buộc chữ in
    options.Password.RequiredLength = 3; // Số ký tự tối thiểu của password
    options.Password.RequiredUniqueChars = 1; // Số ký tự riêng biệt

    // Cấu hình Lockout - khóa user
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1); // Khóa 5 phút
    options.Lockout.MaxFailedAccessAttempts = 3; // Thất bại 5 lần thì khóa
    options.Lockout.AllowedForNewUsers = true;

    // Cấu hình về User.
    options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;  // Email là duy nhất

    // Cấu hình đăng nhập.
    options.SignIn.RequireConfirmedEmail = true;            // Cấu hình xác thực địa chỉ email (email phải tồn tại và của người đó)
    options.SignIn.RequireConfirmedPhoneNumber = false;     // Xác thực số điện thoại
    options.SignIn.RequireConfirmedAccount = true;         // Xác thực tài khoản sau khi đký

});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login";
    options.LogoutPath = "/Register";
    options.AccessDeniedPath = "/AccessDenied";
});

builder.Services.AddAuthentication()
    .AddGoogle(googleOptions =>
    {
        // Đọc thông tin Authentication:Google từ appsettings.json
        IConfigurationSection gconfig = builder.Configuration.GetSection("Authentication:Google");

        // Thiết lập ClientID và ClientSecret để truy cập API google
        googleOptions.ClientId = gconfig["ClientId"];
        googleOptions.ClientSecret = gconfig["ClientSecret"];

        // Cấu hình Url callback lại từ Google (không thiết lập thì mặc định là /signin-google)
        googleOptions.CallbackPath = "/dang-nhap-tu-google";
    })
    .AddFacebook(facebookOptions => {
        // Đọc cấu hình
        IConfigurationSection facebookAuthNSection = builder.Configuration.GetSection("Authentication:Facebook");
        facebookOptions.AppId = facebookAuthNSection["AppId"];
        facebookOptions.AppSecret = facebookAuthNSection["AppSecret"];
        // Thiết lập đường dẫn Facebook chuyển hướng đến
        facebookOptions.CallbackPath = "/dang-nhap-tu-facebook";
    });

builder.Services.AddSingleton<IdentityErrorDescriber, AppIdentityErrorDescriber>();

builder.Services.AddAuthorization(options => {
    options.AddPolicy("AllowEditRole", policyBuilder =>
    {
        policyBuilder.RequireAuthenticatedUser();
        policyBuilder.RequireClaim("full-service", "full");
    });

    options.AddPolicy("InGenZ", policyBuilder =>
    {
        policyBuilder.RequireAuthenticatedUser();
        policyBuilder.Requirements.Add(new GenZRequirements());
    });

    options.AddPolicy("ShowAdminMenu", policyBuilder =>
    {
        policyBuilder.RequireRole("Admin");
    });

    options.AddPolicy("CanUpdateArticle", policyBuilder =>
    {
        policyBuilder.RequireAuthenticatedUser();
        policyBuilder.Requirements.Add(new ArticleUpdateRequirement());
    });
});

builder.Services.AddTransient<IAuthorizationHandler, AppAuthorizationHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();

