using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SkillUI : MonoBehaviour
{
    public static SkillUI Instance;

    [Header("연결할 패널")]
    [SerializeField] private GameObject skillWindow; // 스킬창 부모 오브젝트
    [SerializeField] private GameObject detailView;

    [Header("상세 정보 UI 컴포넌트")]
    public TextMeshProUGUI skillNameText;
    public TextMeshProUGUI currentStatsText;

    private SkillData selectedSkill;

    // 외부에서 "창이 열려있는지" 확인할 수 있는 통로를 만듭니다.
    public bool IsWindowOpen => skillWindow != null && skillWindow.activeSelf;

    public void Awake()
    {
        Instance = this;
        if (skillWindow != null ) skillWindow.SetActive(false);
    }

    void Update()
    {
        if (SkillManager.Instance.stats != null && SkillManager.Instance.stats.isDead)
            return;

        // K키를 누르면 창을 껐다 켬
        if (Input.GetKeyDown(KeyCode.K))
        {
            ToggleSkillWindow();
        }
    }

    public void ToggleSkillWindow()
    {
        if (skillWindow == null) return;

        bool isActive = skillWindow.activeSelf;
        skillWindow.SetActive(!isActive);

        if (isActive)
        {
            // 창이 열릴 때 상세창은 일단 꺼둡니다.
            if (detailView != null) detailView.SetActive(false);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    // [핵심] 리스트 버튼이 클릭되었을 때 호출될 함수
    public void SelectSkill(SkillData skill)
    {
        // [추가] 사망 상태라면 상세 정보 보기 차단
        if (SkillManager.Instance.stats != null && SkillManager.Instance.stats.isDead)
            return;

        selectedSkill = skill;
        if (detailView != null) detailView.SetActive(true);
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (selectedSkill == null) return;

        // 현재 레벨 정보 가져오기
        SkillLevelInfo current = selectedSkill.GetCurrentLevelData();

        skillNameText.text = $"{selectedSkill.skillName} (Lv.{selectedSkill.currentLevel})";
        string statsInfo = "";

        // [Main 스탯] 칼날, 투사체, 회복 등
        statsInfo += $"[Main] Damage: {current.mainDamageMult * 100}% / Hit: {current.mainHitCount}\n";

        // [Sub 스탯] 폭발, 추가 타격 등 (서브 프리팹이 있을 때만 표시)
        if (current.subPrefab != null)
        {
            statsInfo += $"[Explosion] Damage: {current.subDamageMult * 100}% / Hit: {current.subHitCount}\n";
        }

        // [기타 정보] 마나 소모량 및 쿨타임
        statsInfo += $"\nMana: {current.manaCost} / Cooldown: {current.cooldown}s";

        currentStatsText.text = statsInfo;
    }
    public void ForceCloseSkillWindow()
    {
        if (skillWindow != null)
        {
            skillWindow.SetActive(false);
            if (detailView != null) detailView.SetActive(false);
        }
    }
}
