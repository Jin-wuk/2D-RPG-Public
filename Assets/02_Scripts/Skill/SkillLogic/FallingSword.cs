using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingSword : MonoBehaviour
{
    private int damage; // 칼 데미지
    private int maxHits;    // 칼 타수
    private GameObject explosionPrefab;

    // [추가] 폭발 전용 데이터 저장용 변수
    private float explosionMult;
    private int explosionHits;

    private Collider2D targetCollider;

    [Header("폭발 위치 설정")]
    [Tooltip("폭발이 생성될 위치의 오프셋입니다. Y값을 올리면 더 높은 곳에서 터집니다.")]
    [SerializeField] private Vector3 explosionOffset = new Vector3(0, 0.5f, 0); // 기본값 0.5f

    private float interval = 0.2f;
    private bool isProcessing = false;

    private CapsuleCollider2D capsuleCollider;
    private Rigidbody2D rb;
    private Animator anim;

    private HashSet<int> enemiesHitWhileFalling = new HashSet<int>(); // 공중 타격 중복 방지용

    void Awake()
    {
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    public void Setup(int dmg, int hits, GameObject explosion, float subMult, int subHits, Collider2D target)
    {
        damage = dmg;
        maxHits = hits;
        explosionPrefab = explosion;

        // 폭발 정보 저장
        explosionMult = subMult;
        explosionHits = subHits;

        targetCollider = target;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isProcessing) return;

        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                int enemyID = enemy.GetInstanceID();

                // 이번 낙하에서 아직 맞지 않은 적만 타격
                if (!enemiesHitWhileFalling.Contains(enemyID))
                {
                    enemy.TakeDamage(damage, transform.position);
                    enemiesHitWhileFalling.Add(enemyID); // 중복 타격 방지 목록에 추가
                }
            }

        }
        // [추가] 보스 몬스터 체크
        else if (collision.CompareTag("Boss"))
        {
            BossStats boss = collision.GetComponent<BossStats>();
            if (boss == null) boss = collision.GetComponentInParent<BossStats>();

            if (boss != null)
            {
                int bossID = boss.GetInstanceID();
                if (!enemiesHitWhileFalling.Contains(bossID))
                {
                    boss.TakeDamage(damage); // 보스 데미지 전달
                    enemiesHitWhileFalling.Add(bossID);
                }
            }
        }

        int layerMask = LayerMask.GetMask("Ground", "Platform");

        if ((layerMask & (1 << collision.gameObject.layer)) != 0)

        {
                // 2. 그 지형이 내가 가야 할 목표(targetCollider)가 맞는지 확인
            if (collision == targetCollider)
            {
                isProcessing = true;
                enemiesHitWhileFalling.Clear();
                if (anim != null)
                {
                    anim.SetTrigger("Down");
                }

                StartCoroutine(ProcessSwordLogic());
                }
            } 
    }

    private IEnumerator ProcessSwordLogic()
    {
        // 1. 칼을 그 자리에 고정 (물리 정지)
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;

        // 2. 다단 히트 실행 (사용자님이 작성하신 로직 활용)
        Coroutine multiHit = StartCoroutine(ProcessMultiHit());

        // 3. 폭발 타이밍 조절 (예: 0.1초만 기다리고 바로 터뜨림)
        float explosionDelay = 0.5f;
        yield return new WaitForSeconds(explosionDelay);

        // [수정] 이제 skillLevel 체크 대신 프리팹이 있는지로 판단 (구조적 유연함)
        if (explosionPrefab != null)
        {
            SpawnExplosion();
        }

        yield return multiHit;
        yield return new WaitForSeconds(0.2f);
        Destroy(gameObject);
    }

    private IEnumerator ProcessMultiHit()
    {
        for (int i = 0; i < maxHits; i++)
        {
            // 실제 월드 사이즈 계산
            Vector2 worldSize = new Vector2(
                capsuleCollider.size.x * transform.lossyScale.x,
                capsuleCollider.size.y * transform.lossyScale.y
            );

            // 범위 내 적 탐색
            Collider2D[] targets = Physics2D.OverlapCapsuleAll(
                transform.position,
                worldSize,
                capsuleCollider.direction,
                transform.eulerAngles.z,
                LayerMask.GetMask("Enemy","Boss")
            );

            foreach (Collider2D target in targets)
            {
                Enemy enemy = target.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage, transform.position);
                }
                else
                {
                    BossStats boss = target.GetComponent<BossStats>();
                    if (boss == null) boss = target.GetComponentInParent<BossStats>();
                    if (boss != null) boss.TakeDamage(damage);
                }
            }

            if (i < maxHits - 1)
                yield return new WaitForSeconds(interval);
        }
    }
    private void SpawnExplosion()
    {
        Vector3 spawnPos = transform.position + explosionOffset;

        GameObject go = Instantiate(explosionPrefab, spawnPos, Quaternion.identity);
        Explosion explosion = go.GetComponent<Explosion>();

        if (explosion != null)
        {
            // 인스펙터에서 설정한 배율(explosionMult)과 타수(explosionHits)를 전달합니다.
            explosion.Setup(explosionMult, explosionHits);
        }
    }
}
