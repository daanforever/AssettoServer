﻿using AssettoServer.Network.Tcp;
using AssettoServer.Server;

namespace AchievementsPlugin.Achievements
{
    public class OneHundredLaps : IAchievement
    {
        string IAchievement.Name => "100 laps";

        private readonly AchievementsPlugin _plugin;

        public OneHundredLaps(AchievementsPlugin plugin)
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

            if (laps >= 100)
            {
                _plugin.Earn(this, sender);
            }
        }
    }
}