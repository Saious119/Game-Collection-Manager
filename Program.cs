using Microsoft.AspNetCore.Identity.UI.Services;
using WebPWrecover.Services;
using Microsoft.AspNetCore.Authentication;
using System.Globalization;
using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication.Cookies;
using GameCollectionManager.Services;


//using Microsoft.AspNetCore.Authentication.Negotiate;

var builder = WebApplication.CreateBuilder(args);

//var keyVaultEndpoint = new Uri(Environment.GetEnvironmentVariable("VaultUri"));
//builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());
var config = builder.Configuration;


// Add services to the container.
/*var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();*/

builder.Services.AddHttpClient();

builder.Services.AddDistributedMemoryCache();
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // Set the cookie policy options here
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
});

builder.Services.AddSession(options =>
{
    options.Cookie.Name = "DiscordInfo";
    options.IdleTimeout = TimeSpan.FromDays(1);
    options.Cookie.IsEssential = true;
});

var httpContext = new DefaultHttpContext();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<DiscordService>();
DiscordConfigReader DiscordConfigReader = new DiscordConfigReader();
DiscordConfigReader.Init();
string discordID = "Not Logged In";
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = DiscordAuthenticationDefaults.AuthenticationScheme;
})
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    })
    .AddDiscord(options =>
    {
        options.ClientId = DiscordConfigReader.clientID;
        options.ClientSecret = DiscordConfigReader.clientSecret;
        options.CallbackPath = new PathString("/signin-discord");
        options.Scope.Add("identify");
        options.SaveTokens = true;

        options.Events.OnTicketReceived = async context =>
        {
            //Console.WriteLine(context.Principal.Identity.Name + context.Principal.Identities.FirstOrDefault().Claims.FirstOrDefault(c => c.Type == "urn:discord:user:discriminator").Value);
            var discordId = context.Principal.Identity.Name;// + "#" + context.Principal.Identities.FirstOrDefault().Claims.FirstOrDefault(c => c.Type == "urn:discord:user:discriminator").Value;
            // save the discordId to your user database

            if (discordId != null)
            {
                var httpContextAccessor = new HttpContextAccessor();
                Console.WriteLine("Discord ID  = {0}", discordId.ToString());
                DiscordService.userDiscordID = discordId;
                discordID = discordId.ToString();
                httpContextAccessor.HttpContext.Session.SetString("DiscordUserId", discordID);
            }
            else
            {
                Console.WriteLine("Discord ID is null");
            }
        };

        options.ClaimActions.MapCustomJson("urn:discord:avatar:url", user =>
            string.Format(
                CultureInfo.InvariantCulture,
                "https://cdn.discordapp.com/avatars/{0}/{1}.{2}",
                user.GetString("id"),
                user.GetString("avatar"),
                user.GetString("avatar").StartsWith("a_") ? "gif" : "png"));
        options.AccessDeniedPath = "/Account/AccessDenied";
    });


builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
//    .AddMicrosoftIdentityConsentHandler();
builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration);
builder.Services.AddLogging();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCookiePolicy();
app.UseSession();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", async (context) =>
{
    if (context.User.Identity?.IsAuthenticated != true)
    {
        await context.ChallengeAsync("Discord");
    }
    else
    {
        //await context.Response.WriteAsync("Hello, authenticated user " + DiscordService.userDiscordID+ "!");
        //context.Session.SetString("DiscordUserId", discordID);
        context.Response.Redirect("/home");
    }
});

app.Map("/signin-discord", signin =>
{
    signin.Run(async context =>
    {
        var authResult = await context.AuthenticateAsync("Discord");
        if (authResult.Succeeded)
        {
            context.Session.SetString("DiscordUserId", discordID);
            context.SignInAsync(authResult.Principal);
            context.Response.Redirect("/home");
        }
        else
        {
            await context.Response.WriteAsync("Authentication failed");
        }
    });
});

app.Map("/signout", signout =>
{
    signout.Run(context =>
    {
        context.Session.Clear();
        context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        context.Response.Redirect("/");
        return Task.CompletedTask;
    });
});

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

//DiscordService.SetSession(discordID);

app.Run();