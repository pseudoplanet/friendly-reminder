using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using EAGetMail;
using System.Globalization;
using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class MailHandler : MonoBehaviour
{
    private static MailHandler _instance;
    public static MailHandler Instance {
        get => _instance;
        private set {
            if (_instance == null) {
                _instance = value;
            }
            else if (_instance != value) {
                Debug.Log("You messed up buddy.");
                Destroy(value);
            }
        }
    }
    // Set up the singleton implmenentation
    private void Awake() {
        Instance = this;
        mailResults = new List<MailResult>();
    }

    public List<MailResult> mailResults;
    public string accountUsername;
    public string accountToken;

    public string mailSubjectToLookFor;

    // how many seconds to wait before refreshing the mail again
    public float mailRefreshInterval;
    // the time that the mail last was updated
    private float lastMailRefresh;
    public bool isLoggedIn;
    public TextMeshProUGUI emailDisplay;
    public bool doEmailScanning;

    // client configuration
    // You should create your client id and client secret,
    // do not use the following client id in production environment, it is used for test purpose only.
    const string clientID = "289765634222-f6510vskad7a1srsis0nu6g6fd0pmsj5.apps.googleusercontent.com";
    public string clientSecret;
    const string scope = "openid%20profile%20email%20https://mail.google.com";
    const string authUri = "https://accounts.google.com/o/oauth2/v2/auth";
    const string tokenUri = "https://www.googleapis.com/oauth2/v4/token";

    void Update() {
        // updating the mail results every once in a while
        if (isLoggedIn && Time.time > lastMailRefresh + mailRefreshInterval) {
            // reading the user's email on a different thread
            Task task = new Task(() => RetrieveMailWithXOAUTH2(accountUsername, accountToken));
            task.Start();

            // write to the console that the search has been dispatched
            Debug.Log("Scanning for new mail...");

            lastMailRefresh = Time.time;
        }
    }

    public void AttemptGoogleLogin() {
        try
        {
            DoOauthAndRetrieveEmail();
            lastMailRefresh = Time.time;
        }
        catch (Exception ep)
        {
            Debug.Log(ep.ToString());
        }
    }

    public void ProcessMailResults() {
        for (int i = 0; i < mailResults.Count; i++) {
            
        }
        mailResults.Clear();
    }

    // this function is run ON ANOTHER THREAD
    void RetrieveMailWithXOAUTH2(string userEmail, string accessToken)
    {
        if (!doEmailScanning) {return;}
        try
        {
            // Create a folder named "inbox" under current directory
            // to save the email retrieved.
            string localInbox = string.Format("{0}\\inbox", Directory.GetCurrentDirectory());
            // If the folder is not existed, create it.
            if (!Directory.Exists(localInbox))
            {
                Directory.CreateDirectory(localInbox);
            }

            MailServer oServer = new MailServer("imap.gmail.com",
                    userEmail,
                    accessToken, // use access token as password
                    ServerProtocol.Imap4);

            // Set IMAP OAUTH 2.0
            oServer.AuthType = ServerAuthType.AuthXOAUTH2;
            // Enable SSL/TLS connection, most modern email server require SSL/TLS by default
            oServer.SSLConnection = true;
            // Set IMAP4 SSL Port
            oServer.Port = 993;

            // Since EAGetMail 5.3.5, Gmail Rest API is supported as well. You can use the following code
            // to retrieve email using Gmail Rest API.
            // MailServer oServer = new MailServer("gmail.googleapis.com",
            //        userEmail,
            //        accessToken, // use access token as password
            //        ServerProtocol.GmailRestApi);

            // oServer.AuthType = ServerAuthType.AuthXOAUTH2;
            // oServer.SSLConnection = true;

            MailClient oClient = new MailClient("TryIt");
            // Get new email only, if you want to get all emails, please remove this line
            oClient.GetMailInfosParam.GetMailInfosOptions = GetMailInfosOptionType.NewOnly;

            //Debug.Log("Connecting {0} ... " + oServer.Server);
            oClient.Connect(oServer);

            MailInfo[] infos = oClient.GetMailInfos();
            //Debug.Log("Total {0} email(s)\r\n " + infos.Length);

            for (int i = 0; i < infos.Length; i++)
            {
                MailInfo info = infos[i];

                // Receive email from email server
                Mail oMail = oClient.GetMail(info);

                // if the email isn't related to this app, ignore it
                if (oMail.Subject != mailSubjectToLookFor + " (Trial Version)") {continue;}

                // mark the email as read so it doesn't get read by the software again
                // keep in mind the software is only looking for new (unread) emails
                oClient.MarkAsRead(info, true);

                MailResult result = new MailResult();
                result.text = oMail.TextBody;
                // record who actually sent the email
                result.from = oMail.From.Address;

                mailResults.Add(result);
                
                // If you want to delete current email, please use Delete method instead of MarkAsRead
                // oClient.Delete(info);
            }

            // Quit and expunge emails marked as deleted from server.
            oClient.Quit();
            ProcessMailResults();
        }
        catch (Exception ep)
        {
            //Debug.Log(ep.Message);
        }
    }

    async void DoOauthAndRetrieveEmail()
    {
        // Creates a redirect URI using an available port on the loopback address.
        string redirectUri = string.Format("http://127.0.0.1:{0}/", GetRandomUnusedPort());
        Debug.Log("redirect URI: " + redirectUri);

        // Creates an HttpListener to listen for requests on that redirect URI.
        var http = new HttpListener();
        http.Prefixes.Add(redirectUri);
        Debug.Log("Listening ...");
        http.Start();

        // Creates the OAuth 2.0 authorization request.
        string authorizationRequest = string.Format("{0}?response_type=code&scope={1}&redirect_uri={2}&client_id={3}",
            authUri,
            scope,
            Uri.EscapeDataString(redirectUri),
            clientID
        );

        // Opens request in the browser.
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(authorizationRequest) { UseShellExecute = true });

        // Waits for the OAuth authorization response.
        var context = await http.GetContextAsync();

        // Brings the Console to Focus.
        BringConsoleToFront();

        // Sends an HTTP response to the browser.
        var response = context.Response;
        string responseString = string.Format("<html><head></head><body>Please return to the app and close current window.</body></html>");
        var buffer = Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        var responseOutput = response.OutputStream;
        Task responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) =>
        {
            responseOutput.Close();
            http.Stop();
            Debug.Log("HTTP server stopped.");
        });

        // Checks for errors.
        if (context.Request.QueryString.Get("error") != null)
        {
            Debug.Log(string.Format("OAuth authorization error: {0}.", context.Request.QueryString.Get("error")));
            return;
        }

        if (context.Request.QueryString.Get("code") == null)
        {
            Debug.Log("Malformed authorization response. " + context.Request.QueryString);
            return;
        }

        // extracts the code
        var code = context.Request.QueryString.Get("code");
        Debug.Log("Authorization code: " + code);

        string responseText = await RequestAccessToken(code, redirectUri);
        Debug.Log(responseText);

        OAuthResponseParser parser = new OAuthResponseParser();
        parser.Load(responseText);

        var user = parser.EmailInIdToken;
        var accessToken = parser.AccessToken;

        Debug.Log("User: {0} " + user);
        Debug.Log("AccessToken: {0} " + accessToken);

        accountUsername = user;
        accountToken = accessToken;

        // tell the UIManager that the person has logged in via Google (hide the login screen)
        UIManager.Instance.ConfirmLogin();

        // reading the user's email on a different thread
        Task task = new Task(() => RetrieveMailWithXOAUTH2(user, accessToken));
        task.Start();
    }

    async Task<string> RequestAccessToken(string code, string redirectUri)
    {
        Debug.Log("Exchanging code for tokens...");

        // builds the  request
        string tokenRequestBody = string.Format("code={0}&redirect_uri={1}&client_id={2}&client_secret={3}&grant_type=authorization_code",
            code,
            Uri.EscapeDataString(redirectUri),
            clientID,
            clientSecret
            );

        // sends the request
        HttpWebRequest tokenRequest = (HttpWebRequest)WebRequest.Create(tokenUri);
        tokenRequest.Method = "POST";
        tokenRequest.ContentType = "application/x-www-form-urlencoded";
        tokenRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

        byte[] _byteVersion = Encoding.ASCII.GetBytes(tokenRequestBody);
        tokenRequest.ContentLength = _byteVersion.Length;

        Stream stream = tokenRequest.GetRequestStream();
        await stream.WriteAsync(_byteVersion, 0, _byteVersion.Length);
        stream.Close();

        try
        {
            // gets the response
            WebResponse tokenResponse = await tokenRequest.GetResponseAsync();
            using (StreamReader reader = new StreamReader(tokenResponse.GetResponseStream()))
            {
                // reads response body
                return await reader.ReadToEndAsync();
            }

        }
        catch (WebException ex)
        {
            if (ex.Status == WebExceptionStatus.ProtocolError)
            {
                var response = ex.Response as HttpWebResponse;
                if (response != null)
                {
                    Debug.Log("HTTP: " + response.StatusCode);
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        // reads response body
                        string responseText = await reader.ReadToEndAsync();
                        Debug.Log(responseText);
                    }
                }
            }

            throw ex;
        }
    }

    // Hack to bring the Console window to front.

    [DllImport("kernel32.dll", ExactSpelling = true)]
    public static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    public void BringConsoleToFront()
    {
        SetForegroundWindow(GetConsoleWindow());
    }


    // ------------- supporting functions -------------

    // Generate an unqiue email file name based on date time
    static string _generateFileName(int sequence)
    {
        DateTime currentDateTime = DateTime.Now;
        return string.Format("{0}-{1:000}-{2:000}.eml",
            currentDateTime.ToString("yyyyMMddHHmmss", new CultureInfo("en-US")),
            currentDateTime.Millisecond,
            sequence);
    }

    static int GetRandomUnusedPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
