using UnityEngine;

public enum DocumentType
{
    Passport,
    EntryPermit
}

[CreateAssetMenu(
    fileName = "NewDocumentData",
    menuName = "Refugees/Document Data"
)]
public class DocumentData : ScriptableObject
{
    [Header("기본 정보")]
    public DocumentType documentType;

    [Header("문서 정보")]
    public string documentCode;
    public string country;
    public string occupation;

    [Header("유효 기간")]
    public bool isExpired;
}