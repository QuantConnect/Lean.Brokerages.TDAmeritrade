using QuantConnect.Configuration;
using QuantConnect.Logging;
using System.Collections;

namespace QuantConnect.Tests.Brokerages.TDAmeritrade
{
    [TestFixture]
    public class TestSetup
    {
        [Test, TestCaseSource(nameof(TestParameters))]
        public void TestSetupCase()
        {
        }

        public static void ReloadConfiguration()
        {
            // nunit 3 sets the current folder to a temp folder we need it to be the test bin output folder
            var dir = TestContext.CurrentContext.TestDirectory;
            Environment.CurrentDirectory = dir;
            Directory.SetCurrentDirectory(dir);
            // reload config from current path
            Config.Reset();

            var environment = Environment.GetEnvironmentVariables();
            foreach (DictionaryEntry entry in environment)
            {
                var envKey = entry.Key.ToString();
                var value = entry.Value.ToString();

                if (envKey.StartsWith("QC_"))
                {
                    var key = envKey.Substring(3).Replace("_", "-").ToLower();

                    Log.Trace($"TestSetup(): Updating config setting '{key}' from environment var '{envKey}'");
                    Config.Set(key, value);
                }
            }

            // resets the version among other things
            Globals.Reset();

            TestGlobals.Initialize();
        }

        private static void SetUp()
        {
            Log.LogHandler = new CompositeLogHandler();
            Log.Trace("TestSetup(): starting...");
            ReloadConfiguration();
        }

        private static TestCaseData[] TestParameters
        {
            get
            {
                SetUp();
                return new[] { new TestCaseData() };
            }
        }
    }
}
