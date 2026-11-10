using LibraryMvcApp.Data;
using LibraryMvcApp.Services;
using LibraryMvcApp.Services.Implementations;
using LibraryMvcApp.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Syncfusion.Licensing;

var builder = WebApplication.CreateBuilder(args);
//builder.Services.AddSingleton<Microsoft.Office.Interop.Word.Application>();

// Add services to the container.
builder.Services.AddControllersWithViews(o => o.Filters.Add<LibraryMvcApp.Helpers.ReadOnlyRoleFilter>());

// Performance: gzip/brotli for HTML, JSON, CSS and JS
builder.Services.AddResponseCompression(o =>
{
    o.EnableForHttps = true;
    o.Providers.Add<BrotliCompressionProvider>();
    o.Providers.Add<GzipCompressionProvider>();
});
builder.Services.AddSingleton<UploadStorage>();
builder.Services.AddScoped<IFolderPermissionService, FolderPermissionService>();
builder.Services.AddScoped<IFormRegisterService, FormRegisterService>();
builder.Services.AddScoped<IPdfFormExtractor, PdfFormExtractor>();

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Session (لو عايز تستخدمه)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await IdentitySeeder.SeedRolesAsync(roleManager);
}
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseExceptionHandler("/Error");

app.UseStatusCodePagesWithReExecute(
    "/Error",
    "?message=الصفحة غير موجودة"
);

app.UseResponseCompression();

// Performance: browsers re-download nothing that has not changed.
//  - /uploads files have GUID names, so they never change -> cache for 7 days
//  - css/js/lib are versioned with asp-append-version -> cache for 30 days
var staticOptions = new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        var path = ctx.Context.Request.Path;
        var days = path.StartsWithSegments("/uploads") ? 7
                 : path.StartsWithSegments("/lib") || path.StartsWithSegments("/css") || path.StartsWithSegments("/js") ? 30
                 : 1;
        ctx.Context.Response.Headers["Cache-Control"] = $"public,max-age={days * 86400}";
    }
};

// Uploaded books / covers live outside the project (Storage:UploadsPath, e.g. D:\LibraryFiles)
// and are still served at /uploads/... ; files missing there fall through to wwwroot\uploads.
var uploadStorage = app.Services.GetRequiredService<UploadStorage>();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadStorage.Root),
    RequestPath = "/uploads",
    OnPrepareResponse = staticOptions.OnPrepareResponse
});

app.UseStaticFiles(staticOptions);

app.UseRouting();

// ⭐ مهم جدًا  (Authentication قبل Authorization)
app.UseAuthentication();
app.UseAuthorization();

// Session
app.UseSession();

// Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Folder}/{action=Index}/{id?}");
async Task CreateDefaultAdmin(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Add Roles
    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole("Admin"));

    if (!await roleManager.RoleExistsAsync("User"))
        await roleManager.CreateAsync(new IdentityRole("User"));

    // Create Admin
    var adminEmail = "admin@admin.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        await userManager.CreateAsync(adminUser, "Admin123!");
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}

await CreateDefaultAdmin(app);
app.Run();
