using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private GameObject dialoguePanel; // 아까 만든 대화창 패널

    private bool isPlayerInRange = false;

    private void Start()
    {
        // 게임 시작 시 대화창은 꺼두기
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log("플레이어가 의뢰인 근처에 왔습니다. E키를 누르세요!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            Debug.Log("플레이어가 범위를 벗어났습니다.");
        }
    }

    private void Update()
    {
        // 범위 안에 있고 E키를 누르면 대화 시작
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            TriggerDialogue();
        }
    }

    private void TriggerDialogue()
    {
        if (dialoguePanel != null)
        {
            bool isActive = dialoguePanel.activeSelf;
            dialoguePanel.SetActive(!isActive); // 대화창 토글 (켜고 끄기)
            Debug.Log("대화창 상태 변경: " + !isActive);
        }
    }
}