using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum QuickSlotType { Item, Skill }    //슬롯 종류 구분(스킬, 포션)
public class QuickSlot : MonoBehaviour
{
    [Header("슬롯 설정")]
    public KeyCode hotkey;  //사용할 키
    public QuickSlotType slotType;   //등록된 아이템

    [Header("데이터 연결")]
    public ItemData assignedItem;   //아이템일때 사용
    public SkillData assignedSkill;   //스킬 데이터

    [Header("UI 컴포넌트")]
    public Image iconImage;
    public Image cooldownImage;
    public TextMeshProUGUI countText;
    public TextMeshProUGUI hotkeyText;

    private void Start()
    {
        if (hotkeyText != null)
        {
            string keyName = hotkey.ToString();

            // "Alpha1"에서 "Alpha"를 찾아 빈값("")으로 바꿉니다 -> "1"만 남음
            keyName = keyName.Replace("Alpha", "");

            // 키패드 숫자 "Keypad1"도 "1"로 표시하고 싶다면 추가
            keyName = keyName.Replace("Keypad", "");

            // 만약 Space 같은 키를 "공백"이나 "SPC"로 바꾸고 싶다면 아래처럼 추가 가능합니다.
            // if (keyName == "Space") keyName = "SPC";

            hotkeyText.text = keyName;
        }
    }

    private void Update()
    {
        //사망 체크
        bool isDead = SkillManager.Instance.stats != null && SkillManager.Instance.stats.isDead;

        if (!isDead && Input.GetKeyDown(hotkey)) UseSlot();
        UpdateUI();
    }

    private void UseSlot()
    {
        // 마우스 클릭 등으로 호출될 경우를 대비한 2중 방어막
        if (SkillManager.Instance.stats != null && SkillManager.Instance.stats.isDead) return;

        // 1. 아이템(포션 등) 슬롯인 경우
        if (slotType == QuickSlotType.Item && assignedItem != null)
        {

            // 기존에 주석 처리했던 인벤토리 아이템 사용 로직을 실행해야 합니다.
            InventoryManager.Instance.UseItemByName(assignedItem.itemName);
        }
        // 2. 스킬 슬롯인 경우 (이 부분이 핵심!)
        else if (slotType == QuickSlotType.Skill && assignedSkill != null)
        {
            // 스킬이 해금(isUnlocked)되었고 레벨이 0보다 클 때만 시전
            if (assignedSkill.isUnlocked && assignedSkill.currentLevel > 0)
            {
                SkillManager.Instance.CastSkill(assignedSkill);
            }
            else
            {
                Debug.Log($"{assignedSkill.skillName}은 아직 배우지 않은 스킬입니다.");
            }
        }
    }

    private void UpdateUI()
    {
        if (slotType == QuickSlotType.Item) UpdateItemUI();
        else UpdateSkillUI();
    }

    private void UpdateItemUI()
    {
        if (assignedItem == null)
        {
            iconImage.enabled = false;
            countText.text = "";
            cooldownImage.gameObject.SetActive(false);
            return;
        }

        iconImage.enabled = true;
        iconImage.sprite = assignedItem.icon;

        // 1. 개수 표시 (인벤토리 매니저의 GetItemCount 활용)
        int count = InventoryManager.Instance.GetItemCount(assignedItem.itemName);
        countText.text = count > 0 ? count.ToString() : "0";

        // 2. 쿨타임 표시 (Dictionary 기반 로직으로 수정)
        float lastTime = InventoryManager.Instance.GetLastPotionTime(assignedItem.consumableType);
        float cooldown = InventoryManager.Instance.potionCooldown;
        float elapsed = Time.time - lastTime;

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

    private void UpdateSkillUI()
    {
        if (assignedSkill == null)
        {
            iconImage.enabled = false;
            countText.text = "";
            cooldownImage.gameObject.SetActive(false);
            return;
        }

        SkillLevelInfo currentInfo = assignedSkill.GetCurrentLevelData();

        iconImage.enabled = true;
        iconImage.sprite = assignedSkill.icon;

        // 1. 마나 소모량 표시 (마나가 부족하면 빨간색으로 표시)
        countText.text = currentInfo.manaCost.ToString();

        if (SkillManager.Instance.stats.currentMp < currentInfo.manaCost)
            countText.color = Color.red;
        else
            countText.color = Color.white;

        // 2. 쿨타임 표시 (SkillManager의 딕셔너리 데이터 활용)
        float remaining = SkillManager.Instance.GetRemainingCooldown(assignedSkill);
        float total = currentInfo.cooldown;

        if (remaining > 0)
        {
            cooldownImage.gameObject.SetActive(true);
            cooldownImage.fillAmount = remaining / total; // 남은 비율만큼 채움
        }
        else
        {
            cooldownImage.gameObject.SetActive(false);
        }
    }
}

