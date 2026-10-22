using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartScreenAI : MonoBehaviour
{
    public enum State { Idle, Walk, Attack }

    [Header("이동 범위")]
    public float minX = -8f;
    public float maxX = 8f;

    [Header("설정")]
    public float moveSpeed = 2f;
    public float stateChangeWait = 2f; // 상태 결정 사이의 대기 시간

    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private float fixedY;

    void Awake()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        fixedY = transform.position.y;
    }

    void Start()
    {
        StartCoroutine(RandomActionRoutine());
    }

    IEnumerator RandomActionRoutine()
    {
        while (true)
        {
            // 0: 대기, 1: 이동, 2: 공격 중 무작위 선택
            State newState = (State)Random.Range(0, 3);

            switch (newState)
            {
                case State.Idle:
                    yield return StartCoroutine(DoIdle());
                    break;
                case State.Walk:
                    yield return StartCoroutine(DoWalk());
                    break;
                case State.Attack:
                    yield return StartCoroutine(DoAttack());
                    break;
            }

            // 각 행동 사이의 짧은 휴식
            yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
        }
    }

    IEnumerator DoIdle()
    {
        anim.SetBool("isWalking", false);
        yield return new WaitForSeconds(Random.Range(1f, 3f));
    }

    IEnumerator DoWalk()
    {
        float targetX = Random.Range(minX, maxX);
        anim.SetBool("isWalking", true);

        // 방향 전환
        spriteRenderer.flipX = (targetX < transform.position.x);

        while (Mathf.Abs(transform.position.x - targetX) > 0.01f)
        {
            float step = moveSpeed * Time.deltaTime;
            transform.position = new Vector3(Mathf.MoveTowards(transform.position.x, targetX, step), fixedY, 0);
            yield return null;
        }

        anim.SetBool("isWalking", false);
    }

    IEnumerator DoAttack()
    {
        anim.SetBool("isWalking", false);
        // 공격 애니메이션 실행
        anim.SetTrigger("isAttacking");

        // 공격 애니메이션 길이에 맞춰 대기 (예: 1초)
        // 실제 애니메이션 길이에 따라 조정하세요.
        yield return new WaitForSeconds(1.0f);
    }

    private void OnDrawGizmosSelected()
    {
        // 1. 기즈모 색상 설정 (원하는 색으로 변경 가능)
        Gizmos.color = Color.cyan;

        // 2. Y값 결정 (실행 중이면 고정된 fixedY를, 에디터 모드면 현재 높이를 사용)
        float yPos = Application.isPlaying ? fixedY : transform.position.y;

        // 3. 시작점과 끝점 좌표 설정
        Vector3 startLine = new Vector3(minX, yPos, 0);
        Vector3 endLine = new Vector3(maxX, yPos, 0);

        // 4. 가로선 그리기
        Gizmos.DrawLine(startLine, endLine);

        // 5. 양 끝에 작은 수직선을 그려 '범위'임을 명확히 표시 (선택 사항)
        float markerHeight = 0.5f;
        Gizmos.DrawLine(startLine + Vector3.up * markerHeight, startLine + Vector3.down * markerHeight);
        Gizmos.DrawLine(endLine + Vector3.up * markerHeight, endLine + Vector3.down * markerHeight);
    }
}