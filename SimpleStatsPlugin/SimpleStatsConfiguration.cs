using AssettoServer.Server.Configuration;
using JetBrains.Annotations;

namespace SimpleStatsPlugin;

[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
public class SimpleStatsConfiguration : IValidateConfiguration<SampleConfigurationValidator>
{
    public string DataDir { get; init; } = "data";
}
