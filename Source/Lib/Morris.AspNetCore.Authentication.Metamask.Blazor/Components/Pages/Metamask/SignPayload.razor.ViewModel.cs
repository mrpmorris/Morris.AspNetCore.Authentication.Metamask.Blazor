using System.ComponentModel.DataAnnotations;

namespace Morris.AspNetCore.Authentication.Metamask.Blazor.Components.Pages.Metamask;

public partial class SignPayload
{
    public class ViewModel
    {
        [Required]
        public string Account { get; set; } = null!;

        [Required]
        public string Payload { get; set; } = null!;

        [Required]
        public string Signature { get; set; } = null!;
    }

}