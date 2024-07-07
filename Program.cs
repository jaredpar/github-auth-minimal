using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Octokit;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GitHubAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddGitHub(options =>
    {
        var configuration = builder.Configuration;
        options.ClientId = configuration["ClientId"]!;
        options.ClientSecret = configuration["ClientSecret"]!;
        options.CallbackPath = "/signin-github";
        options.SaveTokens = true;
    });

builder.Services.AddAuthorization();
 
var app = builder.Build();
app.MapGet("/", async context =>
{
    var name = context.User is { Identity.IsAuthenticated: true } user ? user.Identity.Name : "Anonymous";
    await context.Response.WriteAsync($"""
        <div>Hello {name}</div>
        <div><a href="/login">Login</a></div>
        <div><a href="/logout">Logout</a></div>
        <div><a href="/user">User Information</a></div>
        <div><a href="/github">Use GitHub Client</a></div>
        """);
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
    await context.Response.WriteAsync($"Hello {context.User!.Identity!.Name}"))
    .RequireAuthorization();


app.MapGet("/github", async context => 
{
    var token = await context.GetTokenAsync("access_token");
    var client = new GitHubClient(new ProductHeaderValue("GitHubAuthMinimal"))
    {
        Credentials = new Credentials(token)
    };

    var repoCount = (await client.Repository.GetAllForCurrent()).Count;
    await context.Response.WriteAsync($"Hello {context.User!.Identity!.Name}, you have {repoCount} repositories!");
}).RequireAuthorization();

app.Run();