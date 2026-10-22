using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [Header("상점 설정")]
    //직접 리스트를 가지지 않고, 데이터 에셋을 참조합니다.
    public ShopInventoryData currentShopData;

    public GameObject shopPanel;    //상점 전체 패널
    public Transform content;   //슬롯들이 생성될 부모 오브젝트
    public GameObject shopSlotPrefab;   //ShopSlot 프리팹


    private void Awake()
    {
        Instance = this;
        shopPanel.SetActive(false);
    }


    // 토글 함수: 상태에 따라 열거나 닫음
    public void ToggleShop()
    {
        if (shopPanel == null)
        {
            Debug.LogError("ShopPanel이 인스펙터에서 할당되지 않았습니다!");
            return;
        }

        if (shopPanel.activeSelf)
        {
            CloseShop();
        }
        else
        {
            OpenShop();
        }
    }

    public void OpenShop()
    {
        if (currentShopData == null)
        {
            Debug.LogError("상점 데이터(ShopInventoryData)가 할당되지 않았습니다!");
            return;
        }

        shopPanel.SetActive(true);
        InventoryManager.Instance.isShopOpen = true;

        InventoryManager.Instance.inventoryPanel.SetActive(true);
        RectTransform invRect = InventoryManager.Instance.inventoryPanel.GetComponent<RectTransform>();
        invRect.anchoredPosition = new Vector2(400, 0);

        CreateShopList();
    }

    // 상점 닫기
    public void CloseShop()
    {
        // 1. shopPanel이 살아있는지 확인
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }

        // 2. InventoryManager 인스턴스가 있는지 확인
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.isShopOpen = false;

            // 3. 인벤토리 패널 오브젝트가 살아있는지 확인
            if (InventoryManager.Instance.inventoryPanel != null)
            {
                RectTransform invRect = InventoryManager.Instance.inventoryPanel.GetComponent<RectTransform>();
                if (invRect != null)
                {
                    invRect.anchoredPosition = Vector2.zero;
                }
                InventoryManager.Instance.inventoryPanel.SetActive(false);
            }
        }
    }

    // 상점 아이템 리스트 생성
    void CreateShopList()
    {
        foreach (Transform child in content) { Destroy(child.gameObject); }

        //currentShopData 안에 들어있는 itemsToSell 리스트를 사용합니다.
        foreach (ItemData item in currentShopData.itemsToSell)
        {
            GameObject obj = Instantiate(shopSlotPrefab, content);
            obj.GetComponent<ShopSlot>().Setup(item);
        }
    }

    // 구매 로직
    public void BuyItem(ItemData item)
    {
        PlayerStats stats = InventoryManager.Instance.stats;

        if (stats.gold >= item.buyPrice)
        {
            SoundManager.Instance.PlaySFX(SoundManager.SoundType.Shop_Transaction);

            // 인벤토리에 빈자리가 있는지 확인
            if (InventoryManager.Instance.AddNewItem(item))
            {
                stats.UpdateGold(-item.buyPrice);
                Debug.Log($"{item.itemName} 구매 완료!");
            }
        }
        else
        {
            Debug.Log("골드가 부족합니다.");
        }
    }
}
