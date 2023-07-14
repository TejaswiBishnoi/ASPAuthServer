using Microsoft.Extensions.Configuration;
using RestSharp;
using System.IdentityModel.Tokens.Jwt;

namespace IdentityProvider
{
    /// <summary>
    /// Class representing JSON Web Key.
    /// </summary>
    public class JSONWebKey
    {
        public string E { get; }
        public string Kty { get; }
        public string N { get; }
        public string Kid { get; }
        public string Alg { get; }
        public string Use { get; }
        public JSONWebKey(string e, string kty, string n, string kid, string alg, string use)
        {
            E = e;
            Kty = kty;
            N = n;
            Kid = kid;
            Alg = alg;
            Use = use;
        }
    }
    public class JSONWebKeySet
    {
        public IList<JSONWebKey> JSONWebKeys { get;}
        public JSONWebKeySet(IList<JSONWebKey> keys)
        {
            JSONWebKeys = keys;
        }
        public void PopulateStore(Dictionary<string, JSONWebKey> publicKeyStore)
        {
            foreach (var key in JSONWebKeys)
            {
                publicKeyStore.Add(key.Kid, key);
            }
        }
    }
    /// <summary>
    /// Provides Google OAuth2.0/OIDC Identity Services
    /// </summary>
    public class GoogleIdentityProvider: IIdentityProvider
    {
        public string Name { get; private set; }
        private string? _clientID;
        private string? _clientSecret;
        private string? _redirectURL;
        private RestClient? _redirectClient;
        private RestClient? _exchangeClient;
        private string? _signUpScope;
        private string? _signInScope;
        private string? _publicKeyURL;
        private readonly Dictionary<string, JSONWebKey> _publicKeyStore;
        private bool _configured;
        public IDictionary<string, string>? CreateAuthorizationRequest(bool signup = false)
        {
            if (!_configured)
            {
                throw new InvalidOperationException("Identity Provider not configured.");
            }
            var scope = signup ? _signUpScope : _signInScope;
            var request = new RestRequest("", Method.Get);
            string state = Name + ":" + Guid.NewGuid().ToString();
            request.AddParameter("client_id", _clientID);
            request.AddParameter("redirect_uri", _redirectURL);
            request.AddParameter("scope", scope);
            request.AddParameter("response_type", "code");
            request.AddParameter("access_type", "online");
            request.AddParameter("include_granted_scopes", "true");
            request.AddParameter("state", state);
            request.AddParameter("prompt", "consent");
            string redirUrl = _redirectClient.BuildUri(request).AbsoluteUri;
            return new Dictionary<string, string> { { "URL", redirUrl}, { "State", state } };
        }
        private bool ValidateToken(string header, string payload, string signature)
        {         
            try
            {
                var jwtHeader = JwtHeader.Base64UrlDeserialize(header);
                var jwtPayload = JwtPayload.Base64UrlDeserialize(payload);
                if (!jwtHeader.ContainsKey("kid")) return false;
                var kid = jwtHeader["kid"] as string;
                
            }
            catch (Exception)
            {
                return false;
            }
            
        }        
        private string GetIDToken(string code)
        {
            if (!_configured)
            {
                throw new InvalidOperationException("Identity Provider not configured.");
            }
            RestRequest request = new("", Method.Post);
            request.AddParameter("code", code);
            request.AddParameter("client_id", _clientID);
            request.AddParameter("client_secret", _clientSecret);
            request.AddParameter("redirect_uri", _redirectURL);
            request.AddParameter("grant_type", "authorization_code");
            var response = _exchangeClient?.Execute(request);
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
        }
        public GoogleIdentityProvider()
        {
            _publicKeyStore = new Dictionary<string, JSONWebKey>();
            Name = "NC";
        }
        public void Configure(string name, IConfigurationSection config)
        {
            if (_configured) return;
            Name = name;
            _clientID = config["ClientID"];
            _clientSecret = config["ClientSecret"];
            _redirectURL = config["RedirectURL"];
            _redirectClient = new RestClient("https://accounts.google.com/o/oauth2/v2/auth");
            _exchangeClient = new RestClient("https://oauth2.googleapis.com/token");
            _signInScope = config["SignInScope"];
            _signUpScope = config["SignUpScope"];
            _publicKeyURL = config["PublicKeyURL"];

            //Fetch Public Key
            RestClient pubKeyFetchClient = new RestClient(_publicKeyURL);
            RestRequest pubKeyFetchRequest = new RestRequest("");
            var pubKey = pubKeyFetchClient.Execute<JSONWebKeySet>(pubKeyFetchRequest);
            if (!pubKey.IsSuccessStatusCode || pubKey.Data == null) throw new InvalidDataException("JSONWebKeySet cannot be fetched");
            pubKey.Data.PopulateStore(_publicKeyStore);
            _configured = true;
        }
    }
}
