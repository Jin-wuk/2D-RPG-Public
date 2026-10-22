using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;
    public PlayerStats stats;
    public Animator anim;
    public Transform firePoint;

    [Header("글로벌 쿨타임 설정")]
    [SerializeField] private float gcdDuration = 0.3f;
    private bool isGlobalCooldown = false;

    //쿨타임 관리용
    private Dictionary<string, float> lastUsageTimes = new Dictionary<string, float>();

    private void Awake() => Instance = this;

    // 현재 쿨타임이 얼마나 남았는지 확인 (UI용)
    public float GetRemainingCooldown(SkillData skill)
    {
        // 스킬이 없거나 배우지 않았다면 쿨타임은 무조건 0
        if (skill == null || !skill.isUnlocked || skill.currentLevel <= 0) return 0;
        if (!lastUsageTimes.ContainsKey(skill.skillName)) return 0;

        float elapsed = Time.time - lastUsageTimes[skill.skillName];
        return Mathf.Max(0, skill.GetCurrentLevelData().cooldown - elapsed);
    }

    public void CastSkill(SkillData skill)
    {
        // [추가] 플레이어가 죽었다면 즉시 리턴
        if (PlayerStats.Instance != null && PlayerStats.Instance.isDead) return;

        // 글로벌 쿨타임 중이면 즉시 리턴
        if (isGlobalCooldown || skill == null) return;

        // 해금 여부 및 레벨 체크
        if (!skill.isUnlocked || skill.currentLevel <= 0) return;

        SkillLevelInfo info = skill.GetCurrentLevelData();

        // 마나 및 개별 쿨타임 체크
        if (stats.currentMp < info.manaCost || GetRemainingCooldown(skill) > 0) return;

        // 모든 조건 통과 시 시전 루틴 시작
        StartCoroutine(ExecuteSkillRoutine(skill, info));

    }

    private IEnumerator ExecuteSkillRoutine(SkillData skill, SkillLevelInfo info)
    {
        isGlobalCooldown = true;
        // 1. 컴포넌트들 가져오기
        PlayerHealth health = stats.GetComponent<PlayerHealth>();
        PlayerMovement movement = stats.GetComponent<PlayerMovement>();

        // 2. 스킬 설정에 따른 상태 변화 (무적 및 이동 제한)
        // SkillData에 isInvincibleDuringCast, isRootedDuringCast 변수가 있다고 가정합니다.
        if (skill.isInvincibleDuringCast && health != null) health.IsInvincible = true;
        if (skill.isRootedDuringCast && movement != null) movement.CanMove = false;

        // 비용 지불 및 개별 쿨타임 기록
        stats.UpdateMp(-info.manaCost);
        lastUsageTimes[skill.skillName] = Time.time;

        // 애니메이션 트리거 실행 (Any State 연결 방식)
        if (anim != null) anim.SetTrigger(info.animationTriggerName);

        // 스킬 에셋의 로직 즉시 실행
        skill.Execute(stats.gameObject, firePoint);

        // 설정된 글로벌 쿨타임만큼 대기
        yield return new WaitForSeconds(gcdDuration);

        // 4. 원래 상태로 복구
        if (health != null) health.IsInvincible = false;
        if (movement != null) movement.CanMove = true;

        isGlobalCooldown = false;
    }
}
