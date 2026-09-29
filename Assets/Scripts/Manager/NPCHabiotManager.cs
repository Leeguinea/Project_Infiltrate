using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; 

public class NPCHabitManager : MonoBehaviour
{
    public HabitType assignedHabit;

    [Header("애니메이션 클립 이름들")]
    public List<string> danceAnimations = new List<string> { "Dance_01", "Dance_02", "Dance_03", "Dance_04", "Dance_05" };
    public string exerciseAnimation = "Exercise_Stretching";
    public string selfCheckAnimation = "SelfCheck";

    [Header("시간 및 주기 설정 (인스펙터에서 조절 가능)")]
    [SerializeField] private float habitDuration = 8f;        // 행동을 지속하는 시간 (초)
    [SerializeField] private float minCooldown = 15f;         // 다음 행동까지 최소 대기 시간 (초)
    [SerializeField] private float maxCooldown = 30f;         // 다음 행동까지 최대 대기 시간 (초)

    private Animator _animator;
    private NavMeshAgent _agent; // 이동을 멈추기 위한 에이전트
    private bool _isActionRunning = false;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>(); // 컴포넌트 가져오기
    }

    private void Start()
    {
        if (assignedHabit == HabitType.None)
        {
            AssignRandomHabit();
        }
    }

    public void AssignRandomHabit()
    {
        HabitType[] validHabits = { HabitType.Dance, HabitType.Exercise,HabitType.SelfCheck };
        assignedHabit = validHabits[UnityEngine.Random.Range(0, validHabits.Length)];
    }

    private void Update()
    {
        // 이미 행동 중이 아니고, 습관이 지정되어 있다면 루프 시작
        if (!_isActionRunning && assignedHabit != HabitType.None)
        {
            StartCoroutine(PlayHabitRoutine());
        }
    }

    private IEnumerator PlayHabitRoutine()
    {
        _isActionRunning = true;

        // 게임 시작하자마자 혹은 직전 행동이 끝나고 바로 실행되지 않도록 '쿨타임(대기 시간)'
        float waitTime = UnityEngine.Random.Range(minCooldown, maxCooldown);
        yield return new WaitForSeconds(waitTime);

        // 애니메이션이 시작되는 순간 NPC의 이동(NavMeshAgent)을 완전히 정지
        if (_agent != null && _agent.enabled)
        {
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero; // 밀려 나가는 관성 방지
        }

        // 할당된 습관 타입에 따라 애니메이션 실행
        switch (assignedHabit)
        {
            case HabitType.Dance:
                string chosenDance = danceAnimations[UnityEngine.Random.Range(0, danceAnimations.Count)];
                if (_animator != null) _animator.SetTrigger(chosenDance);
                break;

            case HabitType.Exercise:
                if (_animator != null) _animator.SetTrigger(exerciseAnimation);
                break;

            case HabitType.SelfCheck:
                if (_animator != null) _animator.SetTrigger(selfCheckAnimation);
                break;
        }

        // 설정한 지속 시간(habitDuration) 동안 그 자리에 멈춰서 행동을 수행
        yield return new WaitForSeconds(habitDuration);

        // 행동이 끝나면 다시 이동(NavMeshAgent)을 풀어주어 원래 하던 길을 가거나 배회
        if (_agent != null && _agent.enabled)
        {
            _agent.isStopped = false;
        }

        _isActionRunning = false;
    }
}