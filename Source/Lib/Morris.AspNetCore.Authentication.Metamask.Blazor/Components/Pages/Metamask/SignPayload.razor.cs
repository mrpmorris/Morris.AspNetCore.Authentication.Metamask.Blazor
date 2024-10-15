using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Morris.AspNetCore.Authentication.Metamask.Blazor.Components.Pages.Metamask;

public partial class SignPayload
{
    [SupplyParameterFromQuery(Name = "Account")]
    public string? Account { get; set; }

    [SupplyParameterFromForm]
    public required ViewModel Model { get; set; } = null!;

    [SupplyParameterFromQuery(Name = MetamaskHandler.StateKey)]
    public required string State { get; set; } = null!;

    private EditForm EditForm = null!;
    private bool ShouldRedirect;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        Model ??= new();
        Model.Account ??= Account ?? throw new ArgumentNullException(nameof(Account));
        Model.Payload ??= MetamaskHandler.GetPayloadToSign(Account);
    }

    private void SaveAsync()
    {
        bool isValid = EditForm.EditContext!.Validate();
        if (!isValid)
            return;

        ShouldRedirect = true;
    }

}