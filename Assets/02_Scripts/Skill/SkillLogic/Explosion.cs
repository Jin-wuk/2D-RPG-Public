using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 이 스크립트를 사용하려면 CircleCollider2D가 반드시 필요하도록 강제합니다.
[RequireComponent(typeof(CapsuleCollider2D))]
public class Explosion : MonoBehaviour
{
    private CapsuleCollider2D capsuleCollider;

    [Header("위치 설정")]
    [SerializeField] private Vector2 detectionOffset; // [추가] 인스펙터에서 조절할 오프셋

    private int damage;
    private int maxHits;      // 총 타수 (SkillData에서 가져옴)
    [SerializeField] private float interval = 0.15f; // 타격 간격 (0.15초마다 한 번씩)

    [Header("파괴 설정 (Destroy Settings)")]
    [Tooltip("모든 타격이 끝난 후, 실제 오브젝트가 파괴되기까지의 추가 대기 시간입니다.")]
    [SerializeField] private float destroyDelay = 0.5f;

    private void Awake()
    {
        // 1. 컴포넌트가 시작될 때 CircleCollider2D를 가져옵니다.
        capsuleCollider = GetComponent<CapsuleCollider2D>();

        // 트리거 체크를 깜빡하더라도 코드에서 자동으로 설정해줍니다.
        capsuleCollider.isTrigger = true;
    }

    // Projectile이 폭발을 생성한 직후 이 함수를 호출해 데이터를 넘겨줍니다.
    public void Setup(float _multiplier, int _hits)
    {
        // 실시간 플레이어 공격력에 배율을 곱해 데미지 확정
        // $Damage = TotalAtk \times Multiplier$
        if (PlayerStats.Instance != null)
        {
            damage = Mathf.RoundToInt(PlayerStats.Instance.TotalAtk * _multiplier);
        }
        else
        {
            // 예외 상황 방지용 기본값 (플레이어 스탯을 찾을 수 없을 때)
            damage = 1;
        }

        maxHits = _hits;

        StartCoroutine(ProcessMultiHit());
    }

    private IEnumerator ProcessMultiHit()
    {
        for (int i = 0; i < maxHits; i++)
        {
            // 1. 캡슐의 월드 스케일을 반영한 실제 사이즈 계산
            Vector2 worldSize = new Vector2(
                capsuleCollider.size.x * transform.lossyScale.x,
                capsuleCollider.size.y * transform.lossyScale.y
            );

            // 2. [핵심] 로컬 오프셋을 월드 좌표로 변환
            // transform.TransformPoint를 쓰면 오브젝트가 회전하거나 스케일이 변해도 오프셋이 정확히 따라옵니다.
            Vector2 targetPos = transform.TransformPoint(detectionOffset);

            // (보스 오브젝트의 레이어 이름이 "Boss"라고 가정합니다. 만약 "Enemy"와 같다면 그대로 두셔도 됩니다.)
            int searchLayer = LayerMask.GetMask("Enemy", "Boss");

            // 2. 오버랩 캡슐로 범위 내 적 탐색
            // transform.eulerAngles.z를 넣어 오브젝트의 회전값도 반영합니다.
            Collider2D[] targets = Physics2D.OverlapCapsuleAll(
                targetPos,
                worldSize,
                capsuleCollider.direction,
                transform.eulerAngles.z,
                searchLayer
            );

            foreach (Collider2D target in targets)
            {
                if (target == null) continue;

                if (target.transform.root == transform.root)
                    continue;

                // [추가 방어] 태그가 Player인 경우도 한 번 더 걸러줍니다.
                if (target.CompareTag("Player"))
                    continue;

                // 타겟이 나를 소환한 플레이어 본체라면 무조건 패스!
                if (transform.parent != null && target.gameObject == transform.parent.gameObject)
                    continue;

                Enemy enemy = target.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage, transform.position);
                }
                else
                {
                    // 일반 몬스터가 아니라면 보스인지 확인
                    BossStats boss = target.GetComponent<BossStats>();
                    if (boss == null) boss = target.GetComponentInParent<BossStats>(); // 부모에 있을 경우 대비

                    if (boss != null)
                    {
                        boss.TakeDamage(damage);
                    }
                }
            }

            if (i < maxHits - 1)
                yield return new WaitForSeconds(interval);
        }
        yield return new WaitForSeconds(destroyDelay);

        Destroy(gameObject);
        
    }
}
