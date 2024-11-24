using AssettoServer.Server;
using Autofac;
using DataStoragePlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AchievementsPlugin.Tests
{
    internal class FakeServerTest
    {
        private readonly FakeServer _fake;

        public FakeServerTest()
        {
            _fake = new FakeServer();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _fake.Dispose();
        }

        [Test]
        public void ContainerBuild()
        {
            Assert.That(_fake.Scope.IsRegistered(typeof(EntryCarManager)), Is.EqualTo(true));
        }
    }
}
