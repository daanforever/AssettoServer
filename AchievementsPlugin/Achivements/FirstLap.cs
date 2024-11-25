using AssettoServer.Network.Tcp;
using AssettoServer.Server;

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
            _plugin.Earn(this, sender);
        }
    }
}
