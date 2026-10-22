using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class BossController : MonoBehaviour
{
    private BossBaseState currentState;

    // [추가] BossStats 참조 (에러 해결용)
    public BossStats stats { get; private set; }
    [HideInInspector] public SpriteRenderer spriteRenderer;

    public Transform groundCheck; // 발밑 위치 오브젝트
    public LayerMask groundLayer;   // 지면 레이어

    [Header("패턴 설정")]
    public Transform player;
    public Rigidbody2D rb;
    public Animator anim;

    public float baseScale = 1.0f;

    [Header("페이즈 설정 (레이지 모드)")]
    public bool isRageMode = false;
    public float rageThreshold = 0.5f; // 50% 이하 시 발동
    public Color rageColor = new Color(1f, 0.4f, 0.4f); // 변신 시 색상

    [Header("점프 공격 설정 (인스펙터 조절)")]
    public int jumpStompDamage = 50;
    public Vector2 stompBoxSize = new Vector2(5f, 2f); // 사각형 범위 (가로, 세로)
    public float jumpStompForceX = 12f;
    public float jumpStompForceY = 6f;

    [Header("레이저발사 지점 설정")]
    public Transform firePoint; // 보스의 가슴이나 중심에 배치한 빈 오브젝트

    [Header("하늘 레이저 범위 설정")]
    public Transform mapLeftX;  // 맵의 왼쪽 끝 X 좌표
    public Transform mapRightX; // 맵의 오른쪽 끝 X 좌표
    public Transform topLimitObj;    // [추가] 레이저가 시작될 천장 높이 오브젝트
    public float bottomY = -10f;     // 레이저가 닿을 바닥 높이 (숫자로 조절 가능)
    public float safeZoneWidth = 7f;

    [Header("공격 예고 설정")]
    // 이 부분이 추가되어야 BossJumpState에서 참조할 수 있습니다.
    public GameObject boxIndicatorPrefab;
    public GameObject laserPrefab;
    public GameObject explodeIndicatorPrefab;
    public GameObject trackingLaserPrefab;

    [Header("패턴 쿨타임 설정")]
    public float laserCooldown = 10f;    // 레이저 쿨타임
    public float skyFallCooldown = 20f; // 하늘 레이저 낙하 패턴 쿨타임
    public float explodeCooldown = 15f;
    public float laserStormCooldown = 25f;

    private float lastLaserTime = -999f;   // 마지막 레이저 사용 시점
    private float lastSkyFallTime = -999f; // [추가] 마지막 하늘 낙하 시점
    private float lastExplodeTime = -999f;
    private float lastLaserStormTime = -999f; // [추가]

    // 레이저 사용 가능 여부 확인
    public bool CanUseLaser() => Time.time >= lastLaserTime + laserCooldown;
    // 하늘 낙하 사용 가능 여부 확인 [추가]
    public bool CanUseSkyFall() => Time.time >= lastSkyFallTime + skyFallCooldown;
    public bool CanUseExplode() => Time.time >= lastExplodeTime + explodeCooldown;
    public bool CanUseLaserStorm() => isRageMode && Time.time >= lastLaserStormTime + laserStormCooldown;

    // 레이저 사용 시점 기록
    public void RecordLaserTime() => lastLaserTime = Time.time;
    public void RecordSkyFallTime() => lastSkyFallTime = Time.time; // [추가]
    public void RecordExplodeTime() => lastExplodeTime = Time.time;
    public void RecordLaserStormTime() => lastLaserStormTime = Time.time; // [추가]

    // 사용할 상태들
    public BossJumpState jumpState;
    public BossIdleState idleState;
    public BossLaserState laserState;
    public BossSkyFallState skyFallState;
    public BossExplodeState explodeState;
    public BossLaserStormState laserStormState;

    void Awake()
    {
        baseScale = Mathf.Abs(transform.localScale.x);
        stats = GetComponent<BossStats>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); //
        anim = GetComponent<Animator>();  //


        GameObject playerObj = GameObject.FindWithTag("Player");

        // 씬에 "LeftLimit", "RightLimit", "TopLimit"이라는 이름의 오브젝트가 있어야 합니다.
        GameObject left = GameObject.Find("Left");
        GameObject right = GameObject.Find("Right");
        GameObject top = GameObject.Find("Top");

        if (left != null) mapLeftX = left.transform;
        if (right != null) mapRightX = right.transform;
        if (top != null) topLimitObj = top.transform;

        // 만약 찾지 못했다면 에러 로그를 띄워 확인을 돕습니다.
        if (mapLeftX == null || mapRightX == null || topLimitObj == null)
        {
            Debug.LogError("보스가 맵 리밋 오브젝트를 찾지 못했습니다! 이름을 확인하세요.");
        }

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        // 상태 초기화
        idleState = new BossIdleState(this);
        jumpState = new BossJumpState(this);
        laserState = new BossLaserState(this);
        skyFallState = new BossSkyFallState(this);
        explodeState = new BossExplodeState(this);
        laserStormState = new BossLaserStormState(this);

        // 처음 시작 상태 설정
        ChangeState(idleState);
    }

    void Update()
    {
        currentState?.Update();
    }

    void FixedUpdate()
    {
        currentState?.PhysicsUpdate(); //
    }

    public void UpdateHp(float current, float max)
    {
        if (!isRageMode && (current / max) <= rageThreshold)
        {
            EnterRageMode();
        }
    }
    private void EnterRageMode()
    {
        isRageMode = true;

        // 상태 전환 전 보스의 움직임을 물리적으로 리셋
        rb.velocity = Vector2.zero;
        rb.gravityScale = 3.0f;
        rb.WakeUp(); // 여기서도 한 번 더 깨워줍니다.

        // BossStats의 originalColor를 붉은색으로 교체 (피격 후 복구 색상 변경)
        BossStats stats = GetComponent<BossStats>();
        if (stats != null)
        {
            stats.SetRageColor(rageColor);
        }

        // 쿨타임 감소
        laserCooldown *= 0.7f;
        skyFallCooldown *= 0.7f;
        explodeCooldown *= 0.7f;
        laserStormCooldown *= 0.7f; // [추가]

        // 즉시 원형 폭발 패턴 실행
        ChangeState(new BossExplodeState(this));
    }

    public void ChangeState(BossBaseState newState)
    {
        currentState?.Exit(); // 기존 상태(레이저 등) 정리 호출
        currentState = newState;

        // newState가 null이면(사망 시) 더 이상 Enter를 호출하지 않음
        if (currentState != null)
        {
            currentState.Enter();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 충돌한 오브젝트의 태그가 "Ground"인지 확인합니다
        if (collision.gameObject.CompareTag("Ground"))
        {
            // 현재 상태에게 땅에 닿았음을 알립니다
            currentState?.OnGroundTouch();
        }
    }
    // [추가] 다음 행동을 결정하는 함수
    public void ChooseNextPattern()
    {
        List<BossBaseState> availablePatterns = new List<BossBaseState>();
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // --- [1단계: 공통 패턴 (일반/레이지 모두 사용)] ---
        if (CanUseSkyFall()) availablePatterns.Add(skyFallState);
        if (CanUseLaser()) availablePatterns.Add(laserState);

        // --- [2단계: 레이지 모드 전용 패턴 추가] ---
        if (isRageMode)
        {
            if (CanUseLaserStorm()) availablePatterns.Add(laserStormState);
            if (CanUseExplode()) availablePatterns.Add(explodeState);
        }

        // --- [3단계: 거리 기반 내려찍기(Jump) 가중치 조절] ---
        if (distanceToPlayer < 5f)
        {
            // 가까우면 점프 확률 대폭 증가 (리스트에 3개 추가)
            availablePatterns.Add(jumpState);
            availablePatterns.Add(jumpState);
            availablePatterns.Add(jumpState);
        }
        else if (distanceToPlayer < 12f)
        {
            // 중간 거리면 점프 확률 보통
            availablePatterns.Add(jumpState);
        }
        else
        {
            // 너무 멀면 점프를 리스트에 넣지 않음 (원거리 레이저 패턴 유도)
        }

        // 안전장치: 리스트가 비어있다면 기본 점프 추가
        if (availablePatterns.Count == 0) availablePatterns.Add(jumpState);

        // --- [4단계: 랜덤 선택 및 실행] ---
        int randomIndex = Random.Range(0, availablePatterns.Count);
        BossBaseState selectedState = availablePatterns[randomIndex];

        // 쿨타임 기록
        if (selectedState == laserStormState) RecordLaserStormTime();
        else if (selectedState == explodeState) RecordExplodeTime();
        else if (selectedState == skyFallState) RecordSkyFallTime();
        else if (selectedState == laserState) RecordLaserTime();

        ChangeState(selectedState);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireCube(groundCheck.position, stompBoxSize);

    }
    private void OnDrawGizmos()
    {
        // 오브젝트가 할당되어 있을 때만 선을 그려줍니다.
        if (mapLeftX == null || mapRightX == null) return;

        Gizmos.color = Color.cyan;
        float leftX = mapLeftX.position.x;
        float rightX = mapRightX.position.x;

        Gizmos.DrawLine(new Vector3(leftX, 10, 0), new Vector3(leftX, -10, 0));
        Gizmos.DrawLine(new Vector3(rightX, 10, 0), new Vector3(rightX, -10, 0));
        Gizmos.DrawLine(new Vector3(leftX, 5, 0), new Vector3(rightX, 5, 0));
    }
}
