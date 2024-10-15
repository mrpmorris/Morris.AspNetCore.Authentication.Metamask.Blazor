using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Morris.AspNetCore.Authentication.Metamask.Blazor;

public sealed class MetamaskHandler : RemoteAuthenticationHandler<MetamaskOptions>
{
    private const string ActionKey = "Action";
    private const string ActionNameLocalChallenge = "LocalChallenge";
    private const string AntiForgeryTokenKey = ".xsrf";
    private const string RedirectKey = ".redirect";
    public const string StateKey = "State";
    private const string UserIdKey = "XsrfId";

    private readonly IDataProtectionProvider DataProtectionProvider;
    private readonly IHttpContextAccessor HttpContextAccessor;

    public MetamaskHandler(
        IOptionsMonitor<MetamaskOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IDataProtectionProvider dataProtectionProvider,
        IHttpContextAccessor httpContextAccessor) 
        : base(options, logger, encoder, clock)
    {
        DataProtectionProvider = dataProtectionProvider ?? throw new ArgumentNullException(nameof(dataProtectionProvider));
        HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    internal string GetPayloadToSign(string? account)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(account);
        Uri hostUri = GetHostUri();
        var challengeContent = new ChallengeContent(
            Host: hostUri,
            ExpiresAfter: Clock.UtcNow.AddMinutes(2),
            Account: account);

        return Protect(challengeContent);
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        if (string.IsNullOrWhiteSpace(properties.RedirectUri))
            if (properties.Items.TryGetValue(".redirect", out string? redirect))
                properties.RedirectUri = redirect;
            else
                properties.RedirectUri = OriginalPathBase + OriginalPath + Request.QueryString;

        string challengeUrl = BuildChallengeUrl(properties);
        Context.Response.Redirect(challengeUrl);
        return Task.CompletedTask;
    }

    protected override async Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
    {
        if (Context.Request.Query[ActionKey] == ActionNameLocalChallenge)
            return HandleRequestResult.SkipHandler();

        IFormCollection form = await Context.Request.ReadFormAsync();
        if (!form.TryGetValue("account", out StringValues account))
            return HandleRequestResult.Fail("Account is required.");
        if (!form.TryGetValue("payload", out StringValues payload))
            return HandleRequestResult.Fail("Payload is required.");
        if (!form.TryGetValue("signature", out StringValues signature))
            return HandleRequestResult.Fail("Signature required.");
        if (!form.TryGetValue("state", out StringValues state))
            return HandleRequestResult.Fail("State is invalid.");
        if (!TryUnprotect(state!, out Dictionary<string, string?>? stateDictionary))
            return HandleRequestResult.Fail("State is invalid.");

        bool payloadIsValid = PayloadIsValid(
            account: account!,
            payload: payload!);
        if (!payloadIsValid)
            return HandleRequestResult.Fail("Invalid payload.");

        bool signedBySameAccount = 
            PayloadWasSignedByAccount(
                account: account,
                payload: payload,
                signature: signature);
        if (!signedBySameAccount)
            return HandleRequestResult.Fail("Invalid signature.");

        var authProperties = new AuthenticationProperties(stateDictionary.ToDictionary());
        authProperties.Items["LoginProvider"] = Scheme.Name;

        if (!ValidateCorrelationId(authProperties))
            return HandleRequestResult.Fail("Invalid correlation id.");

        var authIdClaim = new Claim(
            type: ClaimTypes.NameIdentifier,
            value: account!);
        var authIdentity = new ClaimsIdentity(
            claims: [authIdClaim],
            authenticationType: Scheme.Name);
        var authPrincipal = new ClaimsPrincipal(authIdentity);

        var authTicket = new AuthenticationTicket(
            principal: authPrincipal,
            properties: authProperties,
            authenticationScheme: Scheme.Name);

        return HandleRequestResult.Success(authTicket);
    }

    private IDataProtector GetDataProtector() =>
        DataProtectionProvider.CreateProtector("Metamask");
    
    private Uri GetHostUri()
    {
        var request = HttpContextAccessor.HttpContext.Request;
        Uri hostUri = new Uri($"{request.Scheme}://{request.Host}");
        return hostUri;
    }

    private bool PayloadIsValid(string account, string payload)
    {
        if (!TryUnprotect(payload, out ChallengeContent? challengeContent))
            return false;

        if (challengeContent.HasExpired(Clock))
            return false;

        Uri hostUri = GetHostUri();
        if (hostUri != challengeContent.Host)
            return false;

        if (challengeContent.Account != account)
            return false;

        return true;
    }

    private static bool PayloadWasSignedByAccount(StringValues account, StringValues payload, StringValues signature)
    {
        bool signedBySameAccount;
        try
        {
            var signer = new Nethereum.Signer.EthereumMessageSigner();
            string signingWalletAddress = signer.EncodeUTF8AndEcRecover(payload, signature);

            signedBySameAccount = signingWalletAddress.Equals(account, StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            signedBySameAccount = false;
        }

        return signedBySameAccount;
    }

    private string BuildChallengeUrl(AuthenticationProperties properties)
    {
        GenerateCorrelationId(properties);

        var stateDictionary = new Dictionary<string, string?>();

        if (properties.Items.TryGetValue(UserIdKey, out string? userId))
            stateDictionary[UserIdKey] = userId;

        if (properties.Items.TryGetValue(AntiForgeryTokenKey, out string? antiForgeryToken))
            stateDictionary[AntiForgeryTokenKey] = antiForgeryToken;

        stateDictionary[RedirectKey] = properties.RedirectUri;
        string protectedState = Protect(stateDictionary);

        var queryStringDictionary = new Dictionary<string, string?>();
        queryStringDictionary[ActionKey] = ActionNameLocalChallenge;
        queryStringDictionary[StateKey] = protectedState;

        string result = QueryHelpers.AddQueryString(
            uri: Options.LocalChallengePath,
            queryString: queryStringDictionary);
        return result;
    }

    private string Protect<T>(T state)
        where T : class
    {
        string stateJson = JsonSerializer.Serialize(state)!;
        string protectedState = GetDataProtector().Protect(stateJson);
        return protectedState;
    }

    private bool TryUnprotect<T>(
        string protectedString,
        [NotNullWhen(true)]
        out T? result)
        where T : class
    {
        result = null!;
        if (protectedString is null)
            return false;

        try
        {
            string stateJson = GetDataProtector().Unprotect(protectedString);
            result = JsonSerializer.Deserialize<T>(stateJson)!;
            return true;
        }
        catch (CryptographicException)
        {
            return false;
        }
    }


}
