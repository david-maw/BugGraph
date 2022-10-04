using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Diagnostics;

namespace Graph6;

public partial class MainPage : ContentPage
{
	int count = 0;
    private static readonly string[] _scopes = new[] { "User.Read" };
    private const string TenantId = "5b48c79c-24fd-4e7f-96be-194d6f23b623";
    public const string ClientId = "85e78333-f59d-4623-bf1b-a351802ca32b";
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
			TenantId = TenantId,
			ClientId = ClientId,
			AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
			// MUST be http://localhost or http://localhost:PORT
			// See https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/System-Browser-on-.Net-Core
			RedirectUri = new Uri("http://localhost"),
		};

		// https://learn.microsoft.com/dotnet/api/azure.identity.interactivebrowsercredential
		var interactiveCredential = new InteractiveBrowserCredential(options);

		var graphClient = new GraphServiceClient(interactiveCredential, _scopes);

#endif
		// Using Microsoft.Identity.Client
		var pc = PublicClientApplicationBuilder.Create(ClientId)
				.WithAuthority($"https://login.microsoftonline.com/{TenantId}/")
				.WithDefaultRedirectUri()
				.Build();


		var t = await pc.AcquireTokenInteractive(_scopes)
			#if ANDROID
			.WithParentActivityOrWindow(Platform.CurrentActivity)
			#endif
			.ExecuteAsync();


		var graphClient = new GraphServiceClient("https://graph.microsoft.com/beta", null);

		graphClient.AuthenticationProvider = new DelegateAuthenticationProvider(async (request) =>
		{
			request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", t.AccessToken);
			await Task.FromResult<object>(null);
		});

		try
		{
            user = await graphClient.Me.Request().GetAsync();
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
}

