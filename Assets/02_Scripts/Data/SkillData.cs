using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 레벨별 상세 데이터를 담는 구조체
[System.Serializable]
public struct SkillLevelInfo
{
    public int level;           // 레벨 (1, 2, 3...)
    public int manaCost;        // 해당 레벨의 마나 소모량
    public float cooldown;      // 해당 레벨의 쿨타임

    [Header("Main: 주 효과 (투사체, 칼날, 회복 등)")]
    public GameObject mainPrefab;     // 해당 레벨에서 발동할 메인 프리팹
    public float mainDamageMult;      // 메인 데미지 배율 (TotalAtk의 n배)
    public int mainHitCount;          // 메인 타수 (관통 횟수 등)

    [Header("Sub: 추가 효과 (폭발, 충격파 등)")]
    public GameObject subPrefab;      // 추가로 터질 프리팹 (없으면 Null)
    public float subDamageMult;       // 서브 전용 데미지 배율
    public int subHitCount;           // 서브 전용 타수

    [Header("레벨별 연출")]
    public string animationTriggerName; // 레벨별 애니메이션 트리거

    [Header("강화 비용")]
    public int upgradeGoldCost;
    public List<ItemAmount> upgradeItem;

}

[System.Serializable]
public struct ItemAmount    //필요한 아이템과 개수를 묶는 구조체
{
    public ItemData item;   //필요 아이템
    public int amount;    //필요 수량
}

[CreateAssetMenu(fileName = "New Skill", menuName = "ScriptableObjects / SkillData")]
public abstract class SkillData : ScriptableObject
{
    [Header("기본 정보")]
    public string skillName;
    public Sprite icon;
    [TextArea] public string description;

    public bool isUnlocked = false;    // 이 스킬을 배웠는지 여부
    public int currentLevel = 0; // 현재 플레이어의 스킬 레벨
    public float range = 8f;

    [Header("레벨별 데이터 리스트")]
    public List<SkillLevelInfo> levelInfos; // 인스펙터에서 레벨별로 데이터를 넣습니다.

    [Header("시전 설정 (Casting Settings)")]
    public bool isInvincibleDuringCast; // 시전 중 무적 여부
    public bool isRootedDuringCast;     // 시전 중 이동 불가 여부



    // 현재 레벨에 맞는 정보를 가져오는 도우미 함수
    public SkillLevelInfo GetCurrentLevelData()
    {
        // 리스트의 범위를 벗어나지 않도록 방어 코드 추가
        if (levelInfos == null || levelInfos.Count == 0) return default;

        // 현재 레벨이 0(미해금)이라면 1레벨(인덱스 0)의 정보를 기본으로 보여줍니다.
        int index = Mathf.Clamp(currentLevel - 1, 0, levelInfos.Count - 1);
        return levelInfos[index];
    }

    public abstract void Execute(GameObject caster, Transform firePotin);
}
