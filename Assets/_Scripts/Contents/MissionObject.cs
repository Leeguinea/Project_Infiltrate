using UnityEngine;

public class MissionObject : Interactable
{
    [Header("미션 고유 설정")]
    public string missionName = "서류 탈취";

    public override void OnInteractComplete()
    {
        base.OnInteractComplete();

        Debug.Log($"[미션 성공] {missionName} 완료! GameManager에 알립니다.");

        //// TODO: 나중에 미션 카운트를 올릴 때 여기에
        ///GameManager.Instance.AddMissionCount() 같은 코드
        ///

        gameObject.SetActive(false);
    }
}
