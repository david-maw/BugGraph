namespace Graph6;
public static class AppConstants
{
    // ClientID of the application in (ms sample testing)
    public const string ClientId = "858b4a09-dc31-45d3-83a7-2b5f024f99cd"; // TODO - Replace with your client Id. And also replace in the AndroidManifest.xml

    /// <summary>
    /// Scopes defining what app can access in the graph
    /// </summary>
    public static string[] Scopes = { "User.Read", "MailboxSettings.Read", "Files.ReadWrite.All" };
}