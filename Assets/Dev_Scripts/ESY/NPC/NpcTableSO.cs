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
    AgeMismatch = 7,
    OccupationMismatch = 8,
    BirthDateMismatch = 11,
    PassportExpired = 13,
    PassportCodeMismatch = 15,
    DocumentCodeMismatch = 16,
    NationalityMismatch = 17,
    CriminalRecord = 18,
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

    [Tooltip("나이를 직접 목록으로 관리하고 싶을 때 넣습니다. 비워두면 Min Age~Max Age 사이에서 뽑습니다.")]
    public int[] ages;

    [Tooltip("Ages가 비어 있을 때 사용할 최소 나이입니다.")]
    public int minAge = 18;

    [Tooltip("Ages가 비어 있을 때 사용할 최대 나이입니다.")]
    public int maxAge = 70;

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
    [Tooltip("일반 NPC가 여권을 가지고 올 확률입니다. 입국 신고서는 항상 제출합니다.")]
    public float passportSubmissionChance = 0.8f;

    [Range(0f, 1f)]
    [Tooltip("NPC가 병원 내역/병명을 가지고 생성될 확률입니다.")]
    public float medicalHistoryChance = 0.2f;

    [Range(0f, 1f)]
    [Tooltip("병원 내역이 있는 일반 NPC가 진단서를 가지고 올 확률입니다. 병원 내역이 없으면 제출하지 않습니다.")]
    public float medicalCertificateSubmissionChance = 0.8f;

    [Tooltip("불합격 NPC일 때 선택될 오류 종류별 가중치입니다. 비워두면 기본 오류 목록에서 랜덤 선택합니다.")]
    public WeightedFailReason[] failReasonWeights;

    [Range(0f, 1f)]
    [Tooltip("불합격 NPC가 오류를 하나 더 가질 확률입니다. 0이면 오류는 한 개만 생깁니다.")]
    public float additionalErrorChance = 0f;

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

    private NPCData CreateRandomNpc(string currentDate, NpcFailReason failReason, NpcFailReason[] allowedFailReasons, RuleSO rule = null)
    {
        NPCData npc = new NPCData();
        DocumentData passport = CreateDocument(DocumentType.Passport);
        DocumentData entryPermit = CreateDocument(DocumentType.EntryPermit);
        DocumentData medicalCertificate = CreateDocument(DocumentType.MedicalCertificate);

        npc.englishSurname = PickEnglishSurname();
        npc.englishGivenNames = PickEnglishGivenNames();
        npc.gender = PickWeightedGender(weightedGenders, Pick(genders, Gender.Male));
        npc.age = PickAge();
        npc.nationality = PickWeightedString(weightedNationalities, nationalities, "Stateless");
        npc.dateOfBirth = Pick(birthDates, CreateBirthDate(npc.age, currentDate));
        npc.age = GetAgeFromBirthDate(npc.dateOfBirth, currentDate, npc.age);
        NpcPhotoSet photoSet = PickPhotoSet();
        npc.portrait = GetNpcPhoto(photoSet);
        npc.passportPhotoMatchesNpc = true;
        npc.job = PickWeightedString(weightedJobs, jobs, "None");
        npc.address = Pick(addresses, "Unknown");
        npc.family = new[] { PickWeightedString(weightedFamilyRelationships, familyRelationships, "None") };
        npc.psychiatricHistory = UnityEngine.Random.value <= medicalHistoryChance
            ? PickWeightedString(weightedMedicalHistories, medicalHistories, string.Empty)
            : string.Empty;
        npc.documentCode = CreateCode("DOC");
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
        document.age = npc.age;
        document.portrait = npc.portrait;
        document.nationality = npc.nationality;
        document.dateOfBirth = npc.dateOfBirth;
        document.occupation = npc.job;
        document.residence = npc.address;
        document.familyRelationship = npc.family != null && npc.family.Length > 0 ? npc.family[0] : string.Empty;
        document.documentCode = npc.documentCode;
        document.passportCode = npc.passportCode;
        document.hasCriminalRecord = npc.hasCriminalRecord;
        document.criminalRecordDetails = npc.criminalRecordDetails;
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

        if (reasons == null || reasons.Length == 0)
            return PickWeightedFailReason();

        return reasons[UnityEngine.Random.Range(0, reasons.Length)];
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

        ApplyInvalidPassportPhoto(npc);

        switch (failReason)
        {
            case NpcFailReason.BannedNationality:
                string banned = Pick(rule != null ? rule.bannedNationalities : null, string.Empty);
                if (!string.IsNullOrWhiteSpace(banned))
                {
                    npc.nationality = banned;
                    if (npc.passport != null) npc.passport.nationality = banned;
                    if (npc.entryPermit != null) npc.entryPermit.nationality = banned;
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
                    npc.entryPermit.nationality = PickDifferentText(npc.nationality, nationalities, "Unknown");
                break;

            case NpcFailReason.CriminalRecord:
                npc.hasCriminalRecord = true;
                npc.criminalRecordDetails = "Criminal record";
                if (npc.entryPermit != null)
                {
                    npc.entryPermit.hasCriminalRecord = true;
                    npc.entryPermit.criminalRecordDetails = npc.criminalRecordDetails;
                }
                break;

            case NpcFailReason.PortraitMismatch:
                break;

            case NpcFailReason.NameMismatch:
                ApplyNameMismatch(npc);
                break;

            case NpcFailReason.GenderMismatch:
                if (npc.entryPermit != null)
                    npc.entryPermit.gender = npc.gender == Gender.Male ? Gender.Female : Gender.Male;
                break;

            case NpcFailReason.AgeMismatch:
                if (npc.entryPermit != null)
                    npc.entryPermit.age = Mathf.Max(0, npc.age + UnityEngine.Random.Range(1, 6));
                break;

            case NpcFailReason.OccupationMismatch:
                if (npc.entryPermit != null)
                    npc.entryPermit.occupation = PickDifferentText(npc.job, jobs, "Unknown");
                break;

            case NpcFailReason.BirthDateMismatch:
                if (npc.entryPermit != null)
                {
                    npc.entryPermit.dateOfBirth = CreateBirthDate(Mathf.Max(0, npc.age + UnityEngine.Random.Range(1, 5)), currentDate);
                    npc.entryPermit.age = GetAgeFromBirthDate(npc.entryPermit.dateOfBirth, currentDate, npc.entryPermit.age);
                }
                break;

            case NpcFailReason.PassportExpired:
                if (npc.passport != null)
                    npc.passport.passportExpiryDate = Pick(expiredPassportExpiryDates, CreateExpiredDate(currentDate));
                break;

            case NpcFailReason.PassportCodeMismatch:
                if (npc.entryPermit != null)
                    npc.entryPermit.passportCode = CreateCode("PAS");
                break;

            case NpcFailReason.DocumentCodeMismatch:
                if (npc.entryPermit != null)
                    npc.entryPermit.documentCode = CreateCode("DOC");
                break;
        }
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
        if (npc == null)
            return;

        if (passportSubmissionChance < 1f && UnityEngine.Random.value >= passportSubmissionChance)
            npc.passport = null;

        if (medicalCertificateSubmissionChance < 1f && UnityEngine.Random.value >= medicalCertificateSubmissionChance)
            npc.medicalCertificate = null;
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
        for (int i = 0; i < 8; i++)
        {
            NpcFailReason nextReason = allowedFailReasons != null && allowedFailReasons.Length > 0
                ? allowedFailReasons[UnityEngine.Random.Range(0, allowedFailReasons.Length)]
                : PickWeightedFailReason();

            if (nextReason != NpcFailReason.None && nextReason != primaryFailReason)
                return nextReason;
        }

        return NpcFailReason.None;
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
        return PickDifferentName(currentName, values);
    }

    private string PickDifferentEnglishGivenNames(string currentName)
    {
        string[] values = GetEnglishGivenNameValues();
        return PickDifferentName(currentName, values);
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
        if (values == null || values.Length == 0)
            return currentName + " ?";

        for (int i = 0; i < values.Length; i++)
        {
            string name = values[UnityEngine.Random.Range(0, values.Length)];

            if (!string.Equals(name, currentName, StringComparison.OrdinalIgnoreCase))
                return name;
        }

        return currentName + " ?";
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
            NpcFailReason.AgeMismatch,
            NpcFailReason.OccupationMismatch,
            NpcFailReason.BirthDateMismatch,
            NpcFailReason.PassportExpired,
            NpcFailReason.PassportCodeMismatch,
            NpcFailReason.DocumentCodeMismatch,
            NpcFailReason.NationalityMismatch,
            NpcFailReason.CriminalRecord
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

    private static string CreateBirthDate(int age, string currentDate)
    {
        DateTime date = DateTime.Today;

        if (!string.IsNullOrWhiteSpace(currentDate) && !DateTime.TryParse(currentDate, out date))
            date = DateTime.Today;

        return date.AddYears(-Mathf.Max(0, age)).AddDays(-UnityEngine.Random.Range(0, 365)).ToString("yyyy-MM-dd");
    }

    private int PickAge()
    {
        if (ages != null && ages.Length > 0)
            return ages[UnityEngine.Random.Range(0, ages.Length)];

        return UnityEngine.Random.Range(Mathf.Min(minAge, maxAge), Mathf.Max(minAge, maxAge) + 1);
    }

    private static int GetAgeFromBirthDate(string birthDate, string currentDate, int fallbackAge)
    {
        if (string.IsNullOrWhiteSpace(birthDate) || !DateTime.TryParse(birthDate, out DateTime birth))
            return fallbackAge;

        DateTime current = DateTime.Today;

        if (!string.IsNullOrWhiteSpace(currentDate) && !DateTime.TryParse(currentDate, out current))
            current = DateTime.Today;

        int age = current.Year - birth.Year;

        if (birth.Date > current.Date.AddYears(-age))
            age--;

        return Mathf.Max(0, age);
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

    private NpcFailReason PickWeightedFailReason()
    {
        if (failReasonWeights == null || failReasonWeights.Length == 0)
            return PickDefaultFailReason();

        float totalWeight = 0f;

        for (int i = 0; i < failReasonWeights.Length; i++)
        {
            if (failReasonWeights[i] == null || failReasonWeights[i].reason == NpcFailReason.None)
                continue;

            totalWeight += Mathf.Max(0f, failReasonWeights[i].weight);
        }

        if (totalWeight <= 0f)
            return PickDefaultFailReason();

        float point = UnityEngine.Random.value * totalWeight;

        for (int i = 0; i < failReasonWeights.Length; i++)
        {
            WeightedFailReason weighted = failReasonWeights[i];

            if (weighted == null || weighted.reason == NpcFailReason.None)
                continue;

            point -= Mathf.Max(0f, weighted.weight);

            if (point <= 0f)
                return weighted.reason;
        }

        return PickDefaultFailReason();
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
        if (namePairs == null || namePairs.Length == 0)
            return englishSurnames;

        List<string> values = new List<string>();
        for (int i = 0; i < namePairs.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(namePairs[i]?.englishSurname))
                values.Add(namePairs[i].englishSurname);
        }

        return values.ToArray();
    }

    private string[] GetEnglishGivenNameValues()
    {
        if (namePairs == null || namePairs.Length == 0)
            return englishGivenNames;

        List<string> values = new List<string>();
        for (int i = 0; i < namePairs.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(namePairs[i]?.englishGivenNames))
                values.Add(namePairs[i].englishGivenNames);
        }

        return values.ToArray();
    }
}
