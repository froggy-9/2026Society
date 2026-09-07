$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent

# Exercise the production judge without starting Unity; only Unity data types are stubbed.
$sources = @'
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
namespace UnityEngine {
    public class ScriptableObject { }
    public class Sprite { }
    public class HeaderAttribute : Attribute { public HeaderAttribute(string text) { } }
    public class TooltipAttribute : Attribute { public TooltipAttribute(string text) { } }
    public class HideInInspector : Attribute { }
    public class MinAttribute : Attribute { public MinAttribute(float value) { } }
    public class InspectorNameAttribute : Attribute { public InspectorNameAttribute(string text) { } }
    public class TextAreaAttribute : Attribute {
        public TextAreaAttribute() { }
        public TextAreaAttribute(int min, int max) { }
    }
    public class CreateAssetMenuAttribute : Attribute { public string menuName; public string fileName; }
}
namespace UnityEngine.Serialization {
    public class FormerlySerializedAsAttribute : Attribute {
        public FormerlySerializedAsAttribute(string name) { }
    }
}
public class NewsSO { }
public class NpcTableSO { }
public class SpecialNpcSO { }
'@

$tableSource = Get-Content -Raw -Encoding UTF8 (Join-Path $projectRoot 'Assets/Dev_Scripts/ESY/NPC/NpcTableSO.cs')
$sources += "`n" + [regex]::Match($tableSource, 'public enum NpcFailReason\s*\{[^}]+\}').Value

$files = @(
    'Assets/Dev_Scripts/ESY/Document/DocumentData.cs',
    'Assets/Dev_Scripts/ESY/NPC/NPCData.cs',
    'Assets/Dev_Scripts/PSY/GameSO/RuleSO.cs',
    'Assets/Dev_Scripts/PSY/GameSO/DayDataSO.cs',
    'Assets/Dev_Scripts/PSY/GameManager/InspectionDecision.cs',
    'Assets/Dev_Scripts/PSY/GameManager/InspectionJudge.cs'
)
foreach ($file in $files) {
    $source = Get-Content -Raw -Encoding UTF8 (Join-Path $projectRoot $file)
    $sources += "`n" + ($source -replace '(?m)^using [^;]+;\r?\n', '')
}
Add-Type -TypeDefinition $sources

function New-Applicant {
    $npc = [NPCData]::new()
    $npc.englishSurname = 'KIM'
    $npc.englishGivenNames = 'MIN'
    $npc.nationality = 'Allowed'
    $npc.job = 'Doctor'
    $npc.age = 30
    $npc.dateOfBirth = '2020-06-17'
    $npc.documentCode = 'DOC-1'
    $npc.passportCode = 'PAS-1'
    $npc.portrait = [UnityEngine.Sprite]::new()
    foreach ($field in @('passport', 'entryPermit')) {
        $doc = [DocumentData]::new()
        foreach ($property in @('englishSurname', 'englishGivenNames', 'nationality', 'age', 'dateOfBirth', 'documentCode', 'passportCode', 'portrait')) {
            $doc.$property = $npc.$property
        }
        $doc.occupation = $npc.job
        $doc.passportExpiryDate = '2050-06-19'
        $npc.$field = $doc
    }
    return $npc
}

$rules = @()
for ($day = 1; $day -le 3; $day++) {
    $asset = Get-Content -Raw -Encoding UTF8 (Join-Path $projectRoot "Assets/Dev_Datas/Day/Day$day Data.asset")
    $hex = [regex]::Match($asset, '(?m)^  rejectReasons: ([0-9a-f]+)').Groups[1].Value
    if (!$hex -or $hex.Length % 8 -ne 0) { throw "Invalid inspection items for day $day" }
    $checks = @()
    for ($offset = 0; $offset -lt $hex.Length; $offset += 8) {
        $bytes = [Convert]::FromHexString($hex.Substring($offset, 8))
        $value = [BitConverter]::ToInt32($bytes, 0)
        if (![Enum]::IsDefined([NpcFailReason], $value)) { throw "Removed item $value remains in day $day" }
        $checks += [NpcFailReason]$value
    }
    $rule = [RuleSO]::new()
    $dayData = [DayDataSO]::new()
    $dayData.rejectReasons = $checks
    $rule.checkTypes = $dayData.GetInspectionChecks()
    $rule.bannedNationalities = @('Banned')
    $rule.inspectedOccupations = @('Doctor', 'Pharmacist', 'Distributor')
    $rules += $rule
}

$script:assertions = 0
function Assert-Days($name, $npc, [bool[]]$expected) {
    for ($day = 0; $day -lt 3; $day++) {
        $result = [InspectionJudge]::Evaluate($npc, [RuleSO[]]@($rules[$day]), '2050-06-19')
        if ($result.shouldApprove -ne $expected[$day]) { throw "$name failed on day $($day + 1)" }
        $script:assertions++
    }
}

Assert-Days 'Valid, expires today, no medical history' (New-Applicant) @($true, $true, $true)
$npc = New-Applicant; $npc.passport = $null
Assert-Days 'Missing passport' $npc @($false, $false, $false)
$npc = New-Applicant; $npc.entryPermit = $null
Assert-Days 'Missing entry form' $npc @($false, $false, $false)
$npc = New-Applicant; $npc.passport.portrait = [UnityEngine.Sprite]::new()
Assert-Days 'Portrait mismatch' $npc @($false, $false, $false)
$npc = New-Applicant; $npc.entryPermit.nationality = 'Other'
Assert-Days 'Country mismatch' $npc @($false, $false, $false)
$npc = New-Applicant; $npc.passport.passportExpiryDate = '2050-06-18'
Assert-Days 'Expired passport' $npc @($false, $false, $false)
$npc = New-Applicant; $npc.nationality = 'Banned'; $npc.passport.nationality = 'Banned'; $npc.entryPermit.nationality = 'Banned'
Assert-Days 'Banned country' $npc @($true, $false, $false)
$npc = New-Applicant; $npc.entryPermit.occupation = 'Farmer'
Assert-Days 'Regulated occupation mismatch' $npc @($true, $false, $false)
$npc = New-Applicant; $npc.job = 'Farmer'; $npc.passport.occupation = 'Farmer'; $npc.entryPermit.occupation = 'Pilot'
Assert-Days 'Unregulated occupation mismatch' $npc @($true, $true, $true)
$npc = New-Applicant; $npc.entryPermit.englishSurname = 'LEE'
Assert-Days 'Name mismatch' $npc @($true, $true, $false)
$npc = New-Applicant; $npc.entryPermit.age = 31
Assert-Days 'Age mismatch' $npc @($true, $true, $false)
$npc = New-Applicant; $npc.entryPermit.documentCode = 'FORGED'
Assert-Days 'Document code mismatch' $npc @($true, $true, $false)
$npc = New-Applicant; $npc.entryPermit.passportCode = 'FORGED'
Assert-Days 'Passport code mismatch' $npc @($true, $true, $false)
$npc = New-Applicant; $npc.entryPermit.hasCriminalRecord = $true
Assert-Days 'Criminal record' $npc @($true, $true, $false)
$npc = New-Applicant; $npc.passport = $null; $npc.entryPermit = $null; $npc.useManualDecision = $true
Assert-Days 'Special NPC manual decision preserved' $npc @($true, $true, $true)
$selection = [DayDataSO]::new()
$selection.rejectReasons = @([NpcFailReason]::NameMismatch)
$rules[0].checkTypes = $selection.GetInspectionChecks()
$npc = New-Applicant
$npc.entryPermit.englishSurname = 'LEE'
if ([InspectionJudge]::Evaluate($npc, [RuleSO[]]@($rules[0]), '2050-06-19').shouldApprove) {
    throw 'Selecting Name Mismatch did not enable the check'
}
$selection.rejectReasons = @()
$rules[0].checkTypes = $selection.GetInspectionChecks()
if (![InspectionJudge]::Evaluate($npc, [RuleSO[]]@($rules[0]), '2050-06-19').shouldApprove) {
    throw 'Clearing inspection items did not disable the check'
}
$script:assertions += 2
Write-Output "PASS: $script:assertions decisions including DaySO selection changes"
