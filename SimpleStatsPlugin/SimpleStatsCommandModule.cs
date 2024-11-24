using AssettoServer.Commands;
using Qmmands;

namespace SimpleStatsPlugin;

public class SimpleStatsCommandModule : ACModuleBase
{
    private readonly SimpleStats _plugin;

    public SimpleStatsCommandModule(SimpleStats plugin)
    {
        _plugin = plugin;
    }

    [Command("stats")]
    public void SimpleStatsPlugin()
    {
        Reply("Hello from SimpleStats plugin!");
    }
}
