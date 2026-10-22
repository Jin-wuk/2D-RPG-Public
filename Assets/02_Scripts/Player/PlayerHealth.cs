using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct HitData
{
    public int damage;          // 피해량
    public Vector3 hitPosition; // 공격자 위치 (넉백 방향용)
    public bool useKnockback;   // 넉백 여부
    public float forceX;        // 넉백 힘 X
    public float forceY;        // 넉백 힘 Y
}

public class PlayerHealth : MonoBehaviour
{
    private PlayerStats stats;
    private SpriteRenderer spriteRenderer;
    private Animator anim;
    private Rigidbody2D rigid;

    [Header("피격 설정")]
    [SerializeField] private float invincibilityTime = 1.0f;
    private bool isInvincible = false;

    // [추가] 넉백 중 조작 제한용
    private bool isKnockback = false;
    public bool IsKnockback => isKnockback;


    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody2D>();
    }
    public bool IsInvincible
    {
        get => isInvincible;
        set => isInvincible = value;
    }

    // 충돌이 시작될 때 (최초 1회)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckEnemyCollision(collision);
    }

    // 충돌이 유지되고 있을 때 (매 프레임 호출)
    private void OnTriggerStay2D(Collider2D collision)
    {
        CheckEnemyCollision(collision);
    }


    private void CheckEnemyCollision(Collider2D collision)
    {
        if (isInvincible) return;
        if (LoadingManager.Instance != null && LoadingManager.Instance.IsLoading) return;

        if (collision.gameObject.layer == LayerMask.NameToLayer("HurtBox"))
        {
            
            Enemy enemy = collision.GetComponentInParent<Enemy>();
            if (enemy != null && !enemy.IsDead)
            {
                HitData defaultData = new HitData();
                defaultData.damage = enemy.GetEnemyDamage();
                defaultData.hitPosition = enemy.transform.position; // 몬스터의 위치
                defaultData.useKnockback = true;

                // [핵심 수정] 여기서도 방향을 계산해서 넣어줍니다!
                // 플레이어가 몬스터의 왼쪽에 있으면 -1, 오른쪽에 있으면 1
                float directionX = (transform.position.x < enemy.transform.position.x) ? -1f : 1f;

                // 이제 고정된 2f가 아니라 계산된 방향이 적용됩니다.
                defaultData.forceX = directionX * 2.5f;
                defaultData.forceY = 1.0f;

                ProcessHit(defaultData);
            }
        }
    }

    public void ProcessHit(HitData data)
    {
        if (stats.isDead || isInvincible) return;   // 죽었을 경우 피격 무시


        SoundManager.Instance.PlaySFX(SoundManager.SoundType.PlayerHit);

        // 1. 데이터 업데이트 지시
        stats.TakeDamage(data.damage);

        // 2. 피격 연출 (애니메이션, 넉백)
        if (anim != null) anim.SetTrigger("isHurt");

        // 3. 넉백 처리 (데이터에 명시된 경우만)
        if (data.useKnockback)
        {
            // [수정] enemyPos뿐만 아니라 데이터에 들어있는 forceX, forceY를 그대로 전달합니다.
            ApplyKnockback(data.hitPosition, data.forceX, data.forceY);
        }

        // 3. 무적 상태 돌입
        if (stats.currentHp > 0) StartCoroutine(InvincibilityRoutine());
        else Die();
    }
    private void ApplyKnockback(Vector3 enemyPos, float fx, float fy)
    {
        rigid.velocity = Vector2.zero;

        // 더 이상 dir을 계산해서 곱하지 않습니다.
        rigid.AddForce(new Vector2(fx, fy), ForceMode2D.Impulse);

        StartCoroutine(KnockbackControlRoutine());
    }
    private IEnumerator KnockbackControlRoutine()
    {
        isKnockback = true;
        yield return new WaitForSeconds(0.2f); // 넉백 지속 시간
        isKnockback = false;
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        float timer = 0;
        while (timer < invincibilityTime)
        {
            spriteRenderer.color = new Color(1, 1, 1, 0.5f);
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            timer += 0.2f;
        }
        isInvincible = false;
    }

    private void Die()
    {
        if (anim != null)
        {
            anim.SetTrigger("isDie");
        }
        rigid.velocity = Vector2.zero;
        rigid.simulated = false;

        // [UI 강제 종료] 모든 창을 뒷문으로 닫기
        if (InventoryManager.Instance != null) InventoryManager.Instance.ForceCloseInventory();
        if (SkillUI.Instance != null) SkillUI.Instance.ForceCloseSkillWindow();
        if (SettingsManager.Instance != null) SettingsManager.Instance.ForceCloseSettings();

        if (GetComponent<PlayerController>() != null)
        {
            GetComponent<PlayerController>().enabled = false;
        }
        // 3. 슬로우 모션 코루틴 실행
        StartCoroutine(SlowMotionDeath());
    }

    IEnumerator SlowMotionDeath()
    {
        ResetBossBattleOnPlayerDeath();

        Time.timeScale = 0.2f;

        yield return new WaitForSecondsRealtime(5f);

        Time.timeScale = 0f; // 대기 후 게임 정지

        // 게임오버 패널 띄우기
        if (GameOverUI.Instance != null)
        {
            GameOverUI.Instance.Show();
        }
    }

    private void ResetBossBattleOnPlayerDeath()
    {
        // 1. [추가] 씬에 남아있는 모든 레이저 및 예고 범위 프리팹 제거
        // 프리팹들에 "Indicator" 혹은 "Effect" 태그를 달아두면 관리가 더 편합니다.
        GameObject[] laserEffects = GameObject.FindGameObjectsWithTag("Indicator"); // 태그 사용 시
        foreach (var effect in laserEffects) Destroy(effect);

        // 씬에 있는 모든 보스를 찾아 제거합니다.
        BossStats activeBoss = FindObjectOfType<BossStats>();
        if (activeBoss != null)
        {
            // 보스 체력바 UI 정리
            if (BossHPUI.Instance != null) BossHPUI.Instance.ClearUI();

            // 보스가 들고 있던 포탈 다시 켜기 (필요 시)
            GameObject portal = activeBoss.GetExitPortal();
            GameObject altar = activeBoss.GetAltar();

            // 3. 포탈 복구 (즉시 활성화)
            if (portal != null) StartCoroutine(FadeInObject(portal));

            // 4. 제단 복구 및 리셋
            if (altar != null)
            {
                altar.SetActive(true);
                altar.GetComponent<BossAltar>()?.ResetAltar();
                StartCoroutine(FadeInObject(altar));
            }

            // 5. 보스 HP UI 정리 및 보스 제거
            if (BossHPUI.Instance != null) BossHPUI.Instance.ClearUI();
            Destroy(activeBoss.gameObject);
        }
        else
        {
            // 보스가 이미 죽어서 비활성화된 제단들을 찾아야 할 경우
            BossAltar[] altars = Resources.FindObjectsOfTypeAll<BossAltar>();
            foreach (var altar in altars)
            {
                altar.gameObject.SetActive(true);
                altar.ResetAltar();
            }
        }
    }
    private IEnumerator FadeInObject(GameObject obj)
    {
        obj.SetActive(true);
        SpriteRenderer[] renderers = obj.GetComponentsInChildren<SpriteRenderer>();

        // 초기 투명도 0 설정
        foreach (var sr in renderers)
        {
            Color c = sr.color;
            c.a = 0;
            sr.color = c;
        }

        float alpha = 0;
        // appearanceFadeSpeed 대신 고정값이나 변수를 사용하세요.
        float fadeSpeed = 1.5f;

        while (alpha < 1.0f)
        {
            alpha += Time.deltaTime * fadeSpeed;
            foreach (var sr in renderers)
            {
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
            yield return null;
        }
    }
}
