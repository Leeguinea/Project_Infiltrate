using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    private PlayerEquip _playerEquip;

    public List<ItemType> items = new List<ItemType>();

    public void AddItem(ItemType newITem)
    {
        items.Add(newITem);
    }

}
