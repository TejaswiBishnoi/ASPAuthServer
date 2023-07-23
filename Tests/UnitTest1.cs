using Microsoft.Extensions.Configuration;
using IdentityProvider;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;

namespace Tests
{
    public class Tests
    {
        IConfigurationSection config;
        GoogleIdentityProvider googleIdentityProvider;
        string code;
        string email;
        string name;
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
            email = config["TestEmail"];
            name = config["TestName"];
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
        [TestCase()]
        [TestCase(true)]
        public void AuthorizationURLTest(bool signup = false)
        {
            IDictionary<string, string>? dic = null;
            try
            {
                dic = googleIdentityProvider.CreateAuthorizationRequest(signup);
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
            Assert.IsTrue(!string.IsNullOrEmpty(iden) && iden == email);
        }
        [Test]
        public void GetIdentityInfoTest()
        {
            IdentityObject? info = null;
            try
            {
                info = googleIdentityProvider.ExchangeCodeForIdentityInfo(code);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message + "\n" + ex.StackTrace);
            }
            Assert.IsTrue(info != null && info.Name == name && info.Email == email, info?.Token);
        }
    }
    class MSIDProviderTest
    {
        IConfigurationSection config;
        IIdentityProvider microsoftIdentityProvider;
        string code;
        string identity;
        string name;
        string email;
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true);
            IConfiguration config = builder.Build();
            IConfigurationSection section = config.GetSection("IdentityPlugins").GetSection("OAuth").GetSection("Microsoft");
            this.config = section;
            microsoftIdentityProvider = new MicrosoftIdentityProvider();
            microsoftIdentityProvider.Configure("Microsoft", section);
            code = section["code"];
            identity = section["sub"];
            name = section["name"];
            email = section["email"];
        }
        [Test] 
        public void ConfigTest()
        {
            try
            {
                IIdentityProvider x = new MicrosoftIdentityProvider();
                x.Configure("Microsoft", config);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            Assert.Pass();
        }
        [TestCase()]
        [TestCase(true)]
        public void AuthorizationURLTest(bool signup = false)
        {
            IDictionary<string, string>? dic = null;
            try
            {
                dic = microsoftIdentityProvider.CreateAuthorizationRequest(signup);
            }
            catch (Exception ex)
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
                iden = microsoftIdentityProvider.ExchangeCodeForIdentity(code);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message + "\n" + ex.StackTrace);
            }
            Assert.IsTrue(iden is not null && identity == iden);
        }
        [Test]
        public void GetIdentityInfoTest()
        {
            IdentityObject? info = null;
            try
            {
                info = microsoftIdentityProvider.ExchangeCodeForIdentityInfo(code);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message + "\n" + ex.StackTrace);
            }
            Assert.IsTrue(info != null && info.Name == name && info.Email == email, info?.Token);
            Assert.Pass(info?.Token);
        }
    }
}