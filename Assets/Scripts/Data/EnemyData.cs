using UnityEngine;


[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemy/Enemy Data")]
public class EnemyData : ScriptableObject 
{
    [Header("이동 및 밸런스 데이터")]
    public float speed = 4.0f;

    [Header("시야 감지 데이터")]
    public float viewDistance = 5.0f;
    public float viewAngle = 90.0f;
}