using System.ComponentModel.DataAnnotations;

namespace Morris.AspNetCore.Authentication.Metamask.Blazor.Components.Pages.Metamask;

public partial class SelectWallet
{
    public class ViewModel
    {
        [Required, RegularExpression(@"^0x[a-fA-F0-9]{40}$", ErrorMessage = "Invalid account number")]
        public string Account { get; set; } = null!;
    }
}