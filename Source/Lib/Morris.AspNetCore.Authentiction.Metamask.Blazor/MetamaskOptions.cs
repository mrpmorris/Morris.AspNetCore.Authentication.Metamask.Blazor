using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Morris.AspNetCore.Authentication.Metamask.Blazor;

public class MetamaskOptions : RemoteAuthenticationOptions
{
    public PathString LocalChallengePath { get; set; }
    public PathString LocalSignPayloadPath { get; set; }

    public MetamaskOptions()
    {
        CallbackPath = new PathString(MetamaskOptionsDefaults.CallbackPath);
        LocalChallengePath = new PathString(MetamaskOptionsDefaults.LocalChallengePath);
        LocalSignPayloadPath = new PathString(MetamaskOptionsDefaults.LocalSignPayloadPath);
    }

    public override void Validate()
    {
        base.Validate();
        if (LocalChallengePath == null)
            throw new ArgumentException(
                paramName: nameof(LocalChallengePath),
                message: "Required.");
    }
}
