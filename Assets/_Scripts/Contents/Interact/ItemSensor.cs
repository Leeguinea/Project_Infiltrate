using UnityEngine;

//아이템 센서 전용 
public class ItemSensor : MonoBehaviour
{
    private PlayerEquip _playerEquip;

    void Awake()
    {
        // 부모(Player) 오브젝트에 있는 PlayerEquip을 미리 찾아둠
        _playerEquip = GetComponentInParent<PlayerEquip>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        //아이템오브젝트에서 추출
        ItemObject item = other.GetComponent<ItemObject>();
        if(item != null)
        {
            if(_playerEquip != null)
            {
                _playerEquip.SetInteractableItem(item);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ItemObject item = other.GetComponent<ItemObject>();
        if (item != null)
        {
            if (_playerEquip != null)
            {
                _playerEquip.ClearInteractableItem(item);
            }
        }

    }
}
