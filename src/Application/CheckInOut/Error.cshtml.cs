using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KidsTown.Application.CheckInOut
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ErrorModel : PageModel
    { 
        public string RequestId => string.Empty;

        public bool ShowRequestId => !string.IsNullOrEmpty(value: RequestId);
    }
}