# SimpleStatsPlugin
Sample to show how to add custom HTTP routes and chat commands to the server.

## TODO
* Add trackname to db
* Today top and average lap time.


```xml
<PropertyGroup>
    <EnableDynamicLoading>true</EnableDynamicLoading>
</PropertyGroup>
```

```xml
<ItemGroup>
    <ProjectReference Include="..\AssettoServer\AssettoServer.csproj">
        <Private>false</Private>
        <ExcludeAssets>runtime</ExcludeAssets>
    </ProjectReference>
</ItemGroup>
```

Then create a class that implements the `IAssettoServerPlugin` (for plugins without configuration) or `IAssettoServerPlugin<T>` interface (for plugins with configuration).
You can check the other plugins to see how configuration is handled, but basically you can append sections to `extra_cfg.yml` like this:
```yaml
---
!<type of your configuration class>
Config1: Value1
Config2: Value2
```
