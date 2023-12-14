using BNP.SCG.Web.Models;
using BNP.SCG.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var _config = builder.Services.AddDbContext<ApplicationContext>(opts => opts.UseSqlServer(builder.Configuration.GetConnectionString("DBConnection")));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
builder.Services.AddTransient<IDatabaseSqlConnectionFactory>(e =>
{
    return new SqlConnectionFactory(builder.Configuration.GetConnectionString("DBConnection"));
});
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddScoped(typeof(FulfillService));
builder.Services.AddScoped(typeof(AccountService));
builder.Services.AddScoped(typeof(MasterDataService));
builder.Services.AddScoped(typeof(ROPService));
builder.Services.AddScoped(typeof(SystemConfigService));
builder.Services.AddScoped(typeof(InfoService));
builder.Services.AddScoped(typeof(GateService));
builder.Services.AddScoped(typeof(CustomerService));


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.Cookie.Name = "BNP.SCG";
        option.LoginPath = "/Account/Login";
        option.AccessDeniedPath = "/Account/AccessDenied";
        option.SlidingExpiration = true;
        // option.ExpireTimeSpan = TimeSpan.FromDays(1);
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
