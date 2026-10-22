using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Shop Inventory", menuName = "Shop/Inventory Data")]
public class ShopInventoryData : ScriptableObject
{
    public string shopName; // 상점 이름
    public List<ItemData> itemsToSell; // 이 상점에서 팔 아이템들
}
