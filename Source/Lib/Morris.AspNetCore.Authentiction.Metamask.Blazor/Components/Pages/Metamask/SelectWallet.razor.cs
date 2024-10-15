using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

namespace Morris.AspNetCore.Authentication.Metamask.Blazor.Components.Pages.Metamask;

public partial class SelectWallet
{
    [SupplyParameterFromQuery(Name = "Accounts")]
    public string? QuerystringAccounts { get; set; }

    [SupplyParameterFromForm]
    public required ViewModel Model { get; set; } = null!;

    [SupplyParameterFromQuery(Name = MetamaskHandler.StateKey)]
    public required string State { get; set; } = null!;

    private string[] Accounts = [];
    private EditForm EditForm = null!;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (!string.IsNullOrWhiteSpace(QuerystringAccounts))
            Accounts = QuerystringAccounts.Split(',');
        Model ??= new();
    }
    private void Save()
    {
        bool isValid = EditForm.EditContext!.Validate();
        if (!isValid)
            return;

        var queryString = new QueryString();
        queryString = queryString.Add("account", Model.Account);
        queryString = queryString.Add(MetamaskHandler.StateKey, State ?? "");
        NavigationManager.NavigateTo(Options.Value.LocalSignPayloadPath + queryString);
    }
}