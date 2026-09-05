using UnityEngine;
using UnityEngine.Serialization;

public enum RefugeesEndingType
{
    Preservation,
    FollowUpCare,
    Closure
}

[System.Serializable]
public class PerformanceGrade
{
    [Tooltip("인스펙터 구분용 이름입니다.")]
    public string label = "우수";

    [Tooltip("이 등급에 필요한 최소 정확도입니다. 0.85는 85%입니다.")]
    [Range(0f, 1f)]
    public float minimumAccuracy = 0.85f;

    [Tooltip("기본 성과금에서 몇 %를 지급할지입니다. 0.7은 70%입니다.")]
    [Range(0f, 2f)]
    public float payRate = 0.7f;

    [Tooltip("결과 화면에 표시할 기관 평가 문구입니다.")]
    [TextArea(2, 4)]
    public string comment;
}

[System.Serializable]
public class EndingNewsContent
{
    [Tooltip("이 뉴스가 어느 엔딩에서 출력될지입니다.")]
    public RefugeesEndingType endingType;

    [Tooltip("엔딩 뉴스 상단 작은 텍스트입니다. 예: 최종 보도 · 존손")]
    public string metaText;

    [Tooltip("엔딩 뉴스 헤드라인입니다.")]
    public string headline;

    [Tooltip("엔딩 뉴스 본문입니다.")]
    [TextArea(5, 12)]
    public string body;

    [Tooltip("엔딩 뉴스에 표시할 이미지 목록입니다.")]
    public Sprite[] images;
}

[CreateAssetMenu(
    fileName = "NewEvaluationConfig",
    menuName = "Refugees/Evaluation Config"
)]
public class EvaluationConfigSO : ScriptableObject
{
    [Header("Daily Bonus")]
    [Tooltip("정확도 100% 기준 기본 성과금입니다.")]
    public int baseBonus = 1000;

    [Tooltip("정확도에 따른 평가 등급입니다. 높은 정확도 순서로 넣는 것을 권장합니다.")]
    public PerformanceGrade[] grades =
    {
        new PerformanceGrade { label = "탁월", minimumAccuracy = 0.95f, payRate = 1f },
        new PerformanceGrade { label = "우수", minimumAccuracy = 0.85f, payRate = 0.7f },
        new PerformanceGrade { label = "보통", minimumAccuracy = 0.7f, payRate = 0.45f },
        new PerformanceGrade { label = "미흡", minimumAccuracy = 0.5f, payRate = 0.2f },
        new PerformanceGrade { label = "부진", minimumAccuracy = 0f, payRate = 0f }
    };

    [Header("Ending")]
    [FormerlySerializedAs("happyEndingMinimumAccuracy")]
    [Tooltip("존손 엔딩에 필요한 최종 최소 정확도입니다. 0.8은 80%입니다.")]
    [Range(0f, 1f)]
    public float preservationEndingMinimumAccuracy = 0.8f;

    [Tooltip("정확도 기준 미달이고 잘못 수용/잘못 거절 수가 같을 때 사용할 엔딩입니다.")]
    public RefugeesEndingType tieEndingType = RefugeesEndingType.Closure;

    [Tooltip("엔딩별 최종 뉴스 내용입니다. 씬 이동 없이 현재 씬 최상단 UI에 출력됩니다.")]
    public EndingNewsContent[] endingNews =
    {
        new EndingNewsContent
        {
            endingType = RefugeesEndingType.Preservation,
            metaText = "최종 보도 · 존손",
            headline = "국경 심사 체계 존손 결정",
            body = "국경관리국은 난민 심사 절차가 비교적 안정적으로 유지되었다고 발표했다."
        },
        new EndingNewsContent
        {
            endingType = RefugeesEndingType.FollowUpCare,
            metaText = "최종 보도 · 사후관리",
            headline = "난민 수용 사후관리 체계 가동",
            body = "잘못 수용된 사례가 누적되며 정부는 수용 시설 점검과 사후관리 체계를 강화하기로 했다."
        },
        new EndingNewsContent
        {
            endingType = RefugeesEndingType.Closure,
            metaText = "최종 보도 · 폐쇄조치",
            headline = "국경 관리소 폐쇄조치 발표",
            body = "입국 가능한 난민들이 반복적으로 거절되며 현장의 반발이 커졌고, 정부는 관리소 폐쇄조치를 발표했다."
        }
    };

    public PerformanceGrade GetGrade(float accuracy)
    {
        if (grades == null || grades.Length == 0)
            return new PerformanceGrade { label = "보통", minimumAccuracy = 0f, payRate = 0f };

        PerformanceGrade best = grades[grades.Length - 1];

        for (int i = 0; i < grades.Length; i++)
        {
            PerformanceGrade grade = grades[i];

            if (grade == null)
                continue;

            if (accuracy >= grade.minimumAccuracy)
                return grade;
        }

        return best;
    }

    public EndingNewsContent GetEndingNewsContent(RefugeesEndingType endingType)
    {
        if (endingNews != null)
        {
            for (int i = 0; i < endingNews.Length; i++)
            {
                EndingNewsContent content = endingNews[i];

                if (content != null && content.endingType == endingType)
                    return content;
            }
        }

        return null;
    }
}
