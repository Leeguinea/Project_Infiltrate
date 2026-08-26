using UnityEngine;

public class EnemyStateManager : MonoBehaviour
{
    public enum EnemyState { Patrol, Chase, Doubt, Surprise, Wait }
    public EnemyState CurrentState { get; private set; } = EnemyState.Patrol;

    public void ChangeState(EnemyState newState)
    {
        if(CurrentState == newState) return;

        CurrentState = newState;
        Debug.Log($"상태 변경: {newState}");
    }
}
