using FluentValidation;
using JetBrains.Annotations;

namespace AchievementsPlugin;

// Use FluentValidation to validate plugin configuration
[UsedImplicitly]
public class SampleConfigurationValidator : AbstractValidator<AchievementsConfiguration>
{
    public SampleConfigurationValidator()
    {

    }
}
