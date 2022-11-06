using Microsoft.Graph.Models;
using Microsoft.Identity.Client;
using System.Diagnostics;
using Azure.Identity;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Graph;
using Microsoft.Identity.Client.Extensions.Msal;

namespace Graph6;

public partial class MainPage : ContentPage
{
    int count = 0;
    private User user;
    public MainPage()
    {
        InitializeComponent();
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        count++;

        if (count == 1)
            CounterBtn.Text = $"Clicked {count} time";
        else
            CounterBtn.Text = $"Clicked {count} times";

        SemanticScreenReader.Announce(CounterBtn.Text);
    }
    private async void GetUserInfoBtn_Clicked(object sender, EventArgs e)
    {
#if false
        // using Azure.Identity; 
        // does not work on Android
        var options = new InteractiveBrowserCredentialOptions
        {
            TenantId = AppConstants.TenantId,
            ClientId = AppConstants.ClientId,
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
            // MUST be http://localhost or http://localhost:PORT
            // See https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/System-Browser-on-.Net-Core
            RedirectUri = new Uri("http://localhost"),
        };

        // https://learn.microsoft.com/dotnet/api/azure.identity.interactivebrowsercredential
        var interactiveCredential = new InteractiveBrowserCredential(options);

        var graphClient = new GraphServiceClient(interactiveCredential, AppConstants.Scopes);
#endif

        var authenticationProvider = new BaseBearerTokenAuthenticationProvider(new TokenProvider());
        var graphClient = new GraphServiceClient(authenticationProvider);

        try
        {
            user = await graphClient.Me.GetAsync();
            var x = await graphClient.Me.Drive.GetAsync(); // graphClient.Me.Drive.Items.GetAsync(); does not seem to compile
            Debug.WriteLine(">>> graphClient returned " + x.GetType());
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Exception:" + ex);
        }

        if (user == null)
            HelloLabel.Text = "User information could not be found";
        else
            HelloLabel.Text = $"Hello, {user.DisplayName}!";
    }

    public class TokenProvider : IAccessTokenProvider
    {

        // Using Microsoft.Identity.Client
        private readonly IPublicClientApplication publicClientApplication;

        public TokenProvider()
        {
            publicClientApplication = PublicClientApplicationBuilder.Create(AppConstants.ClientId)
                .WithDefaultRedirectUri()
                .Build();
        }

#if WINDOWS
        private bool once = true;
#endif
        public async Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object> additionalAuthenticationContext = default, CancellationToken cancellationToken = default)
        {
#if WINDOWS
            if (once)
            {
                once = false;
                var storageProperties =
                     new StorageCreationPropertiesBuilder(@"TokenCache", @"C:\Temp")
                     .Build();

                // This hooks up the cross-platform cache into MSAL
                var cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties);
                cacheHelper.RegisterCache(publicClientApplication.UserTokenCache);
            }
#endif
            try
            {
                var accounts = await publicClientApplication.GetAccountsAsync();
                var authenticationResult = await publicClientApplication.AcquireTokenSilent(AppConstants.Scopes, accounts.FirstOrDefault()).ExecuteAsync();

                // get the token and return it in your own way
                return authenticationResult.AccessToken;
            }
            catch (MsalUiRequiredException)
            {
                var authenticationResult = await publicClientApplication.AcquireTokenInteractive(AppConstants.Scopes)
#if ANDROID
                            .WithParentActivityOrWindow(Platform.CurrentActivity)
#endif
                            .ExecuteAsync();

                // get the token and return it in your own way
                return authenticationResult.AccessToken;
            }
        }

        public AllowedHostsValidator AllowedHostsValidator { get; }
    }
}

