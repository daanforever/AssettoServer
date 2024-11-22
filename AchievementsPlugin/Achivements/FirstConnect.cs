using AssettoServer.Network.Tcp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AchievementsPlugin.Achievements
{
    public class FirstConnect : IAchievement
    {
        string IAchievement.Name => "First time on the server";

        private readonly AchievementsPlugin _plugin;

        public FirstConnect(AchievementsPlugin plugin)
        {
            _plugin = plugin;
            _plugin.ECM.ClientConnected += ClientConnected;

        }

        private void ClientConnected(ACTcpClient sender, EventArgs args)
        {
            _plugin.Earn(this, sender);
        }
    }
}
