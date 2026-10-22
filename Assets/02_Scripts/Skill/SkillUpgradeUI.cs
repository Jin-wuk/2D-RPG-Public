using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class SkillUpgradeUI : MonoBehaviour
{
    public static SkillUpgradeUI Instance;

    [Header("패널 영역")]
    public GameObject skillUpgradePanel;    // 전체 루트 패널
    public GameObject listView;             // 왼쪽 스킬 리스트 영역
    public GameObject detailView;           // 오른쪽 상제 정보 영역
    public GameObject costFrame;

    [Header("상세 정보(DetailView) 컴포넌트")]
    public TextMeshProUGUI skillNameText;
    public TextMeshProUGUI currentStatsText;
    public TextMeshProUGUI nextStatsText;
    public TextMeshProUGUI costText;
    public Button upgradeButton;

    [Header("재료 아이템 UI")]
    public GameObject[] materialSlots;
    public Image[] materialIcons;
    public TextMeshProUGUI[] materialAmountTexts;

    // 현재 어떤 스킬을 강화할지 보관하는 변수
    private SkillData selectedSkill;


    private void Awake()
    {
        Instance = this;
        if (skillUpgradePanel != null) skillUpgradePanel.SetActive(false);
    }

    // 건물 상호작용 함수
    public void ToggleShop()
    {
        if (skillUpgradePanel == null) return;

        bool isActive = !skillUpgradePanel.activeSelf;
        skillUpgradePanel.SetActive(isActive);

        if (isActive)
        {
            // 상점을 열 때 처음에는 리스트만 보여주고 상세창은 꺼둡니다.
            listView.SetActive(true);
            detailView.SetActive(false);
        }
    }

    public void CloseShop() => skillUpgradePanel.SetActive(false);


    // [핵심] 리스트의 각 스킬 버튼들이 클릭될 때 이 함수를 호출해야 합니다.
    public void SelectSkill(SkillData skill)
    {
        selectedSkill = skill;      // 클릭한 스킬 데이터를 저장
        detailView.SetActive(true); // 상세창을 켭니다.
        UpdateUI();                 // 정보 갱신
    }

    public void UpdateUI()
    {
        if (selectedSkill == null) return;

        detailView.SetActive(true);

        // 1. 현재 레벨 데이터 정보 조립
        SkillLevelInfo current = selectedSkill.GetCurrentLevelData();
        skillNameText.text = selectedSkill.skillName + (selectedSkill.currentLevel == 0 ? " (미습득)" : $" (Lv.{selectedSkill.currentLevel})");

        // [Main/Sub 분리 표시]
        string currentInfo = $"[Main] {current.mainDamageMult * 100}% (Hit: {current.mainHitCount})";
        if (current.subPrefab != null)
        {
            currentInfo += $"\n[Sub] {current.subDamageMult * 100}% (Hit: {current.subHitCount})";
        }
        currentStatsText.text = currentInfo;

        // 2. 만렙 체크 및 다음 레벨 정보 표시
        if (selectedSkill.currentLevel < selectedSkill.levelInfos.Count)
        {
            if (costFrame != null) costFrame.SetActive(true);

            // 다음 레벨 주머니 가져오기
            SkillLevelInfo next = selectedSkill.levelInfos[selectedSkill.currentLevel];

            // [Main/Sub 다음 레벨 정보 조립]
            string nextInfo = $"-> [Main] {next.mainDamageMult * 100}% (Hit: {next.mainHitCount})";
            if (next.subPrefab != null)
            {
                // 이전 레벨에 폭발이 없었다면 "새로운 효과 추가!" 같은 문구를 띄워주면 좋습니다.
                string subPrefix = (current.subPrefab == null) ? "\n★ [New] " : "\n-> [Sub] ";
                nextInfo += $"{subPrefix}{next.subDamageMult * 100}% (Hit: {next.subHitCount})";
            }
            nextStatsText.text = nextInfo;

            costText.text = next.upgradeGoldCost.ToString();

            // 3. 재료 아이템 업데이트
            for (int i = 0; i < materialSlots.Length; i++)
            {
                if (i < next.upgradeItem.Count)
                {
                    materialSlots[i].SetActive(true);
                    var req = next.upgradeItem[i];
                    materialIcons[i].sprite = req.item.icon;
                    int hasCount = InventoryManager.Instance.GetItemCount(req.item.itemName);
                    materialAmountTexts[i].text = $"{hasCount}/{req.amount}";
                    materialAmountTexts[i].color = (hasCount >= req.amount) ? Color.white : Color.red;
                }
                else
                {
                    materialSlots[i].SetActive(false);
                }
            }

            // 강화 가능 여부 체크 (SkillUpgradeManager가 필요함)
            upgradeButton.interactable = SkillUpgradeManager.Instance.CanUpgrade(selectedSkill);
        }
        else
        {
            // 만렙 처리
            nextStatsText.text = "MAX LEVEL";
            if (costFrame != null) costFrame.SetActive(false);
            upgradeButton.interactable = false;
        }
    }

    // 버튼에 연결할 함수
    public void OnUpgradeClick()
    {
        if (selectedSkill == null) return;

        // 매니저에게 선택된 스킬 강화를 요청
        SkillUpgradeManager.Instance.UpgradeSkill(selectedSkill);
        UpdateUI(); // 강화 후 텍스트 갱신
    }
}
