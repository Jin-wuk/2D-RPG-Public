using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ItemData item;   //현재 슬롯에 담긴 아이템
    public Image iconImage; //아이콘 UI
    public bool isEquipment; //이 슬롯이 장비착용인지 여부
    public EquipType slotType;   //장비창일 경우 어떤 부위인지

    [Header("포션 관련 UI")]
    public TextMeshProUGUI itemCountText;   //포션 개수 텍스트
    public Image cooldownImage; //쿨타임 표시용 이미지
    public int itemCount = 0;

    [Header("강화대상 아이템 표시")]
    public GameObject selectionOutline; // 선택 표시용 이미지(강화)


    private void Update()
    {
        // 쿨타임 UI 업데이트
        UpdateCooldownUI();
    }

    public void UpdateSlotUI()
    {
        if (item != null)
        {
            iconImage.sprite = item.icon;
            iconImage.enabled = true;

            // [버그 1 해결] 2개일 때 "x2"가 정상 출력되도록 로직 확인
            // itemCount가 1보다 클 때만 텍스트를 활성화합니다.
            // 이제 소모품, 재료, 기타 아이템 상관없이 개수가 1개보다 많으면 "x숫자"를 표시합니다.
            if (itemCount > 1)
            {
                itemCountText.text = "x" + itemCount.ToString();
                itemCountText.gameObject.SetActive(true);
            }
            else
            {
                itemCountText.gameObject.SetActive(false);
            }
        }
        else
        {
            iconImage.enabled = false;

            if (itemCountText != null) itemCountText.gameObject.SetActive(false);
            if (cooldownImage != null)
            {
                cooldownImage.fillAmount = 0;
                cooldownImage.gameObject.SetActive(false);
            }
            // [버그 4 해결] 슬롯 데이터 초기화
            itemCount = 0;
        }
    }

    //  쿨타입 시각화 함수
    private void UpdateCooldownUI()
    {
        if (item == null || item.itemType != ItemType.Consumable || cooldownImage == null)
        {
            if (cooldownImage != null) cooldownImage.gameObject.SetActive(false);
            return;
        }

        // InventoryManager에서 이 슬롯 아이템의 "타입"에 맞는 쿨타임 시간을 가져옵니다.
        float cooldown = InventoryManager.Instance.potionCooldown;
        float lastUsed = InventoryManager.Instance.GetLastPotionTime(item.consumableType);
        float elapsed = Time.time - lastUsed;

        if (elapsed < cooldown)
        {
            cooldownImage.gameObject.SetActive(true);
            cooldownImage.fillAmount = 1f - (elapsed / cooldown);
        }
        else
        {
            cooldownImage.gameObject.SetActive(false);
        }
    }

    //슬롯 클릭 시 실행될 함수 (버튼 이벤트에 연결)
    public void OnClickSlot()
    {
        if (item != null)
        {
            InventoryManager.Instance.HandleItemClick(this);
        }
    }
    //강화창에서 선택된 아이템 강조 함수
    public void SetHighlight(bool isActive)
    {
        if (selectionOutline != null)
            selectionOutline.SetActive(isActive);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item != null)
        {
            ItemToolTip.Instance.ShowTooltip(item);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ItemToolTip.Instance.HideTooltip();
    }

    // 슬롯이 닫히거나 파괴될 때 툴팁이 떠 있는 현상 방지
    private void OnDisable()
    {
        if (ItemToolTip.Instance != null) ItemToolTip.Instance.HideTooltip();
    }
}
