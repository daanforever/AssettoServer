using AssettoServer.Network.Tcp;
using AssettoServer.Server;

namespace AchievementsPlugin.Achievements
{
    public class FirstLapWithoutCuts : IAchievement
    {
        string IAchievement.Name => "The first lap without cuts";

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
                _plugin.Earn(this, sender);
            }
        }
    }
}
