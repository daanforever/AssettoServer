using AssettoServer.Server;
using Microsoft.AspNetCore.Mvc;
using AssettoServer.Server.Plugin;
using DotNext;

namespace SimpleStatsPlugin;

[ApiController]
public class SimpleStatsController : ControllerBase
{
    private readonly SimpleStatsPlugin _plugin;
    private readonly ACServer _server;

    public SimpleStatsController(SimpleStatsPlugin plugin, ACServer server)
    {
        _plugin = plugin;
        _server = server;
    }

    [HttpGet("/simplestatsplugin")]
    public string SimpleStats()
    {
        string result = "SimpleStatsPlugin DataDir: " + _plugin.Configuration.DataDir;
        result += 5100.ToString("D7");
        return result;
    }
}
