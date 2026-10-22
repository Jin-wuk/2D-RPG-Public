using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Circular Slash Skill", menuName = "ScriptableObjects/Skills/CircularSlash")]
public class SlashSkill : SkillData
{
    public override void Execute(GameObject caster, Transform firePoint)
    {
        SoundManager.Instance.PlaySFX(SoundManager.SoundType.Skill_W);

        // 1. 현재 레벨의 데이터 뭉치 가져오기
        SkillLevelInfo info = GetCurrentLevelData();

        // 2. 메인 프리팹(슬래시 이펙트)이 없으면 리턴
        if (info.mainPrefab == null) return;

        // 3. 프리팹 생성 및 부모 설정 (플레이어를 따라다니게 함)
        // 
        GameObject go = Instantiate(info.mainPrefab, caster.transform);

        // 4. 위치 및 회전 초기화 (부모인 플레이어 정중앙에 위치)
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;

        // 5. Explosion 컴포넌트 설정
        Explosion explosion = go.GetComponent<Explosion>();
        if (explosion != null)
        {
            // [핵심] 이제 계산된 데미지가 아니라 '배율'과 '타수'를 직접 전달합니다.
            // Explosion 내부에서 PlayerStats.Instance.TotalAtk * multiplier 로 계산될 것입니다.
            explosion.Setup(info.mainDamageMult, info.mainHitCount);
        }
    }
}