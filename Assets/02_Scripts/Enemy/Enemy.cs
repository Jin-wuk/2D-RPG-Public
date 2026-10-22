using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("몬스터 스탯")]
    [SerializeField] private int maxHp = 10;
    private int currentHp;
    [SerializeField] private float knockbackForce = 3f;

    private bool isDead = false;
    public bool IsDead => isDead;

    [Header("공격 설정")]
    [SerializeField] private int enemyDamage = 10;        // 접촉 & 공격 통합 데미지
    [SerializeField] private Vector2 attackArea = new Vector2(1.2f, 1f);
    [SerializeField] private Vector2 attackOffset = new Vector2(0.5f, 0f);
    [SerializeField] private LayerMask playerLayer;

    private Rigidbody2D rigid;
    private SpriteRenderer spriteRenderer;
    private Animator anim;

    [Header("체력바 UI 설정")]
    [SerializeField] private EnemyHpBar healthBar;

    [Header("데미지 폰트 설정")]
    [SerializeField] private GameObject damagePopupPrefab;  //폰트 프리팹

    [Header("몬스터 공격 사운드")]
    [SerializeField] private AudioClip attackClip;

    private EnemySpawner mySpawner;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        currentHp = maxHp;
    }
    private void Start()
    {
        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHp, maxHp);
        }
    }

    public void TakeDamage(int damage, Vector2 playerPos)
    {
        if (isDead) return;

        currentHp -= damage;

        //몬스터 피격 사운드
        SoundManager.Instance.PlaySFX(SoundManager.SoundType.Hit);

        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHp, maxHp);
        }

        //데미지 팝업 생성 (몬스터 머리 위)
        Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
        GameObject popupObj = Instantiate(damagePopupPrefab, spawnPos,Quaternion.identity);

        DamagePopup popupScript = popupObj.GetComponent<DamagePopup>();
        if (popupScript != null)
        {
            // 일반 데미지는 흰색(Color.white)으로 설정
            // 만약 크리티컬 판정을 넣는다면 여기서 Color.red나 Color.yellow를 줄 수도 있습니다.
            popupScript.Setup(damage, Color.white);
        }
        if (currentHp <= 0)
        {
            Die();
            return;
        }

        //넉백 처리
        float direction = transform.position.x > playerPos.x ? 1f : -1f;
        rigid.velocity = Vector2.zero;
        rigid.AddForce(new Vector2(direction * knockbackForce, knockbackForce * 0.5f), ForceMode2D.Impulse);

        //피격 이펙트(색상 변경)
        StartCoroutine(HitEffectRoutine());
        if (anim != null)
            {
                anim.SetTrigger("isHurt");
            }
    }

    private IEnumerator HitEffectRoutine()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }

    //몬스터 공격
    public int GetEnemyDamage() => enemyDamage;

    // 애니메이션 이벤트
    public void OnAttackHit()
    {
        if (isDead) return;

        //몬스터 사운드
        if (attackClip != null)
        {
            SoundManager.Instance.PlayClip(attackClip);
        }

        // 몬스터가 보고 있는 방향에 맞춰 오프셋 적용
        float direction = transform.localScale.x;
        Vector2 finalOffset = new Vector2(attackOffset.x * direction, attackOffset.y);
        Vector2 attackPos = (Vector2)transform.position + finalOffset;

        // 공격 범위 내 플레이어 체크
        Collider2D hitPlayer = Physics2D.OverlapBox(attackPos, attackArea, 0, playerLayer);

        if (hitPlayer != null)
        {
            ApplyDamageToPlayer(hitPlayer.gameObject);
        }
    }
    // 2. [물리 충돌] 몸에 닿았을 때 호출 (몸빵)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            // 이 값은 두 물체가 얼마나 겹쳤든 상관없이 "어느 면이 부딪혔는지"를 정확히 알려줍니다.
            Vector2 contactNormal = collision.GetContact(0).normal;

            // 방향 정보를 포함하여 데미지 함수 호출
            ApplyDamageToPlayer(collision.gameObject, contactNormal);
        }
    }
    // 3. [공통] 플레이어에게 실제 데미지를 전달하는 통로
    // 3. [공통] 인자값에 contactNormal을 추가합니다.
    private void ApplyDamageToPlayer(GameObject playerObj, Vector2? contactNormal = null)
    {
        PlayerHealth playerHealth = playerObj.GetComponent<PlayerHealth>();
        if (playerHealth == null) return;

        float directionX;

        // 만약 물리 충돌 데이터(Normal)가 있다면 그 반대 방향으로 날려버립니다.
        if (contactNormal.HasValue)
        {
            // normal.x가 0보다 크면 플레이어의 오른쪽에서 몬스터가 때린 것이므로 -1(왼쪽)으로 날림
            // (유니티 Normal 벡터는 부딪힌 표면에서 밖으로 나가는 방향이기 때문입니다)
            directionX = contactNormal.Value.x > 0 ? 1f : -1f;
        }
        else
        {
            // 애니메이션 공격(Trigger) 등 Normal 정보가 없는 경우는 기존처럼 좌표 비교
            directionX = (playerObj.transform.position.x < transform.position.x) ? -1f : 1f;
        }

        HitData data = new HitData();
        data.damage = enemyDamage;
        data.hitPosition = transform.position;
        data.useKnockback = true;
        data.forceX = directionX * 3.5f; // 힘을 약간 키워 테스트
        data.forceY = 1.0f;

        playerHealth.ProcessHit(data);
    }

    // 소환기가 자신을 등록할 때 사용함
    public void SetSpawner(EnemySpawner spawner)
    {
        mySpawner = spawner;
    }
    public void OnDeathAnimationEnd()
    {
        // 애니메이션이 끝나면 리스폰을 시작하도록 할 수도 있습니다.
        // (만약 Die()에서 이미 호출했다면 여기서 중복 호출되지 않게 주의하세요)

        Destroy(gameObject);
    }
    private void Die()
    {
        if (isDead) return; // 중복 실행 방지


        SoundManager.Instance.PlaySFX(SoundManager.SoundType.EnemyDie);
        
        isDead = true;      // 죽는 순간 true로 변경

        // AI 스크립트 비활성화 (추가)
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null) ai.enabled = false;

        // 물리/충돌 기능 정지 (시체가 플레이어를 방해하지 않게 함)
        Collider2D[] cols = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in cols)
        {
            col.enabled = false;
        }

        rigid.velocity = Vector2.zero;
        rigid.isKinematic = true;

        if (anim != null)
        {
            anim.ResetTrigger("isHurt");

            anim.SetBool("isWalking", false);
            anim.SetTrigger("isDie");

            Destroy(gameObject, 1.0f);  //보험 : 이벤트 씹힐경우 2초뒤 강제 삭제
        }
        else
        {
            // 애니메이터가 없다면 즉시 삭제
            Destroy(gameObject);
        }

        // 보상 지급 (EnemyReward 호출 등)
        EnemyReward reward = GetComponent<EnemyReward>();
        if (reward != null) reward.GiveRewards();

        // 소환기에게 죽었다고 알림
        if (mySpawner != null)
        {
            mySpawner.StartRespawn();
        }
    }

    // 에디터 뷰에서 공격 범위를 파란색 박스로 표시
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        float direction = transform.localScale.x;
        Vector2 finalOffset = new Vector2(attackOffset.x * direction, attackOffset.y);
        Vector2 attackPos = (Vector2)transform.position + finalOffset;

        Gizmos.DrawWireCube(attackPos, attackArea);
    }
}

