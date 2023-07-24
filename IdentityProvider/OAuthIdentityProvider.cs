using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RestSharp;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace IdentityProvider
{
    /// <summary>
    /// Object representing OAuth Token Endpoint Response.
    /// </summary>
    public class TokenEndpointResponse
    {
        public string? access_token { get; private set; }
        public string? token_type { get; private set; }
        public int expires_in { get; private set; }
        public string? refresh_token { get; private set; }
        public string? id_token { get; private set; }
        public string? scope { get; private set; }
        public TokenEndpointResponse(string? access_token, string? token_type, int expires_in, string? refresh_token, string? id_token, string? scope)
        {
            this.access_token = access_token;
            this.token_type = token_type;
            this.expires_in = expires_in;
            this.refresh_token = refresh_token;
            this.id_token = id_token;
            this.scope = scope;
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
        private readonly Dictionary<string, JsonWebKey> _publicKeyStore;
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
            request.AddParameter("access_type", "offline");
            request.AddParameter("include_granted_scopes", "true");
            request.AddParameter("state", state);
            request.AddParameter("prompt", "consent");
            string redirUrl = _redirectClient.BuildUri(request).AbsoluteUri;
            return new Dictionary<string, string> { { "URL", redirUrl}, { "State", state } };
        }
        public IdentityObject? ExchangeCodeForIdentityInfo(string code)
        {
            if (!_configured)
            {
                throw new InvalidOperationException("Identity Provider not configured.");
            }
            var idToken = GetIDToken(code);
            var jwt = idToken.Split('.');
            if (jwt.Length != 3) throw new Exception("Invalid JWT");
            var header = jwt[0];
            var payload = jwt[1];
            var signature = jwt[2];
            var validatedToken = ValidateToken(header, payload, signature);
            IdentityObject identityInfo = new IdentityObject();
            identityInfo.Email = validatedToken.Claims.Where(x => x.Type == "email").FirstOrDefault()?.Value;
            identityInfo.Name = validatedToken.Claims.Where(x => x.Type == "name").FirstOrDefault()?.Value;
            identityInfo.PictureURL = validatedToken.Claims.Where(x => x.Type == "picture").FirstOrDefault()?.Value;
            identityInfo.Token = idToken;
            identityInfo.MainField = identityInfo.Email;
            if (identityInfo.MainField == null) throw new Exception("Email Claim not found in JWT");
            return identityInfo;
        }
        public string? ExchangeCodeForIdentity(string code)
        {
            if (!_configured)
            {
                throw new InvalidOperationException("Identity Provider not configured.");
            }
            var idToken = GetIDToken(code);
            var jwt = idToken.Split('.');
            if (jwt.Length != 3) throw new Exception("Invalid JWT");
            var header = jwt[0];
            var payload = jwt[1];
            var signature = jwt[2];
            var validatedToken = ValidateToken(header, payload, signature);
            var clm = validatedToken.Claims.Where(x => x.Type == "email").FirstOrDefault();
            if (clm == null) throw new Exception("Email Claim not found in JWT");
            return clm.Value;
        }
        private JwtSecurityToken ValidateToken(string header, string payload, string signature)
        {         
            try
            {
                var jwtHeader = JwtHeader.Base64UrlDeserialize(header);
                var jwtPayload = JwtPayload.Base64UrlDeserialize(payload);
                if (!jwtHeader.ContainsKey("kid")) throw new Exception("JWT Header does not contain kid!");
                var kid = jwtHeader["kid"] as string;
                if (!_publicKeyStore.ContainsKey(kid)) throw new Exception("Key Not Found!");
                var key = _publicKeyStore[kid];

                using (RSA rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportParameters(
                        new RSAParameters(){
                            Modulus = Base64UrlEncoder.DecodeBytes(key.N),
                            Exponent = Base64UrlEncoder.DecodeBytes(key.E)
                        });
                    var rsaSecurityKey = new RsaSecurityKey(rsa);
                    var validationParameters = new TokenValidationParameters()
                    {
                        ValidIssuer = "https://accounts.google.com",
                        ValidAudience = _clientID,
                        IssuerSigningKey = rsaSecurityKey
                    };
                    var handler = new JwtSecurityTokenHandler();
                    string token = header + "." + payload + "." + signature;
                    handler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);  
                    return validatedToken as JwtSecurityToken;
                }
                
            }                        
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
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
            var response = _exchangeClient?.Execute<TokenEndpointResponse>(request);
            if (response == null || !response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to exchange code for token.");
            }
            if (response.Data == null || response.Data.id_token == null)
            {
                throw new Exception("Failed to exchange code for token.");
            }
            return response.Data.id_token;
        }
        public GoogleIdentityProvider()
        {
            _publicKeyStore = new Dictionary<string, JsonWebKey>();
            Name = "Google";
            _configured = false;
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
            RestRequest pubKeyFetchRequest = new RestRequest("/");
            var pubKey = pubKeyFetchClient.Execute(pubKeyFetchRequest);
            if (!pubKey.IsSuccessStatusCode || pubKey.Content == null) throw new InvalidDataException("JSONWebKeySet cannot be fetched");
            var pubKeySet = new JsonWebKeySet(pubKey.Content);
            foreach (var key in pubKeySet.Keys)
            {
                _publicKeyStore.Add(key.Kid, key);
            }
            _configured = true;
        }
    }
    /// <summary>
    /// Provides Microsoft OAuth2.0/OIDC Identity Services
    /// </summary>
    public class MicrosoftIdentityProvider: IIdentityProvider
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
        private readonly Dictionary<string, JsonWebKey> _publicKeyStore;
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
            request.AddParameter("access_type", "offline");
            request.AddParameter("response_mode", "query");
            // Random nonce to be replaced by a concrete implementation in future which verifies it to prevent against replay.
            var rnd = new Random();
            request.AddParameter("nonce", rnd.Next(1000000, 9999999));
            request.AddParameter("state", state);
            request.AddParameter("prompt", "consent");
            string redirUrl = _redirectClient.BuildUri(request).AbsoluteUri;
            return new Dictionary<string, string> { { "URL", redirUrl }, { "State", state } };
        }        
        private JwtSecurityToken ValidateToken(string header, string payload, string signature)
        {
            try
            {
                var jwtHeader = JwtHeader.Base64UrlDeserialize(header);
                var jwtPayload = JwtPayload.Base64UrlDeserialize(payload);
                if (!jwtHeader.ContainsKey("kid")) throw new Exception("JWT Header does not contain kid!");
                var kid = jwtHeader["kid"] as string;
                if (!_publicKeyStore.ContainsKey(kid)) throw new Exception("Key Not Found!");
                var key = _publicKeyStore[kid];

                using (RSA rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportParameters(
                        new RSAParameters()
                        {
                            Modulus = Base64UrlEncoder.DecodeBytes(key.N),
                            Exponent = Base64UrlEncoder.DecodeBytes(key.E)
                        });
                    var rsaSecurityKey = new RsaSecurityKey(rsa);
                    var validationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = false,
                        ValidAudience = _clientID,
                        IssuerSigningKey = rsaSecurityKey
                    };
                    var handler = new JwtSecurityTokenHandler();
                    string token = header + "." + payload + "." + signature;
                    handler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                    return validatedToken as JwtSecurityToken;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
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
            var response = _exchangeClient?.Execute<TokenEndpointResponse>(request);
            if (response == null || !response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to exchange code for token.");
            }
            if (response.Data == null || response.Data.id_token == null)
            {
                throw new Exception("Failed to exchange code for token.");
            }
            return response.Data.id_token;
        }
        public string? ExchangeCodeForIdentity(string code)
        {            
            if (!_configured)
            {
                throw new InvalidOperationException("Identity Provider not configured.");
            }
            var idToken = GetIDToken(code);
            var jwt = idToken.Split('.');
            if (jwt.Length != 3) throw new Exception("Invalid JWT");
            var header = jwt[0];
            var payload = jwt[1];
            var signature = jwt[2];
            var validatedToken = ValidateToken(header, payload, signature);            
            if (validatedToken.Subject == null) throw new Exception("Identity Carrying Claim not found in JWT");
            return validatedToken.Subject;
        }
        public IdentityObject? ExchangeCodeForIdentityInfo(string code)
        {            
            var idToken = GetIDToken(code);
            var jwt = idToken.Split('.');
            if (jwt.Length != 3) throw new Exception("Invalid JWT");
            var header = jwt[0];
            var payload = jwt[1];
            var signature = jwt[2];
            var validatedToken = ValidateToken(header, payload, signature);
            IdentityObject identityInfo = new IdentityObject();
            identityInfo.Email = validatedToken.Claims.Where(x => x.Type == "email").FirstOrDefault()?.Value;
            identityInfo.Name = validatedToken.Claims.Where(x => x.Type == "name").FirstOrDefault()?.Value;
            identityInfo.PictureURL = validatedToken.Claims.Where(x => x.Type == "picture").FirstOrDefault()?.Value;
            identityInfo.Token = idToken;
            identityInfo.MainField = validatedToken.Subject;
            if (identityInfo.MainField == null) throw new Exception("Identity Carrying Claim not found in JWT");
            return identityInfo;
        }
        public MicrosoftIdentityProvider()
        {            
            _publicKeyStore = new Dictionary<string, JsonWebKey>();
            Name = "Microsoft";
            _configured = false;
        }
        public void Configure(string name, IConfigurationSection config)
        {            
            if (_configured) return;
            Name = name;
            _clientID = config["ClientID"];
            _clientSecret = config["ClientSecret"];
            _redirectURL = config["RedirectURL"];
            _redirectClient = new RestClient("https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize");
            _exchangeClient = new RestClient("https://login.microsoftonline.com/consumers/oauth2/v2.0/token");
            _signInScope = config["SignInScope"];
            _signUpScope = config["SignUpScope"];
            _publicKeyURL = config["PublicKeyURL"];

            //Fetch Public Key
            RestClient pubKeyFetchClient = new RestClient(_publicKeyURL);
            RestRequest pubKeyFetchRequest = new RestRequest("/");
            var pubKey = pubKeyFetchClient.Execute(pubKeyFetchRequest);
            if (!pubKey.IsSuccessStatusCode || pubKey.Content == null) throw new InvalidDataException("JSONWebKeySet cannot be fetched");
            var pubKeySet = new JsonWebKeySet(pubKey.Content);
            foreach (var key in pubKeySet.Keys)
            {
                _publicKeyStore.Add(key.Kid, key);
            }
            _configured = true;
        }
    }

    [Obsolete]
    /// <summary>
    /// Apple Identity Provider. Development stopped for now due to apple's restriction on devices.
    /// </summary>
    /// <remarks>
    /// Do not use this class as it is incomplete and for now will not be developed any further.
    /// The Constructor of this class will throw NotImplementedException() indicating class has not been implemented yet.
    /// </remarks>
    class AppleIdentityProvider: IIdentityProvider
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
        private readonly Dictionary<string, JsonWebKey> _publicKeyStore;
        private bool _configured;
        
        public IdentityObject ExchangeCodeForIdentityInfo(string code)
        {
            throw new NotImplementedException();
        }
        public string? ExchangeCodeForIdentity(string code)
        {
            throw new NotImplementedException();
        }
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
            request.AddParameter("response_mode", "query");
            request.AddParameter("access_type", "offline");
            request.AddParameter("state", state);
            string redirUrl = _redirectClient.BuildUri(request).AbsoluteUri;
            return new Dictionary<string, string> { { "URL", redirUrl }, { "State", state } };
        }
        public AppleIdentityProvider()
        {
            throw new NotImplementedException();
            //_publicKeyStore = new Dictionary<string, JsonWebKey>();
            //Name = "Apple";
        }
        public void Configure(string name, IConfigurationSection config)
        {
            if (_configured) return;
            Name = name;
            _clientID = config["ClientID"];
            _clientSecret = config["ClientSecret"];
            _redirectURL = config["RedirectURL"];
            _redirectClient = new RestClient("https://appleid.apple.com/auth/authorize");
            _exchangeClient = new RestClient("https://appleid.apple.com/auth/token");
            _signInScope = config["SignInScope"];
            _signUpScope = config["SignUpScope"];
            _publicKeyURL = config["PublicKeyURL"];

            //Fetch Public Key
            RestClient pubKeyFetchClient = new RestClient(_publicKeyURL);
            RestRequest pubKeyFetchRequest = new RestRequest("/");
            var pubKey = pubKeyFetchClient.Execute(pubKeyFetchRequest);
            if (!pubKey.IsSuccessStatusCode || pubKey.Content == null) throw new InvalidDataException("JSONWebKeySet cannot be fetched");
            var pubKeySet = new JsonWebKeySet(pubKey.Content);
            foreach (var key in pubKeySet.Keys)
            {
                _publicKeyStore.Add(key.Kid, key);
            }
            _configured = true;
        }
    }
}
