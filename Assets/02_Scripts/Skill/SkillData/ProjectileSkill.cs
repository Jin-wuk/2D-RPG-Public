using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Projectile Skill", menuName = "ScriptableObjects/Skills/Projectile")]
public class ProjectileSkill : SkillData
{
    [Header("투사체 전용 설정")]
    public float projectileSpeed = 10f;

    public override void Execute(GameObject caster, Transform firePotin)
    {
        SoundManager.Instance.PlaySFX(SoundManager.SoundType.Skill_Q);

        SkillLevelInfo info = GetCurrentLevelData();
        if (info.mainPrefab == null) return;

        // 시전자의 방향 확인
        SpriteRenderer sr = caster.GetComponent<SpriteRenderer>();
        Quaternion rotation = (sr != null && sr.flipX) ? Quaternion.Euler(0, 0, 180) : Quaternion.identity;

        // 메인 투사체 생성
        GameObject go = Instantiate(info.mainPrefab, firePotin.position, rotation);

        // 투사체 능력치 설정
        Projectile proj = go.GetComponent<Projectile>();
        if (proj != null)
        {
            PlayerStats stats = caster.GetComponent<PlayerStats>();
            int mainDamage = Mathf.RoundToInt(stats.totalAtk * info.mainDamageMult);

            // [수정] 폭발에 필요한 프리팹, 배율, 타수까지 모두 전달
            proj.Setup(
                mainDamage,
                range,
                projectileSpeed,
                info.mainHitCount,
                info.subPrefab,
                info.subDamageMult,
                info.subHitCount
            );
        }

        Debug.Log($"{skillName}");
    }
}
