using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemFloating : MonoBehaviour
{
    [Header("둥실거림 설정")]
    [SerializeField] private float amplitude = 0.15f; // 위아래 범위
    [SerializeField] private float frequency = 1.0f;  // 속도
    [SerializeField] private float floatingOffset = 0.2f; // 바닥에서 얼마나 띄울지

    [Header("착지 레이어 설정")]
    [SerializeField] private LayerMask groundLayer; // 검사할 레이어들을 선택할 수 있습니다.

    private Vector3 startPos;
    private bool isLanded = false; // 바닥에 착지했는지 여부
    private Rigidbody2D rigid;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 태그가 Ground이거나 Platform인 경우 모두 착지로 인정
        if (!isLanded && (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Platform")))
        {
            Landing();
        }
    }
    void Update()
    {
        if (!isLanded) return;

        // 착지 후 둥실거리는 로직
        Vector3 tempPos = startPos;
        tempPos.y += Mathf.Sin(Time.time * Mathf.PI * frequency) * amplitude;
        transform.position = tempPos;
    }

    private void Landing()
    {
        isLanded = true;

        // 중요: 현재 위치에서 floatingOffset만큼 살짝 띄운 곳을 기준점으로 잡습니다.
        startPos = transform.position + Vector3.up * floatingOffset;

        if (rigid != null)
        {
            rigid.velocity = Vector2.zero;
            rigid.isKinematic = true; // 물리 엔진 정지

            // 바닥 충돌용 콜라이더가 비활성화되어야 자연스럽게 둥둥 뜹니다.
            // (이전에 만든 2개의 콜라이더 중 비트리거 콜라이더를 끄는 로직)
            var colliders = GetComponents<Collider2D>();
            foreach (var col in colliders)
            {
                if (!col.isTrigger) col.enabled = false;
            }
        }
    }
}
