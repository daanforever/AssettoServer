using Autofac;
using DataStoragePlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AchievementsPlugin.Tests
{
    public class AchievementsPluginTest
    {
        private readonly FakeServer _fake;
        private readonly AchievementsPlugin _plugin;

        public AchievementsPluginTest()
        {
            _fake = new FakeServer();
            _plugin = _fake.Scope.Resolve<AchievementsPlugin>(new TypedParameter(typeof(DataStorageSql), _fake.Data));
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _plugin.Dispose();
            _fake.Dispose();
        }

        [Test]
        public void Test()
        {
            Assert.That(_plugin, Is.InstanceOf(typeof(AchievementsPlugin)));
        }
    }
}
