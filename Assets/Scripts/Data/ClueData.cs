using System;
using UnityEngine;

#region Enum Definitions
// 1. 외형 단서 (Color / Mesh ON-OFF)
public enum AppearanceType
{
    RedHat,         // 빨간 모자
    PinkGlasses,    // 분홍 선글라스
    BlueBag,        // 파란 가방
    YellowShirt     // 노란 셔츠
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
    NearFountain,       // 분수대 인근
    OnBench,            // 벤치 주변
    NearVendingMachine  // 자판기 근처
}
#endregion

// UI 수첩 출력용 데이터 세트
[Serializable]
public struct ClueSet
{
    public AppearanceType appearance;
    public HabitType habit;
    public LocationZoneType location;

    public ClueSet(AppearanceType appearance, HabitType habit, LocationZoneType location)
    {
        this.appearance = appearance;
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
            AppearanceType.RedHat => "Wearing: Red Hat",
            AppearanceType.PinkGlasses => "Wearing: Pink Glasses",
            AppearanceType.BlueBag => "Carrying: Blue Bag",
            AppearanceType.YellowShirt => "Wearing: Yellow Shirt",
            _ => string.Empty
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
            LocationZoneType.NearFountain => "Location: Loitering near fountain",
            LocationZoneType.OnBench => "Location: Staying near bench",
            LocationZoneType.NearVendingMachine => "Location: Seen near vending machine",
            _ => string.Empty
        };
    }
}