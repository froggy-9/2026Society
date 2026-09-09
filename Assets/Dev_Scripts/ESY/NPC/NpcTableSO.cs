using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class NpcPhotoSet
{
    [Tooltip("화면에 서 있는 NPC 사진입니다.")]
    public Sprite npcPhoto;

    [Tooltip("npcPhoto와 같은 인물의 여권용 사진입니다. 화면용 이미지와는 크롭이나 그림이 달라도 됩니다.")]
    public Sprite passportPhoto;
}

[System.Serializable]
public class NpcNameSet
{
    [HideInInspector]
    [FormerlySerializedAs("koreanName")]
    public string koreanName;

    [Tooltip("여권에 표시할 영문 성입니다. 예: KIM")]
    public string englishSurname;

    [Tooltip("여권에 표시할 영문 이름입니다. 예: MINJI")]
    public string englishGivenNames;
}

public enum NpcFailReason
{
    // Preserve the IDs stored in Day Data and NPC table assets.
    None = 0,
    MissingPassport = 1,
    PortraitMismatch = 4,
    NameMismatch = 5,
    GenderMismatch = 6,
    OccupationMismatch = 8,
    BirthDateMismatch = 11,
    PassportExpired = 13,
    PassportCodeMismatch = 15,
    NationalityMismatch = 17,
    BannedNationality = 19,
    MissingEntryPermit = 20
}

[System.Serializable]
public class WeightedFailReason
{
    [Tooltip("발생시킬 오류 종류입니다.")]
    public NpcFailReason reason = NpcFailReason.NameMismatch;

    [Min(0f)]
    [Tooltip("가중치입니다. 0이면 선택되지 않습니다.")]
    public float weight = 1f;
}

[System.Serializable]
public class WeightedStringValue
{
    [Tooltip("랜덤으로 선택될 값입니다.")]
    public string value;

    [Min(0f)]
    [Tooltip("선택 가중치입니다. 0이면 선택되지 않습니다.")]
    public float weight = 1f;
}

[System.Serializable]
public class WeightedGenderValue
{
    [Tooltip("랜덤으로 선택될 성별입니다.")]
    public Gender value = Gender.Male;

    [Min(0f)]
    [Tooltip("선택 가중치입니다. 0이면 선택되지 않습니다.")]
    public float weight = 1f;
}

[CreateAssetMenu(
    fileName = "NewNpcTable",
    menuName = "Refugees/NPC Table"
)]
public class NpcTableSO : ScriptableObject
{
    private const int RecentPhotoLimit = 2;
    private readonly Queue<int> recentPhotoIndexes = new Queue<int>();

    [Header("Person")]
    [HideInInspector]
    [FormerlySerializedAs("namePairs")]
    public NpcNameSet[] namePairs;

    [HideInInspector]
    [FormerlySerializedAs("names")]
    public string[] koreanNames;

    [Tooltip("랜덤으로 뽑을 영문 성 목록입니다. 예: KIM, PARK")]
    public string[] englishSurnames;

    [Tooltip("랜덤으로 뽑을 영문 이름 목록입니다. 예: MINJI, SEOYEON")]
    public string[] englishGivenNames;

    [Tooltip("랜덤으로 뽑을 성별 목록입니다.")]
    public Gender[] genders = { Gender.Male, Gender.Female };


    [Tooltip("생년월일 목록입니다. 형식은 yyyy-MM-dd를 권장합니다. 값이 있으면 나이는 이 날짜로 다시 계산됩니다.")]
    public string[] birthDates;

    [Tooltip("국적 목록입니다. 여권 본문에 들어갑니다.")]
    public string[] nationalities;

    [Header("Profile")]
    [Tooltip("직업 목록입니다. 입국허가서/추가 서류에 들어갑니다.")]
    public string[] jobs;

    [Tooltip("거주지 또는 체류 예정 주소 목록입니다.")]
    public string[] addresses;

    [Tooltip("가족관계 목록입니다.")]
    public string[] familyRelationships;

    [Tooltip("병원 내역/병명 목록입니다. 비어 있으면 병력 없음으로 생성됩니다.")]
    public string[] medicalHistories;

    [Header("Passport Values")]
    [Tooltip("여권번호 목록입니다.")]
    public string[] passportNumbers;

    [Tooltip("여권 발급일 목록입니다. 형식은 yyyy-MM-dd를 권장합니다.")]
    public string[] passportIssueDates;

    [Tooltip("정상 여권 만료일 목록입니다. 현재 날짜보다 뒤의 날짜를 넣습니다.")]
    public string[] passportExpiryDates;

    [Tooltip("만료 여권 오류를 만들 때 사용할 만료일 목록입니다. 현재 날짜보다 앞의 날짜를 넣습니다.")]
    public string[] expiredPassportExpiryDates;

    [Header("Medical Certificate Values")]
    [Tooltip("진단서 등록번호 목록입니다.")]
    public string[] medicalRegistrationNumbers;

    [Tooltip("진단서 작성일 목록입니다. 형식은 yyyy-MM-dd를 권장합니다.")]
    public string[] medicalCertificateDates;

    [Tooltip("정상 진단서 유효기간 목록입니다. 현재 날짜보다 뒤의 날짜를 넣습니다.")]
    public string[] medicalCertificateValidUntilDates;

    [Tooltip("만료 진단서 오류를 만들 때 사용할 날짜 목록입니다. 현재 날짜보다 앞의 날짜를 넣습니다.")]
    public string[] expiredMedicalCertificateDates;

    [Header("Photo Pairs")]
    [Tooltip("한 칸은 같은 인물의 화면용 NPC 사진과 여권 사진 한 쌍입니다. 정상 NPC는 이 쌍을 사용하고, 오류 NPC는 여권 사진만 다른 쌍에서 가져옵니다.")]
    [FormerlySerializedAs("photos")]
    public NpcPhotoSet[] photoPairs;

    [Header("Generation Probability")]
    [Range(0f, 1f)]
    [Tooltip("일반 NPC가 불합격 NPC로 생성될 확률입니다.")]
    public float invalidNpcChance = 0.3f;

    [Range(0f, 1f)]
    [Tooltip("일반 NPC 후보가 여권을 제출할 확률입니다.")]
    public float passportSubmissionChance = 0.85f;

    [Range(0f, 1f)]
    [Tooltip("병력이 있는 일반 NPC 후보가 진단서를 제출할 확률입니다.")]
    public float medicalCertificateSubmissionChance = 0.5f;

    [Range(0f, 1f)]
    [Tooltip("NPC가 병원 내역/병명을 가지고 생성될 확률입니다.")]
    public float medicalHistoryChance = 0.2f;

    [Range(0f, 1f)]
    [Tooltip("신고서와 진단서를 모두 가진 후보 NPC의 병명을 서로 다르게 생성할 확률입니다. 하루 O/X 분포 조정 전 추첨 확률입니다.")]
    public float medicalDiagnosisMismatchChance = 0.5f;

    [Tooltip("불합격 NPC일 때 선택될 오류 종류별 가중치입니다. 비워두면 기본 오류 목록에서 랜덤 선택합니다.")]
    public WeightedFailReason[] failReasonWeights;

    [Range(0f, 1f)]
    [Tooltip("불합격 NPC가 오류를 하나 더 가질 확률입니다. 0이면 오류는 한 개만 생깁니다.")]
    public float additionalErrorChance = 0.4f;

    [Min(1)]
    [Tooltip("한 NPC에게 동시에 적용될 수 있는 최대 오류 개수입니다.")]
    public int maxFailReasonsPerNpc = 1;

    [Header("Weighted Person Probability")]
    [Tooltip("성별 등장 확률입니다. 비워두면 Genders 배열에서 균등 랜덤으로 뽑습니다.")]
    public WeightedGenderValue[] weightedGenders;

    [Tooltip("국가별 등장 확률입니다. 비워두면 Nationalities 배열에서 균등 랜덤으로 뽑습니다.")]
    public WeightedStringValue[] weightedNationalities;

    [Tooltip("직업별 등장 확률입니다. 비워두면 Jobs 배열에서 균등 랜덤으로 뽑습니다.")]
    public WeightedStringValue[] weightedJobs;

    [Tooltip("가족관계 유형 등장 확률입니다. 비워두면 Family Relationships 배열에서 균등 랜덤으로 뽑습니다.")]
    public WeightedStringValue[] weightedFamilyRelationships;

    [Tooltip("병명별 확률입니다. 비워두면 Medical Histories 배열에서 균등 랜덤으로 뽑습니다.")]
    public WeightedStringValue[] weightedMedicalHistories;

    [Header("Weighted Document Probability")]
    [Tooltip("여권번호 선택 확률입니다. 비워두면 Passport Numbers 배열에서 균등 랜덤으로 뽑습니다.")]
    public WeightedStringValue[] weightedPassportNumbers;

    [Tooltip("여권 발급일 선택 확률입니다. 비워두면 Passport Issue Dates 배열에서 균등 랜덤으로 뽑습니다.")]
    public WeightedStringValue[] weightedPassportIssueDates;

    [Tooltip("여권 만료일 선택 확률입니다. 비워두면 Passport Expiry Dates 배열에서 균등 랜덤으로 뽑습니다.")]
    public WeightedStringValue[] weightedPassportExpiryDates;

    [Tooltip("진단서 등록번호 선택 확률입니다. 비워두면 Medical Registration Numbers 배열에서 균등 랜덤으로 뽑습니다.")]
    public WeightedStringValue[] weightedMedicalRegistrationNumbers;

    [Tooltip("진단서 작성일 선택 확률입니다. 비워두면 Medical Certificate Dates 배열에서 균등 랜덤으로 뽑습니다.")]
    public WeightedStringValue[] weightedMedicalCertificateDates;

    [Tooltip("진단서 유효기간 선택 확률입니다. 비워두면 Medical Certificate Valid Until Dates 배열에서 균등 랜덤으로 뽑습니다.")]
    public WeightedStringValue[] weightedMedicalCertificateValidUntilDates;

    public void ResetRuntimeHistory()
    {
        recentPhotoIndexes.Clear();
    }

    public NPCData CreateRandomNpc(string currentDate, NpcFailReason[] allowedFailReasons, RuleSO rule = null)
    {
        NpcFailReason reason = allowedFailReasons == null || allowedFailReasons.Length == 0
            ? NpcFailReason.None : PickFailReason(allowedFailReasons);
        return CreateRandomNpc(currentDate, reason, allowedFailReasons, rule);
    }

    public NPCData CreateRandomNpc(string currentDate, NpcFailReason failReason)
    {
        return CreateRandomNpc(currentDate, failReason, null);
    }

    public NPCData CreateNpcForDecision(DayDataSO day, RuleSO inspectionRule, bool shouldApprove)
    {
        int[] photoHistory = recentPhotoIndexes.ToArray();
        var rules = new[] { inspectionRule };
        // Rejected candidates must not consume the visible NPC photo history.
        for (int attempt = 0; attempt < 256; attempt++)
        {
            recentPhotoIndexes.Clear();
            foreach (int index in photoHistory)
                recentPhotoIndexes.Enqueue(index);

            NpcFailReason reason = shouldApprove ? NpcFailReason.None : PickWeightedFailReason(day.rejectReasons);
            NPCData candidate = CreateRandomNpc(day.currentDate, reason, day.rejectReasons, day.rule);
            if (InspectionJudge.Evaluate(candidate, rules, day.currentDate).shouldApprove == shouldApprove)
                return candidate;
        }

        recentPhotoIndexes.Clear();
        foreach (int index in photoHistory)
            recentPhotoIndexes.Enqueue(index);
        throw new InvalidOperationException($"NpcTable '{name}': could not generate an {(shouldApprove ? "approved" : "rejected")} NPC for Day {day.day}. Check document data and inspection rules.");
    }

    private NPCData CreateRandomNpc(string currentDate, NpcFailReason failReason, NpcFailReason[] allowedFailReasons, RuleSO rule = null)
    {
        NPCData npc = new NPCData();
        DocumentData passport = CreateDocument(DocumentType.Passport);
        DocumentData entryPermit = CreateDocument(DocumentType.EntryPermit);
        DocumentData medicalCertificate = CreateDocument(DocumentType.MedicalCertificate);

        npc.englishSurname = PickEnglishSurname();
        npc.englishGivenNames = PickEnglishGivenNames();
        npc.gender = PickWeightedGender(weightedGenders, Pick(genders, Gender.Male));
        npc.nationality = PickWeightedString(weightedNationalities, nationalities, "Stateless");
        npc.dateOfBirth = PickRequiredBirthDate();
        NpcPhotoSet photoSet = PickPhotoSet();
        npc.portrait = GetNpcPhoto(photoSet);
        npc.passportPhotoMatchesNpc = true;
        npc.job = PickWeightedString(weightedJobs, jobs, "None");
        npc.address = PickAddressForNationality(npc.nationality);
        npc.family = new[] { PickWeightedString(weightedFamilyRelationships, familyRelationships, "None") };
        npc.psychiatricHistory = UnityEngine.Random.value <= medicalHistoryChance
            ? PickWeightedString(weightedMedicalHistories, medicalHistories, string.Empty)
            : string.Empty;
        if (!npc.HasMedicalHistory)
            npc.psychiatricHistory = string.Empty;
        npc.passportCode = PickWeightedString(weightedPassportNumbers, passportNumbers, CreateCode("PAS"));
        npc.passport = passport;
        npc.entryPermit = entryPermit;
        npc.medicalCertificate = npc.HasMedicalHistory ? medicalCertificate : null;

        CopyNpcToDocument(passport, npc, currentDate);
        CopyNpcToDocument(entryPermit, npc, currentDate);
        CopyNpcToDocument(medicalCertificate, npc, currentDate);
        passport.portrait = GetPassportPhoto(photoSet);

        ApplyFailReason(npc, currentDate, failReason, rule);
        ApplyAdditionalFailReasons(npc, currentDate, failReason, allowedFailReasons, rule);
        ApplySubmissionChance(npc);
        ApplyMedicalDiagnosisMismatch(npc);

        return npc;
    }

    private DocumentData CreateDocument(DocumentType type)
    {
        DocumentData document = new DocumentData();
        document.documentType = type;
        return document;
    }

    private void CopyNpcToDocument(DocumentData document, NPCData npc, string currentDate)
    {
        if (document == null)
            return;

        document.englishSurname = npc.englishSurname;
        document.englishGivenNames = npc.englishGivenNames;
        document.gender = npc.gender;
        document.portrait = npc.portrait;
        document.nationality = npc.nationality;
        document.dateOfBirth = npc.dateOfBirth;
        document.occupation = npc.job;
        document.residence = npc.address;
        document.familyRelationship = npc.family != null && npc.family.Length > 0 ? npc.family[0] : string.Empty;
        document.passportCode = npc.passportCode;
        document.psychiatricHistory = npc.psychiatricHistory;
        document.medicalDiagnosis = npc.psychiatricHistory;
        document.issueDate = PickWeightedString(weightedPassportIssueDates, passportIssueDates, CreateIssueDate(currentDate));
        document.passportExpiryDate = PickWeightedString(weightedPassportExpiryDates, passportExpiryDates, CreateExpiryDate(currentDate));
        document.registrationNumber = PickWeightedString(weightedMedicalRegistrationNumbers, medicalRegistrationNumbers, CreateCode("MED"));
        document.medicalCertificateDate = PickWeightedString(weightedMedicalCertificateDates, medicalCertificateDates, CreateIssueDate(currentDate));
        document.medicalCertificateValidUntil = PickWeightedString(weightedMedicalCertificateValidUntilDates, medicalCertificateValidUntilDates, CreateExpiryDate(currentDate));
    }

    public NpcFailReason PickFailReason(NpcFailReason[] reasons)
    {
        if (UnityEngine.Random.value > invalidNpcChance)
            return NpcFailReason.None;

        return PickWeightedFailReason(reasons);
    }

    private NpcPhotoSet PickPhotoSet()
    {
        if (photoPairs != null && photoPairs.Length > 0)
        {
            int pairIndex = PickPhotoIndex(photoPairs.Length);
            return pairIndex >= 0 ? photoPairs[pairIndex] : null;
        }

        return null;
    }

    private int PickPhotoIndex(int count)
    {
        int startIndex = UnityEngine.Random.Range(0, count);

        for (int offset = 0; offset < count; offset++)
        {
            int index = (startIndex + offset) % count;

            if (count > RecentPhotoLimit && ContainsRecentPhoto(index))
                continue;

            AddRecentPhoto(index);
            return index;
        }

        return -1;
    }

    private bool ContainsRecentPhoto(int index)
    {
        foreach (int recentIndex in recentPhotoIndexes)
        {
            if (recentIndex == index)
                return true;
        }

        return false;
    }

    private void AddRecentPhoto(int index)
    {
        recentPhotoIndexes.Enqueue(index);

        while (recentPhotoIndexes.Count > RecentPhotoLimit)
            recentPhotoIndexes.Dequeue();
    }

    private void ApplyFailReason(NPCData npc, string currentDate, NpcFailReason failReason, RuleSO rule)
    {
        if (npc == null || failReason == NpcFailReason.None)
            return;

        switch (failReason)
        {
            case NpcFailReason.BannedNationality:
                string banned = Pick(rule != null ? rule.bannedNationalities : null, string.Empty);
                if (!string.IsNullOrWhiteSpace(banned))
                {
                    npc.nationality = banned;
                    npc.address = PickAddressForNationality(banned);
                    if (npc.passport != null) npc.passport.nationality = banned;
                    if (npc.entryPermit != null)
                    {
                        npc.entryPermit.nationality = banned;
                        npc.entryPermit.residence = npc.address;
                    }
                }
                break;

            case NpcFailReason.MissingPassport:
                npc.passport = null;
                break;

            case NpcFailReason.MissingEntryPermit:
                npc.entryPermit = null;
                break;

            case NpcFailReason.NationalityMismatch:
                if (npc.entryPermit != null)
                    npc.entryPermit.residence = PickAddressForNationality(npc.nationality, true);
                break;

            case NpcFailReason.PortraitMismatch:
                ApplyInvalidPassportPhoto(npc);
                break;

            case NpcFailReason.NameMismatch:
                ApplyNameMismatch(npc);
                break;

            case NpcFailReason.GenderMismatch:
                if (npc.entryPermit != null)
                    npc.entryPermit.gender = npc.gender == Gender.Male ? Gender.Female : Gender.Male;
                break;


            case NpcFailReason.OccupationMismatch:
                if (npc.entryPermit != null)
                    npc.entryPermit.occupation = PickDifferentText(npc.job, jobs, "Unknown");
                break;

            case NpcFailReason.BirthDateMismatch:
                if (npc.entryPermit != null)
                {
                    npc.entryPermit.dateOfBirth = PickDifferentValue(npc.dateOfBirth, birthDates, "Birth Dates", true);
                }
                break;

            case NpcFailReason.PassportExpired:
                if (npc.passport != null)
                    npc.passport.passportExpiryDate = Pick(expiredPassportExpiryDates, CreateExpiredDate(currentDate));
                break;

            case NpcFailReason.PassportCodeMismatch:
                if (npc.entryPermit != null)
                    npc.entryPermit.passportCode = PickDifferentValue(npc.passportCode, passportNumbers, "Passport Numbers");
                break;

        }
    }

    private string PickAddressForNationality(string country, bool differentCountry = false)
    {
        var candidates = new System.Collections.Generic.List<string>();
        if (addresses != null && nationalities != null)
        {
            foreach (string address in addresses)
            {
                foreach (string registeredCountry in nationalities)
                {
                    if (!NPCData.AddressMatchesNationality(registeredCountry, address))
                        continue;
                    bool matches = string.Equals(registeredCountry.Trim(), country?.Trim(), StringComparison.OrdinalIgnoreCase);
                    if (matches != differentCountry)
                        candidates.Add(address.Trim());
                    break;
                }
            }
        }

        if (candidates.Count == 0)
            throw new InvalidOperationException($"NpcTable '{name}': no configured {(differentCountry ? "foreign" : "matching")} address for '{country}'.");

        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }

    private void ApplyInvalidPassportPhoto(NPCData npc)
    {
        if (npc.passport == null || !npc.passportPhotoMatchesNpc)
            return;

        npc.passport.portrait = PickDifferentPortrait(npc.passport.portrait);
        npc.passportPhotoMatchesNpc = false;
    }

    private void ApplySubmissionChance(NPCData npc)
    {
        if (UnityEngine.Random.value >= passportSubmissionChance)
            npc.passport = null;
        if (UnityEngine.Random.value >= medicalCertificateSubmissionChance)
            npc.medicalCertificate = null;
    }

    private void ApplyMedicalDiagnosisMismatch(NPCData npc)
    {
        if (npc.entryPermit == null || npc.medicalCertificate == null
            || !NPCData.HasMedicalRecord(npc.entryPermit.psychiatricHistory)
            || UnityEngine.Random.value >= medicalDiagnosisMismatchChance)
            return;

        var diagnoses = new List<string>();
        if (medicalHistories != null)
            foreach (string diagnosis in medicalHistories)
                if (NPCData.HasMedicalRecord(diagnosis))
                    diagnoses.Add(diagnosis);

        npc.medicalCertificate.medicalDiagnosis = PickDifferentValue(
            npc.entryPermit.psychiatricHistory, diagnoses.ToArray(), "Medical Histories");
    }

    private void ApplyAdditionalFailReasons(
        NPCData npc,
        string currentDate,
        NpcFailReason primaryFailReason,
        NpcFailReason[] allowedFailReasons,
        RuleSO rule)
    {
        if (npc == null || primaryFailReason == NpcFailReason.None)
            return;

        int maxErrors = Mathf.Max(1, maxFailReasonsPerNpc);

        for (int i = 1; i < maxErrors; i++)
        {
            if (UnityEngine.Random.value > additionalErrorChance)
                break;

            NpcFailReason nextReason = PickExtraFailReason(primaryFailReason, allowedFailReasons);

            if (nextReason == NpcFailReason.None)
                break;

            ApplyFailReason(npc, currentDate, nextReason, rule);
        }
    }

    private NpcFailReason PickExtraFailReason(NpcFailReason primaryFailReason, NpcFailReason[] allowedFailReasons)
    {
        return PickWeightedFailReason(allowedFailReasons, primaryFailReason);
    }

    private Sprite GetNpcPhoto(NpcPhotoSet photoSet)
    {
        if (photoSet == null)
            return null;

        return photoSet.npcPhoto != null ? photoSet.npcPhoto : photoSet.passportPhoto;
    }

    private Sprite GetPassportPhoto(NpcPhotoSet photoSet)
    {
        if (photoSet == null)
            return null;

        return photoSet.passportPhoto != null ? photoSet.passportPhoto : photoSet.npcPhoto;
    }

    private Sprite PickDifferentPortrait(Sprite currentPortrait)
    {
        Sprite portrait = PickAnyPassportPortrait();

        if (portrait == null || portrait != currentPortrait)
            return portrait;

        if (photoPairs != null && photoPairs.Length > 0)
        {
            for (int i = 0; i < photoPairs.Length; i++)
            {
                Sprite candidate = GetPassportPhoto(photoPairs[i]);

                if (candidate != null && candidate != currentPortrait)
                    return candidate;
            }
        }

        return portrait;
    }

    private Sprite PickAnyPassportPortrait()
    {
        if (photoPairs != null && photoPairs.Length > 0)
            return GetPassportPhoto(photoPairs[UnityEngine.Random.Range(0, photoPairs.Length)]);

        return null;
    }

    private string PickDifferentEnglishSurname(string currentName)
    {
        string[] values = GetEnglishSurnameValues();
        return PickDifferentValue(currentName, values, "English Surnames");
    }

    private string PickDifferentEnglishGivenNames(string currentName)
    {
        string[] values = GetEnglishGivenNameValues();
        return PickDifferentValue(currentName, values, "English Given Names");
    }

    private void ApplyNameMismatch(NPCData npc)
    {
        DocumentData target = npc.passport ?? npc.entryPermit ?? npc.medicalCertificate;

        if (target == null)
            return;

        target.englishSurname = PickDifferentEnglishSurname(npc.englishSurname);
        target.englishGivenNames = PickDifferentEnglishGivenNames(npc.englishGivenNames);
    }

    private static string PickDifferentText(string currentValue, string[] values, string fallback)
    {
        string picked = PickDifferentName(currentValue, values);

        if (!string.IsNullOrWhiteSpace(picked) && !string.Equals(picked, currentValue, StringComparison.OrdinalIgnoreCase))
            return picked;

        return fallback;
    }

    private static string PickDifferentName(string currentName, string[] values)
    {
        return PickDifferentValue(currentName, values, "Values");
    }

    private static string PickDifferentValue(string current, string[] values, string field, bool compareDates = false)
    {
        var candidates = new List<string>();
        var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        DateTime currentDate = default;
        if (compareDates && !DateTime.TryParse(current, out currentDate))
            throw new InvalidOperationException($"NpcTable {field}: invalid current date '{current}'.");

        if (values != null)
        {
            foreach (string value in values)
            {
                if (string.IsNullOrWhiteSpace(value))
                    continue;
                string candidate = value.Trim();
                string key = candidate;
                if (compareDates)
                {
                    if (!DateTime.TryParse(candidate, out DateTime date))
                        throw new InvalidOperationException($"NpcTable {field}: invalid date '{candidate}'.");
                    if (date.Date == currentDate.Date)
                        continue;
                    key = date.ToString("yyyy-MM-dd");
                }
                else if (string.Equals(candidate, current?.Trim(), StringComparison.OrdinalIgnoreCase))
                    continue;

                if (unique.Add(key))
                    candidates.Add(candidate);
            }
        }

        if (candidates.Count == 0)
            throw new InvalidOperationException($"NpcTable {field}: configure a value different from '{current}'.");
        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }

    private static NpcFailReason PickDefaultFailReason()
    {
        NpcFailReason[] defaults =
        {
            NpcFailReason.MissingPassport,
            NpcFailReason.MissingEntryPermit,
            NpcFailReason.PortraitMismatch,
            NpcFailReason.NameMismatch,
            NpcFailReason.GenderMismatch,
            NpcFailReason.OccupationMismatch,
            NpcFailReason.BirthDateMismatch,
            NpcFailReason.PassportExpired,
            NpcFailReason.PassportCodeMismatch,
            NpcFailReason.NationalityMismatch
        };

        return defaults[UnityEngine.Random.Range(0, defaults.Length)];
    }

    private static string CreateCode(string prefix)
    {
        return $"{prefix}-{UnityEngine.Random.Range(100000, 999999)}";
    }

    private static string CreateExpiryDate(string currentDate)
    {
        DateTime date = DateTime.Today;

        if (!string.IsNullOrWhiteSpace(currentDate) && !DateTime.TryParse(currentDate, out date))
            date = DateTime.Today;

        return date.AddDays(UnityEngine.Random.Range(30, 420)).ToString("yyyy-MM-dd");
    }

    private static string CreateIssueDate(string currentDate)
    {
        DateTime date = DateTime.Today;

        if (!string.IsNullOrWhiteSpace(currentDate) && !DateTime.TryParse(currentDate, out date))
            date = DateTime.Today;

        return date.AddDays(-UnityEngine.Random.Range(30, 730)).ToString("yyyy-MM-dd");
    }

    private string PickRequiredBirthDate()
    {
        if (birthDates == null || birthDates.Length == 0)
            throw new InvalidOperationException($"NpcTable '{name}': Birth Dates must be configured.");
        string value = birthDates[UnityEngine.Random.Range(0, birthDates.Length)];
        if (!DateTime.TryParse(value, out _))
            throw new InvalidOperationException($"NpcTable '{name}': invalid birth date '{value}'.");
        return value;
    }

    private static string CreateExpiredDate(string currentDate)
    {
        DateTime date = DateTime.Today;

        if (!string.IsNullOrWhiteSpace(currentDate) && !DateTime.TryParse(currentDate, out date))
            date = DateTime.Today;

        return date.AddDays(-UnityEngine.Random.Range(1, 120)).ToString("yyyy-MM-dd");
    }

    private static T Pick<T>(T[] values, T fallback)
    {
        if (values == null || values.Length == 0)
            return fallback;

        return values[UnityEngine.Random.Range(0, values.Length)];
    }

    private static string PickWeightedString(WeightedStringValue[] weightedValues, string[] fallbackValues, string fallback)
    {
        if (weightedValues == null || weightedValues.Length == 0)
            return Pick(fallbackValues, fallback);

        float totalWeight = 0f;

        for (int i = 0; i < weightedValues.Length; i++)
        {
            if (weightedValues[i] == null || string.IsNullOrWhiteSpace(weightedValues[i].value))
                continue;

            totalWeight += Mathf.Max(0f, weightedValues[i].weight);
        }

        if (totalWeight <= 0f)
            return Pick(fallbackValues, fallback);

        float point = UnityEngine.Random.value * totalWeight;

        for (int i = 0; i < weightedValues.Length; i++)
        {
            WeightedStringValue weighted = weightedValues[i];

            if (weighted == null || string.IsNullOrWhiteSpace(weighted.value))
                continue;

            point -= Mathf.Max(0f, weighted.weight);

            if (point <= 0f)
                return weighted.value;
        }

        return Pick(fallbackValues, fallback);
    }

    private static Gender PickWeightedGender(WeightedGenderValue[] weightedValues, Gender fallback)
    {
        if (weightedValues == null || weightedValues.Length == 0)
            return fallback;

        float totalWeight = 0f;

        for (int i = 0; i < weightedValues.Length; i++)
        {
            if (weightedValues[i] == null)
                continue;

            totalWeight += Mathf.Max(0f, weightedValues[i].weight);
        }

        if (totalWeight <= 0f)
            return fallback;

        float point = UnityEngine.Random.value * totalWeight;

        for (int i = 0; i < weightedValues.Length; i++)
        {
            WeightedGenderValue weighted = weightedValues[i];

            if (weighted == null)
                continue;

            point -= Mathf.Max(0f, weighted.weight);

            if (point <= 0f)
                return weighted.value;
        }

        return fallback;
    }

    private NpcNameSet PickNameSet()
    {
        if (namePairs != null && namePairs.Length > 0)
            return namePairs[UnityEngine.Random.Range(0, namePairs.Length)];

        return new NpcNameSet
        {
            englishSurname = Pick(englishSurnames, "UNKNOWN"),
            englishGivenNames = Pick(englishGivenNames, "UNKNOWN")
        };
    }

    private NpcFailReason PickWeightedFailReason(NpcFailReason[] allowed = null, NpcFailReason excluded = NpcFailReason.None)
    {
        if (failReasonWeights == null || failReasonWeights.Length == 0)
        {
            if (allowed == null || allowed.Length == 0)
                return PickDefaultFailReason();
            var candidates = new List<NpcFailReason>();
            foreach (NpcFailReason reason in allowed)
                if (reason != NpcFailReason.None && reason != excluded && !candidates.Contains(reason))
                    candidates.Add(reason);
            return candidates.Count == 0 ? NpcFailReason.None : candidates[UnityEngine.Random.Range(0, candidates.Count)];
        }

        float totalWeight = 0f;

        for (int i = 0; i < failReasonWeights.Length; i++)
        {
            if (!IsEligibleFailReason(failReasonWeights[i], allowed, excluded))
                continue;

            totalWeight += Mathf.Max(0f, failReasonWeights[i].weight);
        }

        if (totalWeight <= 0f)
            throw new InvalidOperationException($"NpcTable '{name}': no positive error weight for the active inspection items.");

        float point = UnityEngine.Random.value * totalWeight;

        for (int i = 0; i < failReasonWeights.Length; i++)
        {
            WeightedFailReason weighted = failReasonWeights[i];

            if (!IsEligibleFailReason(weighted, allowed, excluded))
                continue;

            point -= Mathf.Max(0f, weighted.weight);

            if (point < 0f)
                return weighted.reason;
        }

        for (int i = failReasonWeights.Length - 1; i >= 0; i--)
            if (IsEligibleFailReason(failReasonWeights[i], allowed, excluded))
                return failReasonWeights[i].reason;
        return NpcFailReason.None;
    }

    private static bool IsEligibleFailReason(WeightedFailReason entry, NpcFailReason[] allowed, NpcFailReason excluded)
    {
        return entry != null && entry.weight > 0f && entry.reason != NpcFailReason.None
            && entry.reason != excluded
            && (allowed == null || allowed.Length == 0 || Array.IndexOf(allowed, entry.reason) >= 0);
    }

    private string PickEnglishSurname()
    {
        string value = Pick(englishSurnames, string.Empty);

        if (!string.IsNullOrWhiteSpace(value))
            return value;

        return GetEnglishSurname(PickNameSet());
    }

    private string PickEnglishGivenNames()
    {
        string value = Pick(englishGivenNames, string.Empty);

        if (!string.IsNullOrWhiteSpace(value))
            return value;

        return GetEnglishGivenNames(PickNameSet());
    }

    private static string GetEnglishSurname(NpcNameSet nameSet)
    {
        return nameSet != null && !string.IsNullOrWhiteSpace(nameSet.englishSurname)
            ? nameSet.englishSurname
            : "UNKNOWN";
    }

    private static string GetEnglishGivenNames(NpcNameSet nameSet)
    {
        return nameSet != null && !string.IsNullOrWhiteSpace(nameSet.englishGivenNames)
            ? nameSet.englishGivenNames
            : "UNKNOWN";
    }

    private string[] GetEnglishSurnameValues()
    {
        return englishSurnames;
    }

    private string[] GetEnglishGivenNameValues()
    {
        return englishGivenNames;
    }
}
