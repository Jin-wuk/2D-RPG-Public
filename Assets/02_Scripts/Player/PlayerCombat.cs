using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("공격 변수 관련")]
    [SerializeField] private float attackCooldown = 0.35f;
    private float lastAttackTime;

    [Header("공격 범위 관련")]
    [SerializeField] private Vector2 attackRange = new Vector2(1.5f, 1f);   //공격범위
    [SerializeField] private Vector2 attackOffset = new Vector2(0.8f, 0f);  //캐릭터 중심에서의 거리
    [SerializeField] LayerMask enemyLayer;  //몬스터레이어
    [SerializeField] int maxHitCount = 3;   //한번에 때릴 수 있는 적 수

    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private PlayerStats stats;  //플레이어 스탯 참조

    private void Awake()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        stats = GetComponent<PlayerStats>();
    }

    public void Attack()
    {
        // 죽음 상태체크
        if (stats.isDead) return;

        //공격 쿨타입체크
        if (Time.time < lastAttackTime + attackCooldown) return;

        //공격 애니메이션 실행
        if (anim != null) anim.SetTrigger("isAttacking");

        //공격 사운드
        SoundManager.Instance.PlaySFX(SoundManager.SoundType.Attack);

        int finalDamage = stats.GetRandomDamage();
        

        //공격 방향 결정
        float direction = spriteRenderer.flipX ? -1f : 1f;
        Vector2 finalOffset = new Vector2(attackOffset.x * direction, attackOffset.y);
        Vector2 attackPos = (Vector2)transform.position + finalOffset;

        //공격 판정 실행
        Collider2D[] hitEnemy = Physics2D.OverlapBoxAll(attackPos, attackRange, 0, enemyLayer);

        //타격된 적들에게 데미지 전달
        for (int i = 0; i < hitEnemy.Length && i < maxHitCount; i++)
        {
            Enemy enemy = hitEnemy[i].GetComponent<Enemy>();
            if (enemy != null)
            {
                //적의 TakeDamage 함수 호출 (데미지와 플레이어 위치 전달)
                enemy.TakeDamage(finalDamage, transform.position);
            }

            // 보스 몬스터인지 확인 [추가]
            BossStats boss = hitEnemy[i].GetComponent<BossStats>();
            if (boss != null)
            {
                boss.TakeDamage(finalDamage);
                // 보스는 피격 애니메이션이 없다고 하셨으니, 여기서 별도의 연출(카메라 흔들림 등)을 
                // 추가하면 타격감이 더 살아납니다!
            }
        }


        lastAttackTime = Time.time;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        float direction = 1f;
        if (spriteRenderer != null) direction = spriteRenderer.flipX ? -1f : 1f;

        Vector2 finalOffset = new Vector2(attackOffset.x * direction, attackOffset.y);
        Vector2 attackPos = (Vector2)transform.position + finalOffset;

        Gizmos.DrawWireCube(attackPos, attackRange);
    }

}
