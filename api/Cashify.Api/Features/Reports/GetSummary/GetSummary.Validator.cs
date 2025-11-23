using FluentValidation;

namespace Cashify.Api.Features.Reports.GetSummary;

public class GetSummaryValidator : AbstractValidator<GetSummaryQuery>
{
    public GetSummaryValidator()
    {
        RuleFor(x => x.BusinessId).NotEmpty();
        RuleFor(x => x.Range).Must(GetSummaryQueryExtensions.BeValidRange).When(x => !x.StartDate.HasValue || !x.EndDate.HasValue);
        RuleFor(x => x)
            .Must(x => x.StartDate <= x.EndDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
        RuleFor(x => x.LimitPresetAndRange()).Equal(true).WithMessage("Provide either range preset or start/end dates, not both.");
    }
}

internal static class GetSummaryQueryExtensions
{
    public static bool LimitPresetAndRange(this GetSummaryQuery query)
    {
        var hasPreset = !string.IsNullOrWhiteSpace(query.Range);
        var hasDates = query.StartDate.HasValue || query.EndDate.HasValue;
        return !(hasPreset && hasDates);
    }

    public static bool BeValidRange(string? range)
    {
        if (string.IsNullOrWhiteSpace(range))
        {
            return true;
        }

        var value = range.ToLowerInvariant();
        return value is "daily" or "weekly" or "monthly";
    }
}
