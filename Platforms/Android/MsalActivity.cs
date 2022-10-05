using Android.App;
using Android.Content;
using Microsoft.Identity.Client;

namespace Graph6
{
    [Activity(Exported = true)]
    [IntentFilter(new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault },
        DataHost = "auth",
        DataScheme = $"msal{AppConstants.ClientId}")]
    public class MsalActivity : BrowserTabActivity
    {
    }
}