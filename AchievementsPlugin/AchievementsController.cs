using AssettoServer.Server;
using Microsoft.AspNetCore.Mvc;
using AssettoServer.Server.Plugin;
using DotNext;

namespace AchievementsPlugin;

[ApiController]
public class AchievementsController : ControllerBase
{
    private readonly AchievementsPlugin _plugin;
    private readonly ACServer _server;

    public AchievementsController(AchievementsPlugin plugin, ACServer server)
    {
        _plugin = plugin;
        _server = server;
    }

    [HttpGet("/achievements")]
    public string Achievements()
    {
        string result = "AchievementClasses HTTP/GET";
        return result;
    }
}
