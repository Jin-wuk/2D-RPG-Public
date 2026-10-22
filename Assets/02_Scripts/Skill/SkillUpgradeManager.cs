using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillUpgradeManager : MonoBehaviour
{
    public static SkillUpgradeManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    public bool CanUpgrade(SkillData skill)
    {
        // 만렙 인지 확인
        if (skill.currentLevel >= skill.levelInfos.Count)
        {
            return false;
        }

        // 다음 레벨 정보
        SkillLevelInfo nextLevel = skill.levelInfos[skill.currentLevel];

        PlayerStats playerStats = FindObjectOfType<PlayerStats>();

        if (playerStats == null) return false;

        if (playerStats.gold < nextLevel.upgradeGoldCost) return false;

        foreach (var req in nextLevel.upgradeItem)
        {
            if (InventoryManager.Instance.GetItemCount(req.item.itemName) < req.amount)
            {
                return false;
            }
        }

        return true;
    }

    public void UpgradeSkill(SkillData skill)
    {
        // 1. 강화/해금 가능 여부 먼저 체크
        if (!CanUpgrade(skill)) return;

        // [핵심] 현재 레벨이 0이면 리스트의 0번(1레벨 정보)을, 
        // 현재 레벨이 1이면 리스트의 1번(2레벨 정보)을 가져오게 됩니다.
        // 즉, 'skill.currentLevel' 자체가 '다음 레벨의 인덱스'가 됩니다.
        SkillLevelInfo nextLevel = skill.levelInfos[skill.currentLevel];
        PlayerStats playerStats = FindObjectOfType<PlayerStats>();

        // 2. 비용 지불 (골드 및 아이템)
        playerStats.SpendGold(nextLevel.upgradeGoldCost);
        foreach (var req in nextLevel.upgradeItem)
        {
            InventoryManager.Instance.RemoveItem(req.item.itemName, req.amount);
        }

        // 3. 레벨 상승 및 해금 처리
        if (skill.currentLevel == 0)
        {
            skill.isUnlocked = true;
            skill.currentLevel = 1; // 0에서 1로 습득
            Debug.Log($"{skill.skillName} 스킬을 최초로 습득했습니다!");
#if UNITY_EDITOR
            string path = UnityEditor.AssetDatabase.GetAssetPath(skill);
            Debug.Log($"수정 중인 에셋 경로: {path}");
#endif
        }
        else
        {
            skill.currentLevel++; // 1 -> 2, 2 -> 3으로 강화
            Debug.Log($"{skill.skillName} 스킬을 {skill.currentLevel}레벨로 강화했습니다!");
        }

        // [이 부분 추가] 변경된 데이터를 에디터와 파일에 반영합니다.
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(skill); // 에디터에게 데이터가 변경됨을 알림
        // UnityEditor.AssetDatabase.SaveAssets(); // (선택사항) 파일을 즉시 물리적으로 저장
        #endif
    }
}