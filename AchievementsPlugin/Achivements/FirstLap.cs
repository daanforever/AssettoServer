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
    public class FirstLap : IAchievement
    {
        string IAchievement.Name => "My first lap";

        private readonly AchievementsPlugin _plugin;

        public FirstLap(AchievementsPlugin plugin)
        {
            _plugin = plugin;
            _plugin.Stats.Update += OnUpdate;

        }

        private void OnUpdate(ACTcpClient sender, LapCompletedEventArgs args)
        {
            _ = _plugin.Earn(this, sender);
        }
    }
}
