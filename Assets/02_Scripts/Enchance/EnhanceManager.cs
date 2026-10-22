using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EnhanceManager : MonoBehaviour
{
    public static EnhanceManager Instance;

    [Header("강화관련 UI 연결")]
    public GameObject enhancePanel;
    [Tooltip("강화창 중앙 아이템 아이콘")]
    public Image targetItemIcon;
    //public TextMeshProUGUI itemInfoText;    //장검 (+0) 같은 강화수치 정보
    public TextMeshProUGUI statDiffText;    //공격력 : 10 -> 12...

    private ItemData currentItem;   //현재 강화대에 올라온 아이템
    private InventorySlot sourceSlot;   //아이템이 원래 있던 슬롯(파괴 처리)

    private void Awake()
    {
        Instance = this;
        enhancePanel.SetActive(false);
    }

    public void ToggleEnhance()
    {
        // 1. 강화 패널 상태 반전
        bool isActive = !enhancePanel.activeSelf;
        enhancePanel.SetActive(isActive);

        // 2. 인벤토리 패널 동기화 및 위치 조정
        if (InventoryManager.Instance != null && InventoryManager.Instance.inventoryPanel != null)
        {
            GameObject invPanel = InventoryManager.Instance.inventoryPanel;
            invPanel.SetActive(isActive);

            RectTransform invRect = invPanel.GetComponent<RectTransform>();

            if (isActive)
            {
                // 강화창이 열릴 때: 지정된 위치로 이동 (예: x=400)
                invRect.anchoredPosition = new Vector2(400, 0);
            }
            else
            {
                // 강화창이 닫힐 때: 다시 중앙(0, 0)으로 복귀
                invRect.anchoredPosition = Vector2.zero;
            }
        }

        // 3. 아이템 정보 초기화
        ClearTargetItem();
    }

    //중앙 슬롯 버튼을 눌렀을 때 호출될 함수
    public void OnClickTargetSlot()
    {
        // 등록된 아이템이 있을 때만 실행
        if (currentItem != null)
        {
            ClearTargetItem();
            Debug.Log("강화 대상 등록이 취소되었습니다.");
        }
    }

    public void ClearTargetItem()
    {
        currentItem = null;

        if(targetItemIcon != null)
        {
            targetItemIcon.sprite = null;
            targetItemIcon.enabled = false;
        }
    }

    // 인벤토리에서 호출할 함수
    public void SetTargetItem(ItemData item, InventorySlot slot)
    {
        if (item == null) return;

        currentItem = item;
        sourceSlot = slot; // 원래 있던 슬롯 기억
        UpdateEnhanceUI();
    }

    //UI 표시 시 현재 확률도 보여주면 좋습니다.
    void UpdateEnhanceUI()
    {
        if (currentItem == null) return;

        targetItemIcon.sprite = currentItem.icon;
        targetItemIcon.enabled = true;

        int chance = Mathf.Max(0, 100 - (currentItem.level * 10));

        string atkInfo = $"Atk: {currentItem.atkBonus} -> {currentItem.atkBonus + currentItem.atkUpgrade}";
        string defInfo = $"Def: {currentItem.defBonus} -> {currentItem.defBonus + currentItem.defUpgrade}";

        statDiffText.text = $"<b>{currentItem.itemName} (+{currentItem.level})</b>\n" +
                           $"{atkInfo}\n{defInfo}\n\n" +
                           $"Rate: <color=yellow>{chance}%</color>\n" +
                           $"Cost: {currentItem.upgradePrice} G";
    }


    public void OnClickEnhance()
    {
        if(currentItem == null || sourceSlot == null) return;

        //골드 체크
        if (InventoryManager.Instance.stats.SpendGold(currentItem.upgradePrice))
        {
            // 2. 확률 계산: 100% - (현재레벨 * 10%)
            // 최소 확률을 0% 미만으로 내려가지 않게 Mathf.Max 사용 (최소 5%나 10% 권장)
            int currentSuccessChance = Mathf.Max(0, 100 - (currentItem.level * 10));
            int roll = Random.Range(0, 100);

            if (roll < currentSuccessChance)
            {
                // --- 강화 성공 ---
                currentItem.level++;
                currentItem.atkBonus += currentItem.atkUpgrade;
                currentItem.defBonus += currentItem.defUpgrade;

                // 강화 비용 증가 (예: 기존 비용의 1.2배)
                currentItem.upgradePrice = (int)(currentItem.upgradePrice * 1.2f);

                Debug.Log($"<color=cyan>Success!</color> {currentItem.itemName} +{currentItem.level}");
                UpdateEnhanceUI();
            }
            else
            {
                // --- 강화 실패 및 파괴 ---
                Debug.Log("<color=red>Fail!</color>");
                DestroyItem();
            }

            // 공통 처리: 아이템이 파괴되었든 성공했든 인벤토리 슬롯과 플레이어 스탯은 갱신해야 함
            if (sourceSlot != null) sourceSlot.UpdateSlotUI();
            InventoryManager.Instance.RefreshTotalEquipmentStats();
        }
    }

    // 아이템 파괴 및 UI 초기화
    void DestroyItem()
    {
        if (sourceSlot != null)
        {
            sourceSlot.item = null; // 인벤토리 슬롯 비우기
            sourceSlot.UpdateSlotUI();
        }

        ClearTargetItem(); // 강화창 초기화
        statDiffText.text = "<color=red>Fail!</color>";
    }

}
