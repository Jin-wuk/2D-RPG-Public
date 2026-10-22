using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button respawnButton;

    [Header("부활 비용 UI (TMP)")]
    [SerializeField] private TMP_Text costText;          // 부활 비용 표시 텍스트
    [SerializeField] private TMP_Text currentGoldText;   // 현재 보유 골드 표시 텍스트

    public Transform townSpawnPoint;

    private int calculatedCost; // 계산된 부활 비용 저장용

    private void Awake()
    {
        if (Instance == null) Instance = this;
        gameOverPanel.SetActive(false);

        respawnButton.onClick.AddListener(OnRespawnButtonClicked);
    }

    public void Show()
    {
        gameOverPanel.SetActive(true);
        CalculatePenalty();
    }

    // [추가] 골드 페널티 계산 로직
    private void CalculatePenalty()
    {
        if (PlayerStats.Instance == null) return;

        int currentGold = PlayerStats.Instance.gold;
        // 10% 계산 (소수점 버림)
        calculatedCost = Mathf.FloorToInt(currentGold * 0.1f);

        // UI 업데이트
        if (currentGoldText != null)
            currentGoldText.text = $"보유 골드: {currentGold:N0} G";

        if (costText != null)
            costText.text = $"부활 비용: {calculatedCost:N0} G";

        // 골드가 부족해서 부활 못하는 경우는 없겠지만(10%이므로), 
        // 혹시 모르니 버튼 활성화 체크
        respawnButton.interactable = (currentGold >= calculatedCost);
    }

    private void OnRespawnButtonClicked()
    {
        // 1. 실제 골드 차감
        if (PlayerStats.Instance != null)
        {
            // 마이너스 값을 전달하여 골드 차감
            PlayerStats.Instance.UpdateGold(-calculatedCost);
        }

        // 시간 정상화: 로딩 코루틴이 동작하려면 시간이 흘러야 합니다.
        Time.timeScale = 1f;

        // 부활 시퀀스 코루틴 시작
        StartCoroutine(RespawnSequence());

        // 게임오버 패널 끄기
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    IEnumerator RespawnSequence()
    {
        yield return StartCoroutine(LoadingManager.Instance.FadeAndAction(() =>
        {
            // 이 내부 코드는 화면이 완전히 검은색일 때 실행됩니다.
            PerformResetAndTeleport();
        }));

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Animator anim = player.GetComponent<Animator>();
            // 애니메이터에 'isRespawn' 트리거를 미리 만들어두세요.
            anim.SetTrigger("isRespawn");

            // 애니메이션이 나오는 동안 잠시 대기 (예: 1초)
            yield return new WaitForSeconds(1.0f);

            // [단계 3] 애니메이션이 끝나면 조작 권한 부여
            player.GetComponent<PlayerController>().enabled = true;
        }

    }
    private void PerformResetAndTeleport()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            // 1. 물리 엔진 복구
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            rb.simulated = true;
            rb.velocity = Vector2.zero;

            // 2. 상태 초기화 및 사망 플래그 해제
            PlayerStats stats = player.GetComponent<PlayerStats>();
            stats.isDead = false;
            stats.RestoreHP(stats.maxHp);

            // 3. 조작은 아직 꺼둠 (애니메이션 연출을 위해)
            player.GetComponent<PlayerController>().enabled = false;

            // 4. 마을로 이동
            player.transform.position = townSpawnPoint.position;
        }

        // 5. 보스 및 맵 정리
        ResetBossElements();
        if (MapManager.Instance != null)
        {
            MapManager.Instance.ChangeMap("Map_Town");
        }
    }
    private void ResetBossElements()
    {
        GameObject boss = GameObject.FindWithTag("Boss");
        if (boss != null) Destroy(boss);

        BossAltar altar = FindObjectOfType<BossAltar>(true);
        if (altar != null)
        {
            altar.gameObject.SetActive(true);
            altar.transform.localScale = Vector3.one;
        }
    }
}
