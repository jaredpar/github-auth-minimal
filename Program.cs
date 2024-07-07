using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GitHubAuthenticationDefaults.AuthenticationScheme;
    )
    .AddCookie()
    .AddGitHub(options =>
    {
        var configuration = builder.Configuration;
        options.ClientId = configuration["ClientId"]!;
        options.ClientSecret = configuration["ClientSecret"]!;
        options.CallbackPath = "/signin-github";
    });

builder.Services.AddAuthorization();
 
var app = builder.Build();
app.MapGet("/", async context =>
{
    if (context.User is { Identity.IsAuthenticated: true } user)
    {
        await context.Response.WriteAsync($"Hello {user.Identity!.Name}");
    }
    else
    {
        await context.Response.WriteAsync("Hello Anonymous");
    }
});

app.MapGet("/login", IResult () =>
    Results.Challenge(new AuthenticationProperties()
    {
        RedirectUri = "/"
    }, [GitHubAuthenticationDefaults.AuthenticationScheme]));

app.MapGet("/logout", IResult () =>
    Results.SignOut(new AuthenticationProperties()
    {
        RedirectUri = "/"
    }));

app.MapGet("/user", async context => 
    await context.Response.WriteAsync($"Hello context.User!.Identity!.Name!"))
    .RequireAuthorization();

app.Run();