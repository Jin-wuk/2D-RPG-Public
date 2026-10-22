using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    public enum ItemType { Gold, Equipment, Etc , Consumable }

    [Header("아이템 설정")]
    public ItemType type;
    public int goldAmount = 10;

    public ItemData itemData;   //장비나 기타 아이템일 때 연결

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //플레이어 태그 확인
        if (collision.CompareTag("Player"))
        {
            SoundManager.Instance.PlaySFX(SoundManager.SoundType.ItemPickup);

            if (type == ItemType.Gold)
            {
                // 골드 획득 로직
                var stats = collision.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    stats.UpdateGold(goldAmount);
                    Destroy(gameObject);
                }
            }
            else
            {
                // 장비(Equipment)나 기타(Etc) 아이템 획득 로직
                if (itemData != null)
                {
                    // [수정] AddItem 대신 AddNewItem을 호출하여 ItemFactory를 거치게 합니다.
                    // 이렇게 해야 주운 아이템마다 독립적인 강화 수치를 가질 수 있습니다.
                    bool isAdded = InventoryManager.Instance.AddNewItem(itemData);
                    if (isAdded)
                    {
                        Debug.Log($"{itemData.itemName}을(를) 획득했습니다.");
                        Destroy(gameObject);
                    }
                }
            }
        }
    }
}