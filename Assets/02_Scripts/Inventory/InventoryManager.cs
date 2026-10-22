using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public List<InventorySlot> inventorySlots;  //상단 인벤토리 슬롯들
    public List<InventorySlot> equipmentSlots;  //하단 장비 슬롯들

    public bool isShopOpen = false; //상점이 열렸는지 체크

    [Header("포션 쿨타임 설정")]
    [SerializeField] public float potionCooldown = 1.0f; // 1초 쿨타입
    private Dictionary<ConsumableType, float> lastPotionTimes = new Dictionary<ConsumableType, float>();

    // 외부에서 쿨타임 시간을 가져갈 수 있도록 함수 제공
    public float GetLastPotionTime(ConsumableType type)
    {
        if (lastPotionTimes.ContainsKey(type)) return lastPotionTimes[type];
        return -10f; // 사용 기록이 없으면 즉시 사용 가능하도록 반환
    }

    [Header("연결할 컴포넌트")]
    public PlayerStats stats;
    public GameObject inventoryPanel;

    private void Awake()
    {
        Instance = this;

        // 게임 시작 시 인벤토리를 자동으로 닫아둡니다.
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
    }

    private void Update()
    {
        if (PlayerStats.Instance.isDead) return;

        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }
    /// <summary>
    /// 1. [외부 호출용] 몬스터 드랍, 상점 구매 등 '새로운' 아이템을 얻을 때 사용
    /// </summary>
    public bool AddNewItem(ItemData originalData)
    {
        if (originalData == null) return false;

        if (originalData.maxStack > 1)
        {
            // 이름이 같고, 아직 maxStack만큼 차지 않은 슬롯이 있는지 확인
            InventorySlot existingSlot = inventorySlots.Find(s =>
                s.item != null &&
                s.item.itemName == originalData.itemName &&
                s.itemCount < originalData.maxStack);

            if (existingSlot != null)
            {
                existingSlot.itemCount++; // 개수 증가
                existingSlot.UpdateSlotUI();
                return true;
            }
        }

        // 중복 가능한 아이템이 없거나 장비 아이템인 경우 새로 생성
        ItemData newItemInstance = ItemFactory.CreateItemInstance(originalData);
        return AddToEmptySlot(newItemInstance);
    }

    /// <summary>
    /// 2. [내부/외부 호출용] 장착 해제 등 '이미 생성된' 아이템을 인벤토리로 옮길 때 사용
    /// </summary>
    public bool AddExistingItem(ItemData existingItem)
    {
        if (existingItem == null) return false;

        // 추가 복제 없이 그대로 빈 슬롯에 추가
        return AddToEmptySlot(existingItem);
    }

    /// <summary>
    /// 3. [내부 로직] 실제 빈 슬롯을 찾아 아이템을 꽂아주는 공통 함수
    /// </summary>
    private bool AddToEmptySlot(ItemData item)
    {
        InventorySlot emptySlot = inventorySlots.Find(s => s.item == null);

        if (emptySlot != null)
        {
            emptySlot.item = item;
            emptySlot.itemCount = 1;
            emptySlot.UpdateSlotUI();
            return true;
        }

        Debug.LogWarning("인벤토리에 빈자리가 없습니다.");
        return false;
    }

    //아이템 클릭 시 처리 (인벤토리 <-> 장비창 이동)
    public void HandleItemClick(InventorySlot clickedSlot)
    {
        if (stats != null && stats.isDead) return;

        if (clickedSlot.item == null) return;

        // 1. 상점/강화창 열려있을 때 로직 (기존 유지)
        if (isShopOpen) { SellItem(clickedSlot); return; }
        if (EnhanceManager.Instance != null && EnhanceManager.Instance.enhancePanel.activeSelf)
        {
            if (clickedSlot.item.itemType == ItemType.Equipment)
                EnhanceManager.Instance.SetTargetItem(clickedSlot.item, clickedSlot);
            return;
        }

        // 2. [핵심 수정] 아이템 타입에 따른 분기 처리
        if (clickedSlot.item.itemType == ItemType.Equipment)
        {
            // 장비 아이템 로직
            if (clickedSlot.isEquipment) UnEquip(clickedSlot);
            else Equip(clickedSlot);
        }
        else if (clickedSlot.item.itemType == ItemType.Consumable)
        {
            // [여기 추가!] 소모품(포션)일 경우 사용 함수 호출
            UseConsumable(clickedSlot);
        }
    }

    public void Equip(InventorySlot slot)
    {

        // 맞는 장비 부위 슬롯 찾기
        InventorySlot targetSlot = equipmentSlots.Find(s => s.slotType == slot.item.equipType);

        SoundManager.Instance.PlaySFX(SoundManager.SoundType.Equip);

        // 교체 로직 실행
        ItemData temp = targetSlot.item;
        targetSlot.item = slot.item;
        slot.item = temp;

        targetSlot.UpdateSlotUI();
        slot.UpdateSlotUI();
        RefreshTotalEquipmentStats();

    }

    public void UnEquip(InventorySlot slot)
    {
        // 이미 인스턴스화 된 아이템이므로 AddExistingItem을 호출.
        if (AddExistingItem(slot.item))
        {
            SoundManager.Instance.PlaySFX(SoundManager.SoundType.Unequip);

            slot.item = null; // 기존 장비 슬롯 비우기
            slot.UpdateSlotUI();

            // 스탯 업데이트 함수가 있다면 여기서 호출하세요.
            RefreshTotalEquipmentStats();
        }
    }

    // 아이콘 버튼이나 키보드 입력 시 호출
    public void ToggleInventory()
    {
        if (stats != null && stats.isDead) return;

        if (inventoryPanel != null)
        {
            bool isActive = inventoryPanel.activeSelf;
            inventoryPanel.SetActive(!isActive);
        }
    }

    public void RefreshTotalEquipmentStats()
    {
        if (stats == null) return;

        int totalAtkBonus = 0;
        int totalDefBonus = 0;

        // 모든 장착 슬롯을 검사하여 아이템 성능 합산
        foreach (InventorySlot slot in equipmentSlots)
        {
            if (slot.item != null)
            {
                totalAtkBonus += slot.item.atkBonus;
                totalDefBonus += slot.item.defBonus;
            }
        }

        //계산된 총합을 PlayerStats에 한 번에 전달
        stats.SetEquipmentBonus(totalAtkBonus, totalDefBonus);
    }


    void SellItem(InventorySlot slot)
    {
        SoundManager.Instance.PlaySFX(SoundManager.SoundType.Shop_Transaction);

        int price = slot.item.sellPrice * slot.itemCount;
        stats.UpdateGold(price); // 돈 증가

        slot.item = null; // 슬롯 비우기
        slot.itemCount = 0;
        slot.UpdateSlotUI();

        Debug.Log($"아이템을 {price} G에 판매했습니다.");
    }

    private void UseConsumable(InventorySlot slot)
    {
        if (slot.item == null) return;

        ItemData potion = slot.item;
        ConsumableType type = potion.consumableType;

        //  타입별 쿨타임 체크
        float lastTime = GetLastPotionTime(type);
        if (Time.time < lastTime + potionCooldown) return;

        SoundManager.Instance.PlaySFX(SoundManager.SoundType.PotionUse);

        if (type == ConsumableType.Health) stats.RestoreHP(potion.restoreAmount);
        else if (type == ConsumableType.Mana) stats.RestoreMP(potion.restoreAmount);

        // 시간 기록 업데이트
        lastPotionTimes[type] = Time.time;

        slot.itemCount--;
        // 아이템을 사용한 후에도 개수가 남았다면 UI 갱신, 0이면 비우기
        if (slot.itemCount <= 0) slot.item = null;

        slot.UpdateSlotUI();
    }

    /// <summary>
    /// 퀵슬롯에서 아이템 이름으로 아이템을 사용하기 위해 호출
    /// </summary>
    public void UseItemByName(string itemName)
    {
        // 인벤토리 슬롯 중 아이템 이름이 일치하는 첫 번째 슬롯을 찾음
        InventorySlot slot = inventorySlots.Find(s => s.item != null && s.item.itemName == itemName);

        if (slot != null)
        {

            UseConsumable(slot); // 기존에 만들어둔 소모품 사용 로직 호출
        }
    }

    /// <summary>
    /// 퀵슬롯에 표시할 해당 아이템의 총 개수를 반환
    /// </summary>
    public int GetItemCount(string itemName)
    {
        int totalCount = 0;
        foreach (var slot in inventorySlots)
        {
            if (slot.item != null && slot.item.itemName == itemName)
            {
                totalCount += slot.itemCount;
            }
        }
        return totalCount;
    }

    /// <summary>
    /// [강화/제작용] 특정 아이템을 정해진 수량만큼 인벤토리에서 삭제
    /// </summary>
    public void RemoveItem(string itemName, int amount)
    {
        int remainingToRemove = amount;

        // 인벤토리 슬롯을 돌면서 이름이 일치하는 아이템을 찾음
        foreach (var slot in inventorySlots)
        {
            if (slot.item != null && slot.item.itemName == itemName)
            {
                if (slot.itemCount >= remainingToRemove)
                {
                    // 현재 슬롯의 개수가 지워야 할 양보다 많거나 같으면 완료
                    slot.itemCount -= remainingToRemove;
                    remainingToRemove = 0;
                }
                else
                {
                    // 현재 슬롯의 개수가 부족하면 슬롯을 다 비우고 다음 슬롯으로 넘어감
                    remainingToRemove -= slot.itemCount;
                    slot.itemCount = 0;
                }

                // 슬롯이 0개가 되면 아이템 데이터 삭제
                if (slot.itemCount <= 0)
                {
                    slot.item = null;
                }

                slot.UpdateSlotUI();

                // 모두 지웠다면 루프 종료
                if (remainingToRemove <= 0) break;
            }
        }
    }
    // [추가] 죽었을 때나 강제로 닫아야 할 때 사용하는 함수
    public void ForceCloseInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }
}