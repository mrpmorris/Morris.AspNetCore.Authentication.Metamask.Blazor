# Morris.AspNetCore.Authentication.Metamask.Blazor
![](./Images/logo.png)

Metamask authentication for Blazor applications.

1. Create a new Blazor web app with authentication.
2. In Program.cs add `using Morris.AspNetCore.Authentication.Metamask.Blazor;`
3. Find the following code
```c#
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();
```
and add `.AddMetamask()`
```c#
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddMetamask()
    .AddIdentityCookies();
```
4. Find the following code
```c#
app.MapRazorComponents<App>()
```
and add the following additional assembly
```c#
app.MapRazorComponents<App>()
    .AddAdditionalAssemblies(typeof(MetamaskOptions).Assembly)
```
5. Edit `Routes.razor` (or wherever your `<Router>` component is,
and register additional assemblies
```razor
<Router AppAssembly="typeof(Program).Assembly">
```
change to
```razor
@using Morris.AspNetCore.Authentication.Metamask.Blazor
<Router AppAssembly="typeof(Program).Assembly" AdditionalAssemblies=@([typeof(MetamaskOptions).Assembly])>
```
6. Edit your `App.razor` and add the required javascript reference after the `Blazor` javascript reference.
```html
<script src="_content/Morris.AspNetCore.Authentication.Metamask.Blazor/scripts/index.js"></script>
```