using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("몬스터 스폰 설정")]
    [SerializeField] private GameObject enemyPrefab;    //소환할 몬스터
    [SerializeField] private float respawnDelay = 5f;   //리스폰 시간

    private GameObject currentEnemy;
    private float timer = 0f;
    private bool isRespawning = false;

    private void Start()
    {
        SpawnEnemy();
    }

    private void OnEnable()
    {
        if (currentEnemy == null && !isRespawning)
        {
            SpawnEnemy();
        }
    }

    private void Update()
    {
        // 1. 현재 몬스터가 살아있는지 체크
        if (currentEnemy == null && !isRespawning)
        {
            // 몬스터가 죽은 것을 감지한 순간
            isRespawning = true;
            timer = respawnDelay;
            Debug.Log($"{gameObject.name}: 몬스터 사망 감지, 리스폰 타이머 시작");
        }

        // 2. 리스폰 대기 중이라면 시간 감소
        if (isRespawning)
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                SpawnEnemy();
            }
        }
    }
    //몬스터가 죽을 때 이 함수를 호출할 것입니다.
    public void StartRespawn()
    {
        if (isRespawning) return;

        isRespawning = true;
        timer = respawnDelay; // 여기서부터 설정된 시간(5초 등)만큼 카운트다운 시작
        Debug.Log($"{gameObject.name}: 몬스터 사망 신호 수신. {respawnDelay}초 후 부활합니다.");
    }

    private void SpawnEnemy()
    {
        // 중복 생성 방지
        if (currentEnemy != null) return;

        isRespawning = false;
        currentEnemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity, transform);

        // 몬스터에게 스패너 정보를 넘겨줌 (필요한 경우)
        Enemy enemyScript = currentEnemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.SetSpawner(this);
        }

        Debug.Log($"{gameObject.name}: 몬스터 소환 완료");
    }

    //private IEnumerator RespawnRoutine()
    //{
    //    isRespawning = true;
    //    yield return new WaitForSeconds(respawnDelay);
    //    SpawnEnemy();
    //}
}
