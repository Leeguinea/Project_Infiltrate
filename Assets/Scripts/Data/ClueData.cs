using System;
using UnityEngine;

#region Enum Definitions
// 1. 외형 단서
public enum AppearanceType
{
    None = 0,

    // 모자 그룹 (1 ~ 5)
    Hat1 = 1,
    Hat2 = 2,
    Hat3 = 3,
    Hat4 = 4,
    Hat5 = 5,

    // 안경 그룹 (6 ~ 10)
    Glasses1 = 6,
    Glasses2 = 7,
    Glasses3 = 8,
    Glasses4 = 9,
    Glasses5 = 10
}

// 입모양 단서 (1~10)
public enum MouthType
{
    Mouth01 = 1,
    Mouth02 = 2,
    Mouth03 = 3,
    Mouth04 = 4,
    Mouth05 = 5,
    Mouth06 = 6,
    Mouth07 = 7,
    Mouth08 = 8,
    Mouth09 = 9,
    Mouth10 = 10
}

// 2. 습관/행동 단서 (Animation Trigger)
public enum HabitType
{
    SneezeEvery10s,     // 주기적으로 재채기함
    CheckPhone,         // 스마트폰 확인함
    Dance       // 춤
}

// 3. 선호 장소 단서 (Zone Tag/Collider)
public enum LocationZoneType
{
    // TODO: 필요시 CommercialDistrict, ParkArea 등 실제 이름으로 변경
    ZoneA, // 광장 구역
    ZoneB, // 공원 구역
    ZoneC, // 주택가 구역
    ZoneD  // 상가 구역
}
#endregion

// UI 수첩 출력용 데이터 세트
[Serializable]
public struct ClueSet
{
    public AppearanceType appearance;
    public MouthType mouth;         
    public HabitType habit;
    public LocationZoneType location;

    public ClueSet(AppearanceType appearance, MouthType mouth, HabitType habit, LocationZoneType location)
    {
        this.appearance = appearance;
        this.mouth = mouth;
        this.habit = habit;
        this.location = location;
    }
}

// 수첩 UI 출력을 위한 단서 텍스트 변환 도우미 클래스 (영어 출력)
public static class ClueTextUtility
{
    public static string GetAppearanceText(AppearanceType type)
    {
        return type switch
        {
            AppearanceType.Hat1 => "Wearing: Cap 01",
            AppearanceType.Hat2 => "Wearing: Cap 02",
            AppearanceType.Hat3 => "Wearing: Cap 03",
            AppearanceType.Hat4 => "Wearing: Cap 04",
            AppearanceType.Hat5 => "Wearing: Cap 05",
            AppearanceType.Glasses1 => "Wearing: Glasses 01",
            AppearanceType.Glasses2 => "Wearing: Glasses 02",
            AppearanceType.Glasses3 => "Wearing: Glasses 03",
            AppearanceType.Glasses4 => "Wearing: Glasses 04",
            AppearanceType.Glasses5 => "Wearing: Glasses 05",
            _ => "Wearing: Nothing special"
        };
    }

    public static string GetMouthText(MouthType type)
    {
        return type switch
        {
            MouthType.Mouth01 => "Mouth: Expression 01",
            MouthType.Mouth02 => "Mouth: Expression 02",
            MouthType.Mouth03 => "Mouth: Expression 03",
            MouthType.Mouth04 => "Mouth: Expression 04",
            MouthType.Mouth05 => "Mouth: Expression 05",
            MouthType.Mouth06 => "Mouth: Expression 06",
            MouthType.Mouth07 => "Mouth: Expression 07",
            MouthType.Mouth08 => "Mouth: Expression 08",
            MouthType.Mouth09 => "Mouth: Expression 09",
            MouthType.Mouth10 => "Mouth: Expression 10",
            _ => "Mouth: None"
        };
    }

    public static string GetHabitText(HabitType type)
    {
        return type switch
        {
            HabitType.SneezeEvery10s => "Habit: Sneezes periodically",
            HabitType.CheckPhone => "Habit: Constantly checks phone",
            HabitType.Dance => "Habit: Dancing",
            _ => string.Empty
        };
    }

    public static string GetLocationText(LocationZoneType type)
    {
        return type switch
        {
            LocationZoneType.ZoneA => "Location: Last seen in Zone A",
            LocationZoneType.ZoneB => "Location: Last seen in Zone B",
            LocationZoneType.ZoneC => "Location: Last seen in Zone C",
            LocationZoneType.ZoneD => "Location: Last seen in Zone D",
            _ => string.Empty
        };
    }
}