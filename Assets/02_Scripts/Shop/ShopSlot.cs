using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ItemData item;
    public Image itemIcon;
    //public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI priceText;
    public Button buyButton;

    
    //상점 매니저가 슬롯 초기화할때 호출
    public void Setup(ItemData newItem)
    {
        item = newItem;
        itemIcon.sprite = item.icon;
        //itemNameText.text = item.itemName;
        priceText.text = item.buyPrice.ToString() + " G";

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => ShopManager.Instance.BuyItem(item));
    }
    // 마우스가 상점 슬롯 영역에 들어왔을 때 호출
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item != null && ItemToolTip.Instance != null)
        {
            // 이미 만들어둔 스마트 툴팁 호출!
            ItemToolTip.Instance.ShowTooltip(item);
        }
    }

    // 마우스가 상점 슬롯 영역을 나갔을 때 호출
    public void OnPointerExit(PointerEventData eventData)
    {
        if (ItemToolTip.Instance != null)
        {
            ItemToolTip.Instance.HideTooltip();
        }
    }

    // 상점 창이 닫히거나 슬롯이 비활성화될 때 툴팁이 남지 않도록 처리
    private void OnDisable()
    {
        if (ItemToolTip.Instance != null)
        {
            ItemToolTip.Instance.HideTooltip();
        }
    }
}
