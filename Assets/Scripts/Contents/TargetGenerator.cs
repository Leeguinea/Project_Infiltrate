using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random; // UnityEngine과 System 둘다 Random 클래스가 있어서 절대 지우면 안됨.

// 구역별 NPC 그룹 구조체 (Inspector에서 구역과 부모 오브젝트를 매핑)
[System.Serializable]
public class ZoneNPCGroup
{
    public LocationZoneType zoneType;      // 구역 종류 (ZoneA, ZoneB 등) 
    public Transform zoneParent;           // 해당 구역 NPC들의 부모 Transform ([ZoneA_NPCs] 등)
    [HideInInspector]
    public List<NPCController> npcList = new List<NPCController>(); // 자동 수집될 NPC 리스트
}

// Target 생성 및 인상착의/구역 제어 시스템
public class TargetGenerator : MonoBehaviour
{
    public static TargetGenerator Instance { get; private set; }

    [Header("구역별 NPC 그룹 설정 (4개 구역 매핑)")]
    [SerializeField] private List<ZoneNPCGroup> _zoneGroups = new List<ZoneNPCGroup>();

    [Header("이번 회차 생성 데이터 (확인용)")]
    public ClueSet currentTargetClue;
    public NPCController targetNPC;
    public LocationZoneType currentTargetZone;

    private void Awake()
    {
        // 씬 내에 1개만 존재
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // 각 구역 부모 오브젝트 자식에서 NPCController 자동 수집 (총 40명 수집) // NPCController가 있는 오브젝트 ==  NPC
        foreach (var group in _zoneGroups)
        {
            if (group.zoneParent != null)
            {
                group.npcList = new List<NPCController>(group.zoneParent.GetComponentsInChildren<NPCController>());
            }
            else
            {
                Debug.LogWarning($"[TargetGenerator] {group.zoneType} 구역의 Zone Parent가 할당되지 않았습니다.");
            }
        }
    }

    private void Start()
    {
        GenerateMission();
    }

    

    public void GenerateMission()
    {
        if (_zoneGroups == null || _zoneGroups.Count == 0)
        {
            Debug.LogWarning("NPC 리스트가 비어 있습니다!");
            return;
        }

        // 1. 4개 구역 중 타겟 구역(LocationZoneType) 무작위 선정
        int randomZoneIndex = Random.Range(0, _zoneGroups.Count);
        ZoneNPCGroup selectedZoneGroup = _zoneGroups[randomZoneIndex];

        if (selectedZoneGroup.npcList == null || selectedZoneGroup.npcList.Count == 0)
        {
            Debug.LogWarning($"[TargetGenerator] {selectedZoneGroup.zoneType} 구역에 NPC가 없습니다!");
            return;
        }

        currentTargetZone = selectedZoneGroup.zoneType;

        // 2. 선정된 타겟 구역의 NPC(10명) 중 타겟 1명 무작위 지정
        int targetIndex = Random.Range(0, selectedZoneGroup.npcList.Count);
        targetNPC = selectedZoneGroup.npcList[targetIndex];


        // 3. 단서 조합 무작위 추출 (Location은 타겟 구역으로 고정)
        AppearanceType randomApp = (AppearanceType)Random.Range(0, Enum.GetValues(typeof(AppearanceType)).Length); //외형(안경+모자)
        MouthType randomMouth = (MouthType)Random.Range(0, Enum.GetValues(typeof(MouthType)).Length); // 입모양
        HabitType randomHabit = (HabitType)Random.Range(0, Enum.GetValues(typeof(HabitType)).Length); //습관


        currentTargetClue = new ClueSet(randomApp, randomMouth, randomHabit, currentTargetZone);

        // 4. 모든 구역의 모든 NPC(40명)에게 단서 데이터 적용
        foreach (var group in _zoneGroups)
        {
            foreach (var npc in group.npcList)
            {
                if (npc == targetNPC)
                {
                    // 진짜 타겟: 4개 단서 완벽 적용
                    npc.ApplyClueSet(currentTargetClue, true);
                }
                else
                {
                    // 일반 시민: 타겟 단서와 3개 요소가 완벽히 일치하지 않도록 가짜 단서 부여
                    ClueSet dummyClue = GenerateDummyClue(currentTargetClue);
                    npc.ApplyClueSet(dummyClue, false);
                }
            }
        }

        Debug.Log($"[미션 생성] Target Zone: {currentTargetZone} | Target: {targetNPC.name} | Clues: {currentTargetClue.appearance}, {currentTargetClue.mouth}, {currentTargetClue.habit}, {currentTargetClue.location}");
    }


    // 일반 시민용 가짜 단서 생성 (타겟과 3개 요소가 완벽히 일치하는 경우 방지)
    private ClueSet GenerateDummyClue(ClueSet targetClue)
    {
        AppearanceType app;
        MouthType mouth;
        HabitType habit;
        LocationZoneType loc;


        //무작위 추출 
        do
        {
            app = (AppearanceType)Random.Range(0, Enum.GetValues(typeof(AppearanceType)).Length);
            mouth = (MouthType)Random.Range(0, Enum.GetValues(typeof(MouthType)).Length);
            habit = (HabitType)Random.Range(0, Enum.GetValues(typeof(HabitType)).Length);
            loc = (LocationZoneType)Random.Range(0, Enum.GetValues(typeof(LocationZoneType)).Length);
        }
        while (app == targetClue.appearance && mouth == targetClue.mouth && habit == targetClue.habit && loc == targetClue.location); //타겟과 일치하면 다시 생성

        return new ClueSet(app, mouth, habit, loc);
    }
}