using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
// ReSharper disable MemberCanBePrivate.Global

namespace Application.CheckInOut
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ErrorModel : PageModel
    { 
        public string RequestId { get; } = string.Empty;

        public bool ShowRequestId => !string.IsNullOrEmpty(value: RequestId);
    }
}