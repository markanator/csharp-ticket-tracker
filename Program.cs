using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TheBugTracker.Data;
using TheBugTracker.Models;
using TheBugTracker.Services;
using TheBugTracker.Services.Factories;
using TheBugTracker.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// STARTUP STUFF
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(DataUtils.GetConnectionString(builder.Configuration),
                                                    // enable splitting of queries to improve perf, & prevent redundant data fetching
                                                    o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Adding Custom Auth + Roles
builder.Services.AddIdentity<BTUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddClaimsPrincipalFactory<BTUserClaimsPrincipleFactory>() // used to attach companyId to identity
        .AddDefaultUI()
        .AddDefaultTokenProviders();

// add custom services
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ICompanyInfoService, CompanyInfoService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<ITicketHistoryService, TicketHistoryService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IInviteService, InviteService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<ILookupService, LookupService>();

builder.Services.AddScoped<IEmailSender, EmailService>();
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

builder.Services.AddControllersWithViews();

var app = builder.Build();
// SEED
await DataUtils.ManageDataAsync(app);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
} else {
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
;
app.MapRazorPages();

app.Run();
