using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recovery Skill",menuName = "ScriptableObjects/Skills/Recovery")]
public class RecoverySkill : SkillData
{
    [Header("프리팹 생성위치")]
    public Vector3 spawnOffset = new Vector3(0, 1.0f, 0);

    public override void Execute(GameObject caster, Transform firePoint)
    {
        SoundManager.Instance.PlaySFX(SoundManager.SoundType.Skill_E);

        SkillLevelInfo info = GetCurrentLevelData();

        if (info.mainPrefab == null) return;

        PlayerStats stats = caster.GetComponent<PlayerStats>();
        if (stats == null) return;

        // 3. 회복 이펙트 생성 (플레이어를 따라다니도록 부모 설정)
        GameObject go = Instantiate(info.mainPrefab, caster.transform);
        go.transform.localPosition = spawnOffset;

        // 회복 전용 스크립트(HealEffect)에 데이터 전달
        Heal healEffect = go.GetComponent<Heal>();
        if (healEffect != null)
        {
            // 메인 배율(mainDamageMult)을 사용하여 최대 체력 대비 회복량 계산
            int healAmount = Mathf.RoundToInt(stats.maxHp * info.mainDamageMult);

            // 타수(mainHitCount)만큼 회복 실행
            healEffect.Setup(stats, healAmount, info.mainHitCount);
        }
    }
}
