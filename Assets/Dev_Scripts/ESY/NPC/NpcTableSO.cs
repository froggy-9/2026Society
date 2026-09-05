using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class NpcPhotoSet
{
    [Tooltip("화면에 서 있는 NPC 사진입니다.")]
    public Sprite npcPhoto;

    [Tooltip("여권에 들어갈 사진입니다. 정상 NPC라면 npcPhoto와 같은 이미지를 넣으세요.")]
    public Sprite passportPhoto;
}

[System.Serializable]
public class NpcNameSet
{
    [Tooltip("여권에 표시할 한글 성명입니다. 예: 김민지")]
    public string koreanName;

    [Tooltip("여권에 표시할 영문 성입니다. 예: KIM")]
    public string englishSurname;

    [Tooltip("여권에 표시할 영문 이름입니다. 예: MINJI")]
    public string englishGivenNames;
}

public enum NpcFailReason
{
    None,
    MissingPassport,
    MissingEntryPermit,
    PortraitMismatch,
    NameMismatch,
    PassportExpired,
    PassportCodeMismatch,
    DocumentCodeMismatch
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
    [Tooltip("한 칸에 한글 성명, 영문 성, 영문 이름을 함께 넣습니다.")]
    public NpcNameSet[] namePairs;

    [HideInInspector]
    [FormerlySerializedAs("names")]
    public string[] koreanNames;

    [HideInInspector]
    public string[] englishSurnames;

    [HideInInspector]
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

    [Header("Passport Values")]
    [Tooltip("여권번호 목록입니다.")]
    public string[] passportNumbers;

    [Tooltip("여권 발급일 목록입니다. 형식은 yyyy-MM-dd를 권장합니다.")]
    public string[] passportIssueDates;

    [Tooltip("정상 여권 만료일 목록입니다. 현재 날짜보다 뒤의 날짜를 넣습니다.")]
    public string[] passportExpiryDates;

    [Tooltip("만료 여권 오류를 만들 때 사용할 만료일 목록입니다. 현재 날짜보다 앞의 날짜를 넣습니다.")]
    public string[] expiredPassportExpiryDates;

    [Tooltip("여권 발급기관 목록입니다.")]
    public string[] issuingAuthorities;

    [Header("Photo Pairs")]
    [Tooltip("사진 한 칸 안에 NPC 사진과 여권 사진을 함께 넣습니다.")]
    [FormerlySerializedAs("photos")]
    public NpcPhotoSet[] photoPairs;

    public void ResetRuntimeHistory()
    {
        recentPhotoIndexes.Clear();
    }

    public NPCData CreateRandomNpc(string currentDate, NpcFailReason failReason)
    {
        NPCData npc = new NPCData();
        DocumentData passport = CreateDocument(DocumentType.Passport);
        DocumentData entryPermit = CreateDocument(DocumentType.EntryPermit);

        NpcNameSet nameSet = PickNameSet();
        npc.koreanName = GetKoreanName(nameSet);
        npc.englishSurname = GetEnglishSurname(nameSet);
        npc.englishGivenNames = GetEnglishGivenNames(nameSet);
        npc.gender = Pick(genders, Gender.Male);
        npc.age = PickAge();
        npc.nationality = Pick(nationalities, "Stateless");
        npc.dateOfBirth = Pick(birthDates, CreateBirthDate(npc.age, currentDate));
        npc.age = GetAgeFromBirthDate(npc.dateOfBirth, currentDate, npc.age);
        NpcPhotoSet photoSet = PickPhotoSet();
        npc.portrait = GetNpcPhoto(photoSet);
        npc.job = Pick(jobs, "None");
        npc.address = Pick(addresses, "Unknown");
        npc.family = new[] { Pick(familyRelationships, "None") };
        npc.documentCode = CreateCode("DOC");
        npc.passportCode = Pick(passportNumbers, CreateCode("PAS"));
        npc.passport = passport;
        npc.entryPermit = entryPermit;

        CopyNpcToDocument(passport, npc, currentDate);
        CopyNpcToDocument(entryPermit, npc, currentDate);
        passport.portrait = GetPassportPhoto(photoSet);

        ApplyFailReason(npc, currentDate, failReason);

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
        document.koreanName = npc.koreanName;
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
        document.issueDate = Pick(passportIssueDates, CreateIssueDate(currentDate));
        document.issuingAuthority = Pick(issuingAuthorities, "Border Immigration Office");
        document.passportExpiryDate = Pick(passportExpiryDates, CreateExpiryDate(currentDate));
    }

    public NpcFailReason PickFailReason(NpcFailReason[] reasons)
    {
        if (reasons == null || reasons.Length == 0)
            return PickDefaultFailReason();

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

    private void ApplyFailReason(NPCData npc, string currentDate, NpcFailReason failReason)
    {
        if (npc == null || failReason == NpcFailReason.None)
            return;

        switch (failReason)
        {
            case NpcFailReason.MissingPassport:
                npc.passport = null;
                break;

            case NpcFailReason.MissingEntryPermit:
                npc.entryPermit = null;
                break;

            case NpcFailReason.PortraitMismatch:
                if (npc.passport != null)
                    npc.passport.portrait = PickDifferentPortrait(npc.portrait);
                break;

            case NpcFailReason.NameMismatch:
                if (npc.passport != null)
                {
                    npc.passport.koreanName = PickDifferentName(npc.koreanName);
                    npc.passport.englishSurname = PickDifferentEnglishSurname(npc.englishSurname);
                    npc.passport.englishGivenNames = PickDifferentEnglishGivenNames(npc.englishGivenNames);
                }
                break;

            case NpcFailReason.PassportExpired:
                if (npc.passport != null)
                    npc.passport.passportExpiryDate = Pick(expiredPassportExpiryDates, CreateExpiredDate(currentDate));
                break;

            case NpcFailReason.PassportCodeMismatch:
                if (npc.passport != null)
                    npc.passport.passportCode = CreateCode("PAS");
                break;

            case NpcFailReason.DocumentCodeMismatch:
                if (npc.entryPermit != null)
                    npc.entryPermit.documentCode = CreateCode("DOC");
                break;
        }
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

    private string PickDifferentName(string currentName)
    {
        string[] values = GetKoreanNameValues();
        return PickDifferentName(currentName, values);
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
            NpcFailReason.MissingEntryPermit,
            NpcFailReason.PortraitMismatch,
            NpcFailReason.NameMismatch,
            NpcFailReason.PassportExpired,
            NpcFailReason.PassportCodeMismatch,
            NpcFailReason.DocumentCodeMismatch
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

    private NpcNameSet PickNameSet()
    {
        if (namePairs != null && namePairs.Length > 0)
            return namePairs[UnityEngine.Random.Range(0, namePairs.Length)];

        return new NpcNameSet
        {
            koreanName = Pick(koreanNames, "이름없음"),
            englishSurname = Pick(englishSurnames, "UNKNOWN"),
            englishGivenNames = Pick(englishGivenNames, "UNKNOWN")
        };
    }

    private static string GetKoreanName(NpcNameSet nameSet)
    {
        return nameSet != null && !string.IsNullOrWhiteSpace(nameSet.koreanName)
            ? nameSet.koreanName
            : "이름없음";
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

    private string[] GetKoreanNameValues()
    {
        if (namePairs == null || namePairs.Length == 0)
            return koreanNames;

        List<string> values = new List<string>();
        for (int i = 0; i < namePairs.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(namePairs[i]?.koreanName))
                values.Add(namePairs[i].koreanName);
        }

        return values.ToArray();
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
