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
        DocumentData medicalCertificate = npc.medicalCertificate;

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
                return TextMatches(npc.englishSurname, passport?.englishSurname, permit?.englishSurname, medicalCertificate?.englishSurname)
                    && TextMatches(npc.englishGivenNames, passport?.englishGivenNames, permit?.englishGivenNames, medicalCertificate?.englishGivenNames);

            case RuleCheckType.GenderMatch:
                return EnumMatches(npc.gender, passport?.gender, permit?.gender);

            case RuleCheckType.AgeMatch:
                return IntMatches(passport?.age, permit?.age);

            case RuleCheckType.BirthDateMatch:
                return TextMatches(npc.dateOfBirth, passport?.dateOfBirth, permit?.dateOfBirth);

            case RuleCheckType.OccupationMatch:
                if (!ShouldInspectOccupation(npc, rule))
                    return true;
                return TextMatches(npc.job, passport?.occupation, permit?.occupation);

            case RuleCheckType.DocumentCodeMatch:
                return TextMatches(npc.documentCode, passport?.documentCode, permit?.documentCode);

            case RuleCheckType.PassportCodeMatch:
                return TextMatches(npc.passportCode, passport?.passportCode, permit?.passportCode);

            case RuleCheckType.PassportNotExpired:
                return PassportIsValid(passport, currentDate);

            case RuleCheckType.NoCriminalRecord:
                return !npc.hasCriminalRecord && !(permit?.hasCriminalRecord ?? false);

            case RuleCheckType.NationalityAllowed:
                if (rule.bannedNationalities == null)
                    return true;

                foreach (string banned in rule.bannedNationalities)
                {
                    if (string.IsNullOrWhiteSpace(banned))
                        continue;

                    if (IsSameText(banned, npc.nationality)
                        || IsSameText(banned, passport?.nationality)
                        || IsSameText(banned, permit?.nationality))
                        return false;
                }

                return true;

            case RuleCheckType.NationalityMatch:
                return passport != null && permit != null
                    && IsSameText(passport.nationality, permit.nationality);

            default:
                return true;
        }
    }

    private static bool IsSameText(string left, string right)
    {
        return !string.IsNullOrWhiteSpace(left) && !string.IsNullOrWhiteSpace(right)
            && string.Equals(left.Trim(), right.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static bool ShouldInspectOccupation(NPCData npc, RuleSO rule)
    {
        if (rule.inspectedOccupations == null || rule.inspectedOccupations.Length == 0)
            return true;

        foreach (string occupation in rule.inspectedOccupations)
        {
            if (IsSameText(occupation, npc.job)
                || IsSameText(occupation, npc.passport?.occupation)
                || IsSameText(occupation, npc.entryPermit?.occupation))
                return true;
        }

        return false;
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
