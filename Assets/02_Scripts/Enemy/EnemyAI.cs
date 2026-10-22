using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("몬스터 이동 설정")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private Transform groundCheck; //몬스터 앞바닥 체크
    [SerializeField] private LayerMask groundLayer;

    [Header("몬스터 행동 주기 설정")]
    [SerializeField] private float minThinkTime = 1f;   //최소 대기/이동 시간
    [SerializeField] private float maxThinkTime = 4f;   //최대 ""

    //몬스터 공격 관련
    private bool isAttacking = false;
    private float lastAttackTime;
    [SerializeField] private float attackCooldown = 2f;

    private int walkDir = 1;    //오른쪽1 왼쪽 -1
    private bool isIdle = false;    //현재 멈춰있는지 여부
    private float thinkTimer;   //다음 행동까지 남은시간

    private Enemy enemy;    //Enemy 스크립트 참조
    private Rigidbody2D rigid;
    private Animator anim;

    private Vector3 baseScale;
    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        enemy = GetComponent<Enemy>();
        DecideNextAction();

        baseScale = transform.localScale;
    }

    private void Update()
    {
        if (enemy != null && enemy.IsDead) return;

        thinkTimer -= Time.deltaTime;

        if(thinkTimer <= 0 )
        {
            DecideNextAction();
        }

        if(anim != null )
        {
            anim.SetBool("isWalking", !isIdle && !isAttacking);
        }
    }
    private void FixedUpdate()
    {
        // 멈춘 상태라면 이동 로직으로
        if (isIdle)
        {
            rigid.velocity = new Vector2(0, rigid.velocity.y);
            return;
        }


        if ((enemy != null && enemy.IsDead) || isIdle || isAttacking)
        {
            rigid.velocity = new Vector2(0, rigid.velocity.y);
            return;
        }

        // 이동 로직 적용
        rigid.velocity = new Vector2(walkDir * moveSpeed, rigid.velocity.y);

        // [최적화] 이동 중일 때만 바닥 체크 실행
        if (Mathf.Abs(rigid.velocity.x) > 0.1f)
        {
            RaycastHit2D groundInfo = Physics2D.Raycast(groundCheck.position, Vector2.down, 0.5f, groundLayer);
            if (groundInfo.collider == null)
            {
                Turn();
                DecideNextAction();
            }
        }
    }

    public void TryAttack()
    {
        Debug.Log("공격 범위 내 플레이어 감지됨!");
        if ((enemy != null && enemy.IsDead) || isAttacking) return;

        if (Time.time >=lastAttackTime + attackCooldown)
        {
            Debug.Log("공격 애니메이션 실행 시도");
            isAttacking = true;
            rigid.velocity = Vector2.zero;

            if(anim != null)
            {

                anim.SetTrigger("isAttacking");

                //사운드 추가예정

            }
            lastAttackTime = Time.time;
        }
    }

    // 애니메이션 이벤트에서 호출할 공격 종료 함수
    public void EndAttack()
    {
        isAttacking = false;
    }

    private void Turn()
    {
        walkDir *= -1;

        // [중요] Sprite flip이 아닌 Scale을 뒤집어 자식 오브젝트(groundCheck)도 반전시킵니다.
        // 현재 walkDir이 1이면 (1,1,1), -1이면 (-1,1,1)이 되도록 설정
        transform.localScale = new Vector3(walkDir, 1, 1);
    }

    private void DecideNextAction()
    {
        isIdle = Random.Range(0, 100) < 40; //40퍼 확률로 멈춤

        if (!isIdle)
        {
            // 30% 확률로 가던 방향과 반대로 꺾음
            if (Random.Range(0, 100) < 30)
            {
                walkDir *= -1;
            }
            transform.localScale = new Vector3(walkDir, 1, 1);
        }

        // 이 행동을 얼마나 유지할지 결정
        thinkTimer = Random.Range(minThinkTime, maxThinkTime);
    }
}
