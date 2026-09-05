using System;
using System.Collections.Generic;

public static class InspectionJudge
{
    public static InspectionDecision Evaluate(
        NPCData npc,
        IEnumerable<RuleSO> rules,
        string currentDate
    )
    {
        if (npc == null)
            return new InspectionDecision(false, "NPC data is missing.");

        if (npc.useManualDecision)
            return new InspectionDecision(npc.manualShouldApprove, npc.manualDecisionReason);

        if (rules == null)
            return new InspectionDecision(true, "No active rule failed.");

        foreach (RuleSO rule in rules)
        {
            if (rule == null)
                continue;

            RuleCheckType[] checkTypes = rule.GetCheckTypes();

            for (int i = 0; i < checkTypes.Length; i++)
            {
                if (!PassesCheck(npc, rule, checkTypes[i], currentDate, out string reason))
                    return new InspectionDecision(false, reason);
            }
        }

        return new InspectionDecision(true, "No active rule failed.");
    }

    private static bool PassesCheck(
        NPCData npc,
        RuleSO rule,
        RuleCheckType checkType,
        string currentDate,
        out string reason
    )
    {
        reason = rule.ruleName;

        DocumentData passport = npc.passport;
        DocumentData permit = npc.entryPermit;

        switch (checkType)
        {
            case RuleCheckType.None:
                return true;

            case RuleCheckType.PassportRequired:
                return passport != null;

            case RuleCheckType.EntryPermitRequired:
                return permit != null;

            case RuleCheckType.PortraitMatch:
                return passport != null && npc.portrait == passport.portrait;

            case RuleCheckType.NameMatch:
                return TextMatches(npc.koreanName, passport?.koreanName, permit?.koreanName)
                    && TextMatches(npc.englishSurname, passport?.englishSurname)
                    && TextMatches(npc.englishGivenNames, passport?.englishGivenNames);

            case RuleCheckType.GenderMatch:
                return EnumMatches(npc.gender, passport?.gender, permit?.gender);

            case RuleCheckType.AgeMatch:
                return IntMatches(npc.age, passport?.age, permit?.age);

            case RuleCheckType.OccupationMatch:
                return TextMatches(npc.job, passport?.occupation, permit?.occupation);

            case RuleCheckType.ResidenceMatch:
                return TextMatches(npc.address, passport?.residence, permit?.residence);

            case RuleCheckType.DocumentCodeMatch:
                return TextMatches(npc.documentCode, passport?.documentCode, permit?.documentCode);

            case RuleCheckType.PassportCodeMatch:
                return TextMatches(npc.passportCode, passport?.passportCode, permit?.passportCode);

            case RuleCheckType.PassportNotExpired:
                return PassportIsValid(passport, currentDate);

            case RuleCheckType.NoCriminalRecord:
                return !npc.hasCriminalRecord && !(permit?.hasCriminalRecord ?? false);

            case RuleCheckType.NoPsychiatricHistory:
                return string.IsNullOrWhiteSpace(npc.psychiatricHistory)
                    && string.IsNullOrWhiteSpace(permit?.psychiatricHistory);

            default:
                return true;
        }
    }

    private static bool PassportIsValid(DocumentData passport, string currentDate)
    {
        if (passport == null)
            return false;

        if (string.IsNullOrWhiteSpace(passport.passportExpiryDate))
            return false;

        if (string.IsNullOrWhiteSpace(currentDate))
            return false;

        if (!DateTime.TryParse(passport.passportExpiryDate, out DateTime expiry))
            return false;

        if (!DateTime.TryParse(currentDate, out DateTime today))
            return false;

        return expiry.Date >= today.Date;
    }

    private static bool TextMatches(params string[] values)
    {
        string baseline = null;

        foreach (string value in values)
        {
            if (string.IsNullOrWhiteSpace(value))
                continue;

            string normalized = value.Trim();

            if (baseline == null)
            {
                baseline = normalized;
                continue;
            }

            if (!string.Equals(baseline, normalized, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    private static bool IntMatches(params int?[] values)
    {
        int? baseline = null;

        foreach (int? value in values)
        {
            if (!value.HasValue || value.Value <= 0)
                continue;

            if (!baseline.HasValue)
            {
                baseline = value;
                continue;
            }

            if (baseline.Value != value.Value)
                return false;
        }

        return true;
    }

    private static bool EnumMatches<T>(params T?[] values) where T : struct
    {
        T? baseline = null;

        foreach (T? value in values)
        {
            if (!value.HasValue)
                continue;

            if (!baseline.HasValue)
            {
                baseline = value;
                continue;
            }

            if (!EqualityComparer<T>.Default.Equals(baseline.Value, value.Value))
                return false;
        }

        return true;
    }
}
