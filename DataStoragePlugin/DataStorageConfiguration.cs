using AssettoServer.Server.Configuration;
using JetBrains.Annotations;

namespace DataStoragePlugin;

[UsedImplicitly(ImplicitUseKindFlags.Assign, ImplicitUseTargetFlags.WithMembers)]
public class DataStorageConfiguration : IValidateConfiguration<SampleConfigurationValidator>
{
    public string DataDir { get; set; } = "data";
}
