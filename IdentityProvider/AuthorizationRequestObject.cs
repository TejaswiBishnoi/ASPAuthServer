namespace IdentityProvider
{
    public class AuthorizationRequestObject
    {
        public AuthorizationRequestObject(string? redirectURL, string? state)
        {
            RedirectURL = redirectURL;
            State = state;
        }
        public string? RedirectURL { get; }
        public string? State { get; }
    }
}
