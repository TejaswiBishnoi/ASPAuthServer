using Microsoft.Extensions.Configuration;
using RestSharp;
using System;

namespace IdentityProvider
{
    /// <summary>
    /// Provides Google OAuth2.0/OIDC Identity Services
    /// </summary>
    public class GoogleIdentityProvider: IIdentityProvider
    {
        public string Name { get; private set; }
        private string? ClientID { get; set; }
        private string? ClientSecret { get; set; }
        private string? RedirectURL { get; set; }
        private RestClient? RedirectClient;
        private RestClient? ExchangeClient;
        private string? SignUpScope { get; set; }
        private string? SignInScope { get; set; }
        private bool Configured { get; set; } = false;
        public IDictionary<string, string>? CreateAuthorizationRequest(bool signup = false)
        {
            if (!Configured)
            {
                throw new InvalidOperationException("Identity Provider not configured.");
            }
            var scope = signup ? SignUpScope : SignInScope;
            var request = new RestRequest("", Method.Get);
            string state = Name + ":" + Guid.NewGuid().ToString();
            request.AddParameter("client_id", ClientID);
            request.AddParameter("redirect_uri", RedirectURL);
            request.AddParameter("scope", scope);
            request.AddParameter("response_type", "code");
            request.AddParameter("access_type", "online");
            request.AddParameter("include_granted_scopes", "true");
            request.AddParameter("state", state);
            request.AddParameter("prompt", "consent");
            string redirUrl = RedirectClient.BuildUri(request).AbsoluteUri;
            return new Dictionary<string, string> { { "URL", redirUrl}, { "State", state } };
        }
        private string GetIDToken(string code)
        {
            if (!Configured)
            {
                throw new InvalidOperationException("Identity Provider not configured.");
            }
            RestRequest request = new("", Method.Post);
            request.AddParameter("code", code);
            request.AddParameter("client_id", ClientID);
            request.AddParameter("client_secret", ClientSecret);
            request.AddParameter("redirect_uri", RedirectURL);
            request.AddParameter("grant_type", "authorization_code");
            var response = ExchangeClient?.Execute(request);
            if (response?.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new HttpRequestException("Failed to exchange code for token.");
            }
            try
            {
                var resp = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json: response?.Content);
                if (resp == null || !resp.ContainsKey("id_token")) throw new Exception("Failed to deserialize response and fetch ID Token");
                return resp["id_token"];
            }
            catch (Exception ex)
            {
                throw new DeserializationException(response, ex);
            }
            return "";
        }
        public GoogleIdentityProvider()
        {
            Name = "NC";
        }
        public void Configure(string name, IConfigurationSection config)
        {
            if (Configured) return;
            Name = name;
            ClientID = config["ClientID"];
            ClientSecret = config["ClientSecret"];
            RedirectURL = config["RedirectURL"];
            RedirectClient = new RestClient("https://accounts.google.com/o/oauth2/v2/auth");
            ExchangeClient = new RestClient("https://oauth2.googleapis.com/token");
            SignInScope = config["SignInScope"];
            SignUpScope = config["SignUpScope"];
            Configured = true;
        }
    }
}
