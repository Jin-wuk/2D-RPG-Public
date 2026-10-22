using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossJumpState : BossBaseState
{
    private float prepTime = 1.5f;      // 1. 점프 전 기 모으는 시간
    private float hangTime = 0.7f;      // 2. 공중 정점에서 멈춰있는 시간
    private float jumpForceUp = 10f;    // 상승 힘
    private float jumpForceForward = 10f; // 전진 힘
    private float slamForce = 30f;      // 3. 하강 시 내리꽂는 힘

    public bool isProcessStarted = false;

    private GameObject currentIndicator;

    public BossJumpState(BossController owner) : base(owner) { }

    public override void Enter()
    {
        isProcessStarted = false;

        // [추가] 레이지 모드 여부에 따른 수치 및 애니메이션 속도 조절
        if (owner.isRageMode)
        {
            // 광폭화: 2~3배 빠른 공격
            prepTime = 0.5f;     // 기 모으기 단축 (기존 1.5f)
            hangTime = 0.3f;     // 공중 체공 단축 (기존 0.7f)
            slamForce = 50f;     // 낙하 속도 증가 (기존 30f)
            owner.anim.speed = 1.8f; // 애니메이션 속도 자체를 올림
        }
        else
        {
            // 일반: 기본 수치
            prepTime = 1.5f;
            hangTime = 0.7f;
            slamForce = 30f;
            owner.anim.speed = 1.0f;
        }

        // 코루틴은 MonoBehaviour에서 실행해야 하므로 owner(Controller)를 통해 실행합니다.
        owner.StartCoroutine(JumpSequence());
    }

    IEnumerator JumpSequence()
    {
        isProcessStarted = true;

        // --- [단계 1: 점프 준비] ---
        // 준비 애니메이션을 재생하고 일정 시간 대기합니다.
        owner.anim.SetTrigger("isJump");
        owner.rb.velocity = Vector2.zero; // 제자리에서 기 모으기

        yield return new WaitForSeconds(prepTime);

        // --- [단계 2: 도약 및 상승] ---
        // 플레이어 방향으로 솟구칩니다.
        owner.anim.SetTrigger("isAir");

        float dir = (owner.player.position.x > owner.transform.position.x) ? 1f : -1f;
        owner.transform.localScale = new Vector3(dir * owner.baseScale, owner.baseScale, 1f);
        owner.rb.velocity = new Vector2(dir * jumpForceForward, jumpForceUp);

        // 정점(y속도가 0에 가까워질 때)까지 대기
        while (owner.rb.velocity.y > 0.1f)
        {
            yield return null;
        }

        // --- [단계 3: 공중 체공 (정점)] ---
        // 공중에서 잠시 멈춰 플레이어를 압박합니다.
        owner.rb.velocity = Vector2.zero;
        owner.rb.gravityScale = 0; // 중력을 잠시 꺼서 공중에 고정
        owner.rb.WakeUp();

        RaycastHit2D hitGround = Physics2D.Raycast(owner.transform.position, Vector2.down, 20f, owner.groundLayer);

        // [핵심 추가] 하강 직전 플레이어 위치에 박스형 예고 프리팹 생성
        Vector2 indicatorPos;
        if (hitGround.collider != null)
        {
            // 지면을 찾았다면 그 지점(Point)에 생성
            indicatorPos = hitGround.point;
        }
        else
        {
            // 지면을 못 찾았다면(허공 등) 안전하게 플레이어의 y축이나 특정 높이로 설정
            indicatorPos = new Vector2(owner.transform.position.x, owner.player.position.y);
        }

        // 2. 인디케이터 생성
        // [중요 수정] 인디케이터 생성 시 currentIndicator 변수에 할당합니다.
        if (owner.boxIndicatorPrefab != null)
        {
            // 지역 변수 indicator가 아닌 클래스 변수 currentIndicator를 사용하세요.
            currentIndicator = Object.Instantiate(owner.boxIndicatorPrefab, indicatorPos, Quaternion.identity);
            currentIndicator.transform.localScale = new Vector3(owner.stompBoxSize.x, owner.stompBoxSize.y, 1f);

            if (currentIndicator.TryGetComponent<AttackIndicator>(out var indicatorScript))
            {
                indicatorScript.SetIndicator(hangTime);
            }
        }

        yield return new WaitForSeconds(hangTime);

        // --- [단계 4: 강한 하강 (수직 낙하)] ---
        // [수정] 하강 직전 삭제할 때도 변수명 변경
        if (currentIndicator != null)
        {
            Object.Destroy(currentIndicator);
            currentIndicator = null;
        }

        // 중력을 다시 켜고 아래로 강하게 밀어냅니다.
        owner.anim.SetTrigger("isFalling");
        owner.rb.gravityScale = 3.0f; // 떨어질 때 더 묵직하게
        owner.rb.velocity = Vector2.down * slamForce; // 훨씬 즉각적이고 부드러움

    }

    public override void Update() { }
    public override void PhysicsUpdate() { }



    public override void OnGroundTouch()
    {
        // 1. 중복 실행 방지
        if (!isProcessStarted) return;
        isProcessStarted = false; // 착지 로직 시작됨을 표시

        // 2. 지면 강타 데미지 판정
        ExecuteStompAttack();

        // 3. 착지 애니메이션 대기 코루틴 실행
        owner.StartCoroutine(LandingSequence());
    }
    private void ExecuteStompAttack()
    {
        int playerLayer = LayerMask.GetMask("Player");
        // 1. Raycast로 지면의 정확한 위치 파악 (선택 사항이지만 정교한 판정을 위해 사용)
        // 발밑(groundCheck)에서 아래로 짧게 쏴서 실제 땅 위치를 찾습니다.
        RaycastHit2D hitGround = Physics2D.Raycast(owner.groundCheck.position, Vector2.down, 1f, owner.groundLayer);

        Vector2 spawnPos = (hitGround.collider != null) ? hitGround.point : (Vector2)owner.groundCheck.position;

        // 2. [핵심] 사각형 범위 판정 (OverlapBox)
        Collider2D hit = Physics2D.OverlapBox(spawnPos, owner.stompBoxSize, 0f, playerLayer);

        if (hit != null)
        {
            PlayerHealth pHealth = hit.GetComponent<PlayerHealth>();
            if (pHealth != null)
            {
                // 플레이어의 월드 X 좌표와 보스의 월드 X 좌표를 비교합니다.
                float directionX = (hit.transform.position.x < owner.transform.position.x) ? -1f : 1f;

                HitData stompData = new HitData();
                stompData.damage = owner.jumpStompDamage;
                stompData.hitPosition = owner.transform.position;
                stompData.useKnockback = true;

                // [핵심 수정] 넉백 힘에 방향을 곱해서 전달합니다.
                // 인스펙터의 jumpStompForceX가 12라면, 왼쪽일 때 -12, 오른쪽일 때 12가 됩니다.
                stompData.forceX = directionX * owner.jumpStompForceX;
                stompData.forceY = owner.jumpStompForceY;

                // 데이터 전달
                pHealth.ProcessHit(stompData);
            }
        }
    }
    IEnumerator LandingSequence()
    {
        // 착지 속도 초기화 (미끄러짐 방지)
        owner.rb.velocity = Vector2.zero;

        // [연출] 착지 애니메이션 트리거 (예: "isLanding")
        // 만약 "isFalling" 애니메이션에서 착지 동작으로 이어지는 구조라면 해당 트리거를 호출하세요.
        owner.anim.SetTrigger("isFalling");

        // 애니메이션 길이에 맞춰 대기 (예: 0.5초 정도, 애니메이션 실제 길이에 맞게 조절하세요)
        yield return new WaitForSeconds(0.5f);

        // 대기 후 대기 상태로 복귀
        owner.ChangeState(owner.idleState);
    }

    public override void Exit()
    {
        // 1. 가장 먼저 실행 중인 모든 코루틴 중단
        owner.StopAllCoroutines();

        // 2. 인디케이터 즉시 제거
        if (currentIndicator != null)
        {
            Object.Destroy(currentIndicator);
            currentIndicator = null;
        }

        // 3. 물리 상태 완전 초기화 (순서 중요)
        if (owner.rb != null)
        {
            owner.rb.gravityScale = 3.0f; // 중력 먼저 복구
            owner.rb.velocity = Vector2.zero; // 속도 초기화
            owner.rb.WakeUp(); // [핵심] 리지드바디를 강제로 깨워서 중력을 계산하게 만듦
        }

        // 4. 애니메이션 강제 초기화
        owner.anim.speed = 1.0f; // 다른 패턴에 영향 주지 않도록 속도 원복
        // Triggers뿐만 아니라 현재 재생 중인 애니메이션을 강제로 Idle로 돌리는 것도 방법입니다.
        owner.anim.Play("Boss_Idle"); // 애니메이션 이름이 Idle이라면 강제로 재생
        owner.anim.ResetTrigger("isJump");
        owner.anim.ResetTrigger("isAir");
        owner.anim.ResetTrigger("isFalling");
    }
}