namespace Morris.AspNetCore.Authentication.Metamask.Blazor;

public static class MetamaskOptionsDefaults
{
    /// <summary>
    /// The default scheme for Metamask authentication. Defaults to <c>Metamask</c>.
    /// </summary>
    public const string AuthenticationScheme = "Metamask";

    /// <summary>
    /// The default path used to handle metamask authentication. Defaults to <c>/signin-metamask</c>.
    /// </summary>
    public const string CallbackPath = "/signin-metamask";

    /// <summary>
    /// The default display name for Metamask authentication. Defaults to <c>Metamask</c>.
    /// </summary>
    public const string DisplayName = "Metamask";

    /// <summary>
    /// The default path to redirect to for signing in via metamask
    /// </summary>
    public const string LocalChallengePath = "/metamask-select-wallet";

    /// <summary>
    /// The default path to redirect to for signing a payload.
    /// </summary>
    public const string LocalSignPayloadPath = "/metamask-sign-payload";

}
