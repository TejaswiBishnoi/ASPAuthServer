using Microsoft.Extensions.Configuration;
using IdentityProvider;

namespace Tests
{
    public class Tests
    {
        IConfigurationSection config;
        GoogleIdentityProvider googleIdentityProvider;
        string code;
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true);
            IConfiguration config = builder.Build();
            IConfigurationSection section = config.GetSection("IdentityPlugins").GetSection("OAuth").GetSection("Google");            
            this.config = section;
            googleIdentityProvider = new GoogleIdentityProvider();
            googleIdentityProvider.Configure("Google", section);
            code = config["code"];
        }
        [SetUp]
        public void Setup()
        {
            
        }

        [Test]
        public void ConfigTest()
        {
            try
            {
                var x = new GoogleIdentityProvider();
                x.Configure("Google", config);                
            }
            catch(Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            Assert.Pass();
        }        
        [Test]
        public void AuthorizationURLTest()
        {
            IDictionary<string, string>? dic = null;
            try
            {
                dic = googleIdentityProvider.CreateAuthorizationRequest();
            }
            catch(Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            if (dic == null || !dic.ContainsKey("URL"))
            {
                Assert.Fail("Request not complete!");
            }
            Assert.Pass(dic["URL"]);
        }
        [Test]
        public void GetIdentityTest()
        {
            string? iden = null;
            try
            {
                iden = googleIdentityProvider.ExchangeCodeForIdentity(code);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message + "\n" + ex.StackTrace);
            }
            Assert.IsTrue(!string.IsNullOrEmpty(iden) && iden == "tejaswibishnoi@gmail.com");
        }
        [Test]
        public void DeserializeTest()
        {
            try
            {
                //Json string below is an example provided by Google and hence is safe to put here.
                var resp = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json: "{\n  \"access_token\": \"1/fFAGRNJru1FTz70BzhT3Zg\",\n  \"expires_in\": 3920,\n  \"token_type\": \"Bearer\",\n  \"scope\": \"https://www.googleapis.com/auth/drive.metadata.readonly\",\n  \"refresh_token\": \"1//xEoDL4iW3cxlI7yDbSRFYNG01kVKM2C-259HOF2aQbI\"\n}");
                Assert.Pass(resp["access_token"].);
            }
            catch (Exception e) { Assert.Fail();}
        }
    }
}