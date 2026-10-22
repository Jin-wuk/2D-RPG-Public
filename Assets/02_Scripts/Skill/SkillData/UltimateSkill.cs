using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ultimate Skill", menuName = "ScriptableObjects/Skills/Ultimate")]

public class UltimateSkill : SkillData
{

    public override void Execute(GameObject caster, Transform firePoint)
    {
        SoundManager.Instance.PlaySFX(SoundManager.SoundType.Skill_R);

        SkillLevelInfo info = GetCurrentLevelData();

        // 1. 메인 프리팹(떨어지는 칼) 확인
        if (info.mainPrefab == null) return;

        PlayerStats stats = caster.GetComponent<PlayerStats>();
        if (stats == null) return;

        // 2. 메인 데미지 계산 (칼 자체의 위력)
        int mainDamage = Mathf.RoundToInt(stats.totalAtk * info.mainDamageMult);

        // 코루틴 시작 시 배분된 데이터를 넘겨줍니다.
        caster.GetComponent<MonoBehaviour>().StartCoroutine(SwordRainRoutine(caster, info, mainDamage));
    }

    private IEnumerator SwordRainRoutine(GameObject caster, SkillLevelInfo info, int mainDamage)
    {
        int swordCount = 8;
        float spawnWidth = 10f;
        float spawnHeight = 12f;

        for (int i = 0; i < swordCount; i++)
        {
            float rawX = caster.transform.position.x + Random.Range(-spawnWidth, spawnWidth);
            float clampedX = Mathf.Clamp(rawX, MapManager.Instance.MinX + 0.5f, MapManager.Instance.MaxX - 0.5f);

            Vector3 rayOrigin = new Vector3(clampedX, 20f, 0);
            RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, Vector2.down, 40f, LayerMask.GetMask("Ground", "Platform"));

            if (hits.Length > 0)
            {
                RaycastHit2D targetHit = hits[Random.Range(0, hits.Length)];
                Vector3 finalSpawnPos = new Vector3(clampedX, targetHit.point.y + spawnHeight, 0);

                // 3. 메인 프리팹 생성
                GameObject go = Instantiate(info.mainPrefab, finalSpawnPos, Quaternion.identity);
                FallingSword sword = go.GetComponent<FallingSword>();

                if (sword != null)
                {
                    // [핵심 전달] 
                    // 칼 데미지(mainDamage), 칼 타수(mainHitCount)와 함께 
                    // 폭발 정보(subPrefab, subDamageMult, subHitCount)를 묶어서 전달합니다.
                    sword.Setup(
                        mainDamage,
                        info.mainHitCount,
                        info.subPrefab,
                        info.subDamageMult,
                        info.subHitCount,
                        targetHit.collider
                    );
                }
            }

            yield return new WaitForSeconds(0.15f);
        }
    }
}


