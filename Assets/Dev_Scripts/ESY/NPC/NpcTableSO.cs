using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class NpcPleaStory
{
    public string storyName;

    [TextArea(2, 5)]
    public string pleaText;

    [TextArea(2, 5)]
    public string approveNews;

    [TextArea(2, 5)]
    public string rejectNews;
}

[System.Serializable]
public class NpcPhotoSet
{
    public Sprite npcPhoto;
    public Sprite passportPhoto;
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
    public string[] names;
    public Gender[] genders = { Gender.Male, Gender.Female };
    public int minAge = 18;
    public int maxAge = 70;

    [Header("Profile")]
    public string[] jobs;
    public string[] addresses;
    public string[] familyRelationships;

    [Header("Photos")]
    [FormerlySerializedAs("photoPairs")]
    public NpcPhotoSet[] photos;

    [HideInInspector]
    [FormerlySerializedAs("portraits")]
    public Sprite[] portraits;

    [Header("Plea Stories")]
    public NpcPleaStory[] pleaStories;

    public void ResetRuntimeHistory()
    {
        recentPhotoIndexes.Clear();
    }

    public NpcCase CreateRandomCase(string currentDate, bool hasPlea, NpcFailReason failReason)
    {
        NPCData npc = ScriptableObject.CreateInstance<NPCData>();
        DocumentData passport = CreateDocument(DocumentType.Passport);
        DocumentData entryPermit = CreateDocument(DocumentType.EntryPermit);
        NpcCase npcCase = ScriptableObject.CreateInstance<NpcCase>();

        npc.npcName = Pick(names, "Unknown");
        npc.gender = Pick(genders, Gender.Male);
        npc.age = UnityEngine.Random.Range(Mathf.Min(minAge, maxAge), Mathf.Max(minAge, maxAge) + 1);
        NpcPhotoSet photoSet = PickPhotoSet();
        npc.portrait = GetNpcPhoto(photoSet);
        npc.job = Pick(jobs, "None");
        npc.address = Pick(addresses, "Unknown");
        npc.family = new[] { Pick(familyRelationships, "None") };
        npc.documentCode = CreateCode("DOC");
        npc.passportCode = CreateCode("PAS");
        npc.passport = passport;
        npc.entryPermit = entryPermit;

        CopyNpcToDocument(passport, npc, currentDate);
        CopyNpcToDocument(entryPermit, npc, currentDate);
        passport.portrait = GetPassportPhoto(photoSet);

        npcCase.npc = npc;

        ApplyFailReason(npc, currentDate, failReason);

        if (hasPlea)
            ApplyPleaStory(npcCase);

        return npcCase;
    }

    private DocumentData CreateDocument(DocumentType type)
    {
        DocumentData document = ScriptableObject.CreateInstance<DocumentData>();
        document.documentType = type;
        return document;
    }

    private void CopyNpcToDocument(DocumentData document, NPCData npc, string currentDate)
    {
        document.npcName = npc.npcName;
        document.gender = npc.gender;
        document.age = npc.age;
        document.portrait = npc.portrait;
        document.occupation = npc.job;
        document.residence = npc.address;
        document.familyRelationship = npc.family != null && npc.family.Length > 0 ? npc.family[0] : string.Empty;
        document.documentCode = npc.documentCode;
        document.passportCode = npc.passportCode;
        document.hasCriminalRecord = npc.hasCriminalRecord;
        document.criminalRecordDetails = npc.criminalRecordDetails;
        document.psychiatricHistory = npc.psychiatricHistory;
        document.passportExpiryDate = CreateExpiryDate(currentDate);
    }

    public NpcFailReason PickFailReason(NpcFailReason[] reasons)
    {
        if (reasons == null || reasons.Length == 0)
            return PickDefaultFailReason();

        return reasons[UnityEngine.Random.Range(0, reasons.Length)];
    }

    private NpcPhotoSet PickPhotoSet()
    {
        if (photos != null && photos.Length > 0)
        {
            int pairIndex = PickPhotoIndex(photos.Length);
            return pairIndex >= 0 ? photos[pairIndex] : null;
        }

        Sprite portrait = PickPortrait();
        return new NpcPhotoSet
        {
            npcPhoto = portrait,
            passportPhoto = portrait
        };
    }

    private Sprite PickPortrait()
    {
        if (portraits == null || portraits.Length == 0)
            return null;

        int index = PickPhotoIndex(portraits.Length);
        return index >= 0 ? portraits[index] : null;
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

    private void ApplyPleaStory(NpcCase npcCase)
    {
        NpcPleaStory story = PickPleaStory();

        if (story == null)
            return;

        npcCase.canPlead = true;
        npcCase.pleaChance = 1f;
        npcCase.pleaText = story.pleaText;
        npcCase.approveNews = story.approveNews;
        npcCase.rejectNews = story.rejectNews;
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
                    npc.passport.npcName = PickDifferentName(npc.npcName);
                break;

            case NpcFailReason.PassportExpired:
                if (npc.passport != null)
                    npc.passport.passportExpiryDate = CreateExpiredDate(currentDate);
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

    private NpcPleaStory PickPleaStory()
    {
        if (pleaStories == null || pleaStories.Length == 0)
            return null;

        return pleaStories[UnityEngine.Random.Range(0, pleaStories.Length)];
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

        if (photos != null && photos.Length > 0)
        {
            for (int i = 0; i < photos.Length; i++)
            {
                Sprite candidate = GetPassportPhoto(photos[i]);

                if (candidate != null && candidate != currentPortrait)
                    return candidate;
            }
        }

        if (portraits != null)
        {
            for (int i = 0; i < portraits.Length; i++)
            {
                if (portraits[i] != null && portraits[i] != currentPortrait)
                    return portraits[i];
            }
        }

        return portrait;
    }

    private Sprite PickAnyPassportPortrait()
    {
        if (photos != null && photos.Length > 0)
            return GetPassportPhoto(photos[UnityEngine.Random.Range(0, photos.Length)]);

        return PickPortrait();
    }

    private string PickDifferentName(string currentName)
    {
        if (names == null || names.Length == 0)
            return currentName + " ?";

        for (int i = 0; i < names.Length; i++)
        {
            string name = names[UnityEngine.Random.Range(0, names.Length)];

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
}
