using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private int damage;
    private float range;
    private float speed;
    private int hitCount;

    private GameObject subPrefab; // 폭발 프리팹
    private float subMult;        // 폭발 배율
    private int subHits;          // 폭발 타수

    private Vector3 startPos;

    [Header("폭발 위치 설정")]
    [Tooltip("폭발이 생성될 위치의 오프셋입니다. Y값을 올리면 더 높은 곳에서 터집니다.")]
    [SerializeField] private Vector3 explosionOffset = new Vector3(0, 0.5f, 0); // 기본값 0.5f

    public void Setup(int _damage, float _range, float _speed, int _hits, GameObject _sub, float _subMult, int _subHits)
    {
        damage = _damage;
        range = _range;
        speed = _speed;
        hitCount = _hits;

        // 서브 데이터 저장
        subPrefab = _sub;
        subMult = _subMult;
        subHits = _subHits;

        startPos = transform.position;
    }

    private void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        // [버그 수정 2] 사거리를 벗어나면 폭발하고 사라지게 함
        if (Vector3.Distance(startPos, transform.position) >= range)
        {
            Explode();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null) enemy.TakeDamage(damage, transform.position);

            Explode();
        }
        else if (collision.CompareTag("Boss"))
        {
            BossStats boss = collision.GetComponent<BossStats>();
            // 만약 자식 오브젝트에 콜라이더가 있다면 GetComponentInParent 사용
            if (boss == null) boss = collision.GetComponentInParent<BossStats>();

            if (boss != null)
            {
                boss.TakeDamage(damage);
            }

            // 보스에 부딪혀도 폭발 효과는 나와야 하므로 호출
            Explode();
        }
    }

    private void Explode()
    {
        if (subPrefab != null)
        {
            Vector3 spawnPos = transform.position + explosionOffset;

            GameObject exp = Instantiate(subPrefab, spawnPos, Quaternion.identity);
            Explosion explosion = exp.GetComponent<Explosion>();

            if (explosion != null)
            {
                // Explosion의 새로운 Setup: (배율, 타수) 전달
                // 최종 데미지 = $TotalAtk \times subMult$
                explosion.Setup(subMult, subHits);
            }
        }

        // 투사체 자신을 파괴
        Destroy(gameObject);
    }
}
