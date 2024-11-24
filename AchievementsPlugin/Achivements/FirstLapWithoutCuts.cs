using AssettoServer.Network.Tcp;
using AssettoServer.Server;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AchievementsPlugin.Achievements
{
    public class FirstLapWithoutCuts : IAchievement
    {
        string IAchievement.Name => "First lap without cuts";

        private readonly AchievementsPlugin _plugin;

        public FirstLapWithoutCuts(AchievementsPlugin plugin)
        {
            _plugin = plugin;
            _plugin.Stats.Update += OnUpdate;
        }

        private void OnUpdate(ACTcpClient sender, LapCompletedEventArgs result)
        {
            if (result.Packet.Cuts == 0)
            {
                _ = _plugin.Earn(this, sender);
            }
        }
    }
}
