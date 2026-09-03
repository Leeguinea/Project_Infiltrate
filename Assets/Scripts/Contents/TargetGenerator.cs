using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random; // UnityEngine과 System 둘다 Random 클래스가 있어서 절대 지우면 안됨.

// target 생성
// 인상착의를 랜덤으로 바꾸는 기능
public class TargetGenerator : MonoBehaviour
{
    [Header("광장 NPC 리스트")]
    public List<NPCController> npcList = new List<NPCController>();

    [Header("이번 회차 생성 데이터 (확인용)")]
    public ClueSet currentTargetClue;
    public NPCController targetNPC;


    [SerializeField] private Transform npcGroupParent; // [NPC_Group] 부모 오브젝트 연결

    private void Start()
    {
        GenerateMission();
    }

    private void Awake()
    {
        if (npcGroupParent != null)
        {
            // 부모 밑에 있는 모든 NPCController를 자식에서 자동으로 가져옴
            npcList = new List<NPCController>(npcGroupParent.GetComponentsInChildren<NPCController>());
        }
    }


    public void GenerateMission()
    {
        if (npcList == null || npcList.Count == 0)
        {
            Debug.LogWarning("NPC 리스트가 비어 있습니다!");
            return;
        }

        // 1. 15명 중 타겟 1명 무작위 지정
        int targetIndex = Random.Range(0, npcList.Count);
        targetNPC = npcList[targetIndex];

        // 2. 단서 3개 조합 무작위 추출
        AppearanceType randomApp = (AppearanceType)Random.Range(0, Enum.GetValues(typeof(AppearanceType)).Length);
        HabitType randomHabit = (HabitType)Random.Range(0, Enum.GetValues(typeof(HabitType)).Length);
        LocationZoneType randomLoc = (LocationZoneType)Random.Range(0, Enum.GetValues(typeof(LocationZoneType)).Length);

        currentTargetClue = new ClueSet(randomApp, randomHabit, randomLoc);

        // 3. 전체 NPC에게 단서 데이터 적용
        for (int i = 0; i < npcList.Count; i++)
        {
            if (npcList[i] == targetNPC)
            {
                // 진짜 타겟: 3개 단서 완벽 적용
                npcList[i].ApplyClueSet(currentTargetClue, true);
            }
            else
            {
                // 일반 시민: 단서 3개가 완벽히 겹치지 않는 가짜 조합 부여
                ClueSet dummyClue = GenerateDummyClue(currentTargetClue);
                npcList[i].ApplyClueSet(dummyClue, false);
            }
        }

        Debug.Log($"[미션 생성] Target: {targetNPC.name} | Clues: {currentTargetClue.appearance}, {currentTargetClue.habit}, {currentTargetClue.location}");
    }

    // 일반 시민용 가짜 단서 생성 (타겟과 3개 모두 일치하는 경우 방지)
    private ClueSet GenerateDummyClue(ClueSet targetClue)
    {
        AppearanceType app;
        HabitType habit;
        LocationZoneType loc;

        do
        {
            app = (AppearanceType)Random.Range(0, Enum.GetValues(typeof(AppearanceType)).Length);
            habit = (HabitType)Random.Range(0, Enum.GetValues(typeof(HabitType)).Length);
            loc = (LocationZoneType)Random.Range(0, Enum.GetValues(typeof(LocationZoneType)).Length);
        }
        while (app == targetClue.appearance && habit == targetClue.habit && loc == targetClue.location);

        return new ClueSet(app, habit, loc);
    }
}