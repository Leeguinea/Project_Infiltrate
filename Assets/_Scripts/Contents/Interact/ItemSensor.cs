using UnityEngine;
using System.Collections.Generic;
//아이템 센서 전용 
public class ItemSensor : MonoBehaviour
{
    private PlayerEquip _playerEquip;
    private List<ItemObject> _nearItems = new List<ItemObject>();

    void Awake()
    {
        // 부모(Player) 오브젝트에 있는 PlayerEquip을 미리 찾아둠
        _playerEquip = GetComponentInParent<PlayerEquip>();
    }
    
    //들어올때
    private void OnTriggerEnter(Collider other)
    {
        //아이템오브젝트에서 추출
        ItemObject item = other.GetComponent<ItemObject>();
        if(item != null)
        {
            if(!_nearItems.Contains(item))
            {
                _nearItems.Add(item);
                if (_playerEquip != null)
                {
                    //데이터전달
                    _playerEquip.SetInteractableItem(_nearItems);
                }
            }
        }
    }

    //나갈때
    private void OnTriggerExit(Collider other)
    {
        ItemObject item = other.GetComponent<ItemObject>();
        if (item != null)
        {
            //리스트에 있을 때 제거
            if (_nearItems.Contains(item))
            {
                _nearItems.Remove(item);
            }

            //목록이 바뀌면 PlayerEquip(SetInteractableItem) 업데이트
            if (_playerEquip != null)
            {
                _playerEquip.SetInteractableItem(_nearItems);
            }
        }

    }
}
