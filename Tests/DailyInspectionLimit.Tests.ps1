$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent

# Run the real game state controller with deterministic manager and Unity stubs.
$source = @'
using UnityEngine;
namespace UnityEngine {
    public class MonoBehaviour {
        public object gameObject;
        protected static void Destroy(object value) { }
        protected static T FindFirstObjectByType<T>() where T : class { return null; }
    }
    public class HeaderAttribute : System.Attribute { public HeaderAttribute(string text) { } }
    public class SerializeField : System.Attribute { }
    public static class Mathf { public static int Max(int a, int b) { return System.Math.Max(a, b); } }
    public static class Time { public static float deltaTime; public static float timeScale = 1; }
}
public class NPCData { }
public class RuleSO { }
public class NewsSO { }
public class DayDataSO { public int maxInspectionCount; }
public static class PleaResultLog { public static void Clear() { } }
public class DayManager {
    public DayDataSO CurrentDayData = new DayDataSO();
    public NewsSO CurrentNews;
    public System.Collections.Generic.List<RuleSO> CurrentRules;
    public RuleSO TodayRule;
    public string CurrentRuleDescription;
    public int Quota = 8;
    public float DayTime = 120;
    public bool LoadDay(int day) {
        CurrentDayData.maxInspectionCount = day == 1 ? 3 : 2;
        return true;
    }
}
public class EvaluationManager {
    public int Records, Settlements, SettledCount;
    public bool Bankrupt;
    public void ResetGame() { ResetDay(); }
    public void ResetDay() { Records = 0; Settlements = 0; SettledCount = 0; }
    public void SubmitJudgement(bool approved, bool expected, NPCData npc, string reason) { Records++; }
    public void CalculateResult(int count, int quota) {
        Settlements++;
        SettledCount = count;
        if (Records != count) throw new System.Exception("Result missed the final judgement");
    }
    public bool IsGameOver() { return Bankrupt; }
}
'@
foreach ($file in @('Assets/Dev_Scripts/PSY/TitleScene/GameState.cs', 'Assets/Dev_Scripts/PSY/GameManager/RefugeesGameManager.cs')) {
    $text = Get-Content -Raw -Encoding UTF8 (Join-Path $projectRoot $file)
    $source += "`n" + ($text -replace '(?m)^using [^;]+;\r?\n', '')
}
Add-Type -TypeDefinition $source
$flags = [Reflection.BindingFlags]'Instance,NonPublic'
$manager = [RefugeesGameManager]::new()
$days = [DayManager]::new()
$evaluation = [EvaluationManager]::new()
$manager.GetType().GetField('dayManager', $flags).SetValue($manager, $days)
$manager.GetType().GetField('evaluationManager', $flags).SetValue($manager, $evaluation)
$script:checks = 0
function Assert($condition, $message) {
    if (!$condition) { throw $message }
    $script:checks++
}
function Begin-Inspection {
    $manager.BeginDayNews()
    if ($manager.CurrentState -eq [GameState]::StartMap) { $manager.BeginDayNews() }
    $manager.StartInspection()
}
$manager.GameStart()
Begin-Inspection
$manager.SubmitJudgement($true, $true)
$manager.SubmitJudgement($false, $false)
Assert ($manager.InspectedNpcCount -eq 2) 'Approvals and denials must both count'
Assert ($manager.CurrentState -eq [GameState]::Inspection) 'Day ended before the limit'
$manager.SubmitJudgement($false, $true)
Assert ($manager.CurrentState -eq [GameState]::Result) 'The limit must show the result'
Assert ($manager.RemainingTime -eq 120) 'Early completion should not require timeout'
Assert ($evaluation.SettledCount -eq 3) 'The last NPC was excluded from results'
Assert (!$manager.CanSpawnNpc()) 'Spawning remained enabled after the limit'
$manager.SubmitJudgement($true, $true)
$manager.EndDay()
Assert ($manager.InspectedNpcCount -eq 3 -and $evaluation.Settlements -eq 1) 'Repeated submissions changed the settled result'
$manager.NextDay()
Assert ($manager.CurrentDay -eq 2 -and $manager.InspectedNpcCount -eq 0) 'Daily count did not reset'
Assert ($evaluation.Records -eq 0 -and $manager.MaxInspectionCount -eq 2) 'New day retained old records or limit'
Begin-Inspection
$manager.SubmitJudgement($true, $true)
[UnityEngine.Time]::deltaTime = 121
$manager.GetType().GetMethod('Update', $flags).Invoke($manager, @()) | Out-Null
Assert ($manager.CurrentState -eq [GameState]::Result -and $evaluation.SettledCount -eq 1) 'Timeout did not settle a partial day'
$evaluation.Bankrupt = $true
$manager.NextDay()
Assert ($manager.CurrentState -eq [GameState]::GameOver) 'Bankruptcy should follow the daily result'
Write-Output "PASS: $script:checks daily limit and transition checks"
