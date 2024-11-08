using AssettoServer.Commands;
using Qmmands;

namespace SimpleStatsPlugin;

public class SimpleStatsCommandModule : ACModuleBase
{
    private readonly SimpleStatsPlugin _plugin;

    public SimpleStatsCommandModule(SimpleStatsPlugin plugin)
    {
        _plugin = plugin;
    }

    [Command("stats")]
    public void SimpleStatsPlugin()
    {
        Reply("Hello from SimpleStats plugin!");
    }
}
