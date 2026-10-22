using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("이동 관련 변수")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float flashJumpForce = 8f; //앞으로 점프
    [SerializeField] private float upJumpForce = 16f;   //위 점프

    private Rigidbody2D rigid;
    private SpriteRenderer spriteRenderer;
    private Animator anim;

    [Header("점프 관련 변수")]
    private int jumpCount = 0;
    private int maxJumpCount = 2;
    private bool isGrounded;

    [Header("점프 이펙트")]
    [SerializeField] private GameObject upjumpEffectPrefab;

    [Header("땅 체크")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float checkRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer; // 절대 못뚫는 바닥
    [SerializeField] private LayerMask platformLayer;   //아래 점프 가능 바닥
    private LayerMask combinedLayer;   //두 레이어를 합친 것 (GroundCheck용)

    private CapsuleCollider2D playerCollider;

    public bool CanMove { get; set; } = true;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        playerCollider = GetComponent<CapsuleCollider2D>();

        combinedLayer = groundLayer | platformLayer;
    }

    // 좌우 이동 처리
    public void Move(float dir)
    {
        // 이동 불가 상태라면 속도를 0으로 만들고 리턴
        if (!CanMove)
        {
            anim.SetBool("isWalking", false);
            return;
        }

        // 땅에 있을 때는 평소처럼 이동
        if (isGrounded)
        {
            rigid.velocity = new Vector2(dir * moveSpeed, rigid.velocity.y);
        }
        else
        {
            // 공중일 때: 현재 속도가 이동 속도보다 빠르면(플래시 점프 중) 가속도를 유지함
            // 단, 입력 방향과 이동 방향이 반대라면 제동을 위해 속도 변경 허용
            if (Mathf.Abs(rigid.velocity.x) <= moveSpeed || Mathf.Sign(dir) != Mathf.Sign(rigid.velocity.x))
            {
                rigid.velocity = new Vector2(dir * moveSpeed, rigid.velocity.y);
            }
        }

        if (dir != 0)
        {
            spriteRenderer.flipX = dir < 0;
            anim.SetBool("isWalking", true);
        }
        else anim.SetBool("isWalking", false);
    }

    // 점프 처리
    public void Jump(float dir)
    {
        if (!CanMove) return; // 이동 불가면 점프도 불가

        if (isGrounded)
        {
            //첫번째 점프
            rigid.velocity = new Vector2(rigid.velocity.x, jumpForce);
            jumpCount = 1;
            isGrounded = false;
            //Debug.Log("첫번째 점프");
        }
        else if (jumpCount < maxJumpCount)
        {
            // [플래시 점프]
            float jumpDir = spriteRenderer.flipX ? -1f : 1f;

            // Y축 속도는 약간만 주어 체공 시간을 늘리고, X축으로 강하게 밀어냄
            rigid.velocity = new Vector2(jumpDir * flashJumpForce, jumpForce * 0.5f);

            jumpCount++;
            //if (anim != null) anim.SetTrigger("doDoubleJump");
            //Debug.Log("플래시 점프: 앞으로 대쉬!");
        }
    }

    // 위 점프 (수직으로 높게 상승)
    public void UpJump()
    {
        if (!CanMove) return; // 이동 불가면 점프도 불가

        //점프 사운드
        SoundManager.Instance.PlaySFX(SoundManager.SoundType.Jump);

        if (isGrounded)
        {
            rigid.velocity = new Vector2(rigid.velocity.x * 0.3f, upJumpForce);
            jumpCount = 1;

            SpawnUpJumpEffect();
        }
    }

    public void SpawnUpJumpEffect()
    {
        if (upjumpEffectPrefab == null) return;

        Vector3 spawnPos = transform.position + (Vector3.up * 0.3f);
        GameObject effect = Instantiate(upjumpEffectPrefab, spawnPos, Quaternion.identity);
        Destroy(effect, 0.5f); // 1초 뒤 삭제
    }

    //아래 점프
    public void DownJump()
    {
        if (!CanMove) return; // 이동 불가면 점프도 불가

        bool onPlatform = Physics2D.OverlapCircle(groundCheck.position, checkRadius, platformLayer);

        if (onPlatform)
        {
            StartCoroutine(DownJumpRoutine());
            //Debug.Log("발판 아래 점프 실행");
        }
        else
        {
            //Debug.Log("아래점프 불가!");
        }
    }

    private IEnumerator DownJumpRoutine()
    {
        // 1. 현재 발밑에 있는 발판의 콜라이더 정보를 가져옵니다.
        Collider2D platformCollider = Physics2D.OverlapCircle(groundCheck.position, checkRadius, platformLayer);

        if (platformCollider != null)
        {
            // 2. [핵심] 플레이어 콜라이더와 해당 발판 콜라이더 간의 충돌을 직접 무시합니다.
            Physics2D.IgnoreCollision(playerCollider, platformCollider, true);

            // 3. 물리 엔진을 깨우기 위해 아래로 힘을 줍니다.
            rigid.velocity = new Vector2(rigid.velocity.x, -5f); // 속도를 조금 더 높였습니다.

            // 4. 발판을 충분히 통과할 때까지 기다립니다.
            yield return new WaitForSeconds(0.4f);

            // 5. 충돌 무시를 다시 해제합니다.
            if (platformCollider != null)
            {
                Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
            }
        }
    }

    //지면 체크 (Update에서 지속 호출)
    public void GroundCheck()
    {
        // 지면 체크
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, combinedLayer);

        anim.SetBool("isJumping", !isGrounded);

        // [중요] 땅에 닿아 있고, 위로 올라가는 중이 아닐 때만 초기화
        // rigid.velocity.y > 0.1f 인 동안은 점프 중이므로 초기화하지 않음
        if (isGrounded && rigid.velocity.y <= 0.01f)
        {
            jumpCount = 0;
        }
    }
}
