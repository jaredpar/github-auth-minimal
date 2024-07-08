# GitHub Authentication with Minimal APIs

This is a minimal API demonstration of how to do GitHub OAuth. It's intended to be fairly low level to demonstrate the building blocks necessary for authentication.

Understanding authentication in ASP.NET core is a topic I've struggled with. This repo is an example of one of the ways I approached learning more about it. I'm sharing it as an example in the hopes it will help someone else out. Also in case others want to point out items I'm misssing.

## Local Developement

To test this locally you'll need to create a Github Application 

1. [Create a new GitHub Application](https://github.com/settings/apps/new)
2. Set the _Callback URL_ to `http://localhost:5001/signin-github`
3. Click _Create GitHub App_
4. Use _Client Id_ (not _App Id_) for _ClientId_ below
5. Click _Generate a new client secret_ and use that for _ClientSecret_ below

Then run the following commands in the root of the project:

```cmd
> dotnet user-secrets set ClientId "YOUR_CLIENT_ID"
> dotnet user-secrets set ClientSecret "YOUR_CLIENT_ID"
```

After that you should be able to F5 the project and play around.

## Notes

Here are a few notes on items that really tripped me up when I was working with authentication.

### Schemes

At the core APS.NET authentication is based on schemes. Every scheme has a name that is used to differentiate it from other schemes. The names are arbitrary and can be changed. Can even say add the cookie authentication scheme multiple time using different names.

The below code is setting up the schemes to us in different scenarios. The default scheme is cookies but when a challenge occurs it will use the GitHub scheme.

```csharp
options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
options.DefaultChallengeScheme = GitHubAuthenticationDefaults.AuthenticationScheme;
```

This comes into play with the `/user` endpoint which requires authoritazion. When a non-authenticated user hits this endpoint then the middleware will issue a challenge using the GitHub scheme.

### SignIn

One part I struggled to understand is the flow of data. Essentially how does a successful GitHub authentication persist to a cookie? That roughly translates into the following: 

- Challenge is the authentication action and it's controlled by the `DefaultChallengeSceme` scheme. For OAuth this will send a HTTP 302 to the GitHub login page. When that completes the user is redirected back to the application to the callback path specified in the GitHub application. This will then trigger the `SignIn` action.
- SignIn is _not_ about the act of signing in a user, it's instead about the act of persisting a successful sign in. This is controlled by the `DefaultSignInScheme`. This is commonly a cookie as it's a standard persistence mechanism.

### Accessing GitHub as the User

Once OAuth is working you just need a few tweaks to access GitHub as the user. The first step is to persist the tokens during authentication in the cookies.

```csharp
options.SaveTokens = true;
```

Once that is complete then on any authenticated `HttpContext` the tokens can be accessed via the `GetTokenAsync` method.

```csharp
var token = await context.GetTokenAsync("access_token");
var client = new GitHubClient(new ProductHeaderValue("GitHubAuthMinimal"))
{
    Credentials = new Credentials(token)
};
```

### Good Reads

- [Overview of ASP.NET Core Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication)
- [Overview of the Identity Platform](https://learn.microsoft.com/en-us/entra/fundamentals/identity-fundamental-concepts)
- [The Authorization request details](https://www.oauth.com/oauth2-servers/authorization/the-authorization-request/)
