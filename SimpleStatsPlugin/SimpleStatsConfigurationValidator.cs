using FluentValidation;
using JetBrains.Annotations;

namespace SimpleStatsPlugin;

// Use FluentValidation to validate plugin configuration
[UsedImplicitly]
public class SampleConfigurationValidator : AbstractValidator<SimpleStatsConfiguration>
{
    public SampleConfigurationValidator()
    {
        RuleFor(cfg => cfg.DataDir).NotEmpty();
    }
}
