using AssettoServer.Commands;
using Qmmands;

namespace AchievementsPlugin;

public class AchievementsCommandModule : ACModuleBase
{
    private readonly AchievementsPlugin _plugin;

    public AchievementsCommandModule(AchievementsPlugin plugin)
    {
        _plugin = plugin;
    }

    [Command("/achievements")]
    public void AchievementsPlugin()
    {
        Reply("Hello from AchievementClasses plugin!");
    }
}
