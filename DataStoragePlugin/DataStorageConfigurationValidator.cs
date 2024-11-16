using FluentValidation;
using JetBrains.Annotations;

namespace DataStoragePlugin;

// Use FluentValidation to validate plugin configuration
[UsedImplicitly]
public class SampleConfigurationValidator : AbstractValidator<DataStorageConfiguration>
{
    public SampleConfigurationValidator()
    {
        RuleFor(cfg => cfg.DataDir).NotEmpty();
    }
}
