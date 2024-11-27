using AssettoServer.Network.Tcp;
using AssettoServer.Server;

namespace AchievementsPlugin.Achievements
{
    public class OneThousandLaps : IAchievement
    {
        string IAchievement.Name => "1000 laps";

        private readonly AchievementsPlugin _plugin;

        public OneThousandLaps(AchievementsPlugin plugin)
        {
            _plugin = plugin;
            _plugin.Stats.Update += OnUpdate;

        }

        private void OnUpdate(ACTcpClient sender, LapCompletedEventArgs result)
        {
            var laps = _plugin.DataStorage.ExecuteScalar<int>(@"
                SELECT COUNT(*)
                FROM records r
                INNER JOIN players p ON r.player_id = p.id
                WHERE
                  p.hashedGUID = @HashedGuid
            ", new { sender.HashedGuid });

            if (laps >= 1000)
            {
                _plugin.Earn(this, sender);
            }
        }
    }
}
