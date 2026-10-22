using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillListButton : MonoBehaviour
{
    public SkillData mySkill;   // 버튼 담당할 스킬 데이터
    public Image iconImage;     // 스킬 아이콘 표시용
    public Button targetButton;

    private void Start()
    {
        if (mySkill != null && iconImage != null)
        {
            iconImage.sprite = mySkill.icon;
        }

        // GetComponent 대신 직접 연결된 버튼에 이벤트를 등록합니다.
        if (targetButton != null)
        {
            targetButton.onClick.AddListener(OnClickButton);
        }
        else
        {
            Debug.LogError($"{gameObject.name}의 Target Button이 비어있습니다!");
        }
    }

    public void OnClickButton()
    {
        if (mySkill == null) return;

        // 1. 강화 창이 열려 있는 경우
        if (SkillUpgradeUI.Instance != null && SkillUpgradeUI.Instance.skillUpgradePanel.activeInHierarchy)
        {
            SkillUpgradeUI.Instance.SelectSkill(mySkill);
        }
        // 2. K키 스킬 창이 열려 있는 경우
        else if (SkillUI.Instance != null && SkillUI.Instance.gameObject.activeInHierarchy)
        {
            SkillUI.Instance.SelectSkill(mySkill);
        }
    }

}
