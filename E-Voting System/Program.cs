using E_Voting_System.Data;
using E_Voting_System.Models;
using E_Voting_System.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
// ====================== SERVICES ======================

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = false;
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Email
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailSender, EmailSender>();

// Custom Services
builder.Services.AddScoped<IVoteService, VoteService>();
builder.Services.AddScoped<IOtpService, OtpService>();

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// ====================== MIDDLEWARE ======================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// ====================== SEED DATA ======================

// ====================== SEED DATA ======================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // ----- Create Roles -----
        string[] roles = { "Admin", "ElectionOfficer", "Voter" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // ----- Create Admin User -----
        string adminEmail = "admin@evoting.com";
        string adminPassword = "Admin@12345";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "System Administrator",
                SouthAfricanId = "9001015800085",
                DateOfBirth = new DateTime(1990, 1, 1),
                IsEligibleVoter = true,
                RegisteredAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // ----- Seed Parties (only if none exist) -----
        if (!context.Parties.Any())
        {
            context.Parties.AddRange(
                new Party { Name = "African National Congress", PartyCode = "ANC", IsActive = true },
                new Party { Name = "Democratic Alliance", PartyCode = "DA", IsActive = true },
                new Party { Name = "Economic Freedom Fighters", PartyCode = "EFF", IsActive = true },
                new Party { Name = "Inkatha Freedom Party", PartyCode = "IFP", IsActive = true }
            );
            await context.SaveChangesAsync();
        }

        // ----- Seed one open Election -----
        if (!context.Elections.Any())
        {
            context.Elections.Add(new Election
            {
                Name = "2026 National & Provincial Elections",
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(30),
                IsOpen = true,
                ResultsPublished = false
            });
            await context.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}