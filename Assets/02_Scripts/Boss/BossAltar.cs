using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossAltar : MonoBehaviour
{
    [Header("소환 설정")]
    public GameObject bossPrefab;
    public Transform spawnPoint;
    [SerializeField] private GameObject portal;
    public string summonItemName = "Boss_Summon"; // 인벤토리의 아이템 이름
    public int requiredAmount = 1;

    [Header("UI 설정")]
    public GameObject summonPanel;   // 소환 확인 패널
    public Button summonButton;      // 실제 소환 버튼
    public TMP_Text countText;          // [보유/필요]를 표시할 UI Text (또는 TMP_Text)
    public Color normalColor = Color.white; // 아이템이 충분할 때 색상
    public Color lackingColor = Color.red;  // 아이템이 부족할 때 색상 (빨간색)

    private bool isPlayerNearby = false;
    private bool isBossSpawned = false;

    private Animator anim;



    private void Awake()
    {
        anim = GetComponent<Animator>();
        if (summonButton != null)
            summonButton.onClick.AddListener(OnSummonButtonClick);
    }
    private void Update()
    {
        // 1. 플레이어가 근처에 있고, 윗키(UpArrow 또는 W)를 누르면 패널 표시
        if (isPlayerNearby && !isBossSpawned)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                OpenSummonPanel();
            }
        }
    }
    // 소환 패널을 열 때 호출되는 함수
    private void OpenSummonPanel()
    {
        summonPanel.SetActive(true);
        RefreshSummonUI();
    }

    // 인벤토리 상태를 확인하여 UI 텍스트와 버튼 상태 갱신
    public void RefreshSummonUI()
    {
        if (InventoryManager.Instance == null) return;

        // InventoryManager에서 현재 보유량 가져오기
        int currentCount = InventoryManager.Instance.GetItemCount(summonItemName);

        // 텍스트 업데이트 (예: 0 / 1)
        if (countText != null)
        {
            countText.text = $"{currentCount} / {requiredAmount}";
            // 보유량이 필요량보다 적으면 빨간색, 같거나 많으면 흰색
            countText.color = (currentCount >= requiredAmount) ? normalColor : lackingColor;
        }

        // 아이템이 충분할 때만 버튼 활성화
        summonButton.interactable = (currentCount >= requiredAmount);
    }

    public void ResetAltar()
    {
        isBossSpawned = false;
        // 애니메이션 상태를 초기화하고 싶다면 Rebind 사용
        if (anim != null) anim.Rebind();
    }

    // 2. 플레이어 진입 감지
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = true;
            // 팁: 이때 "↑ 키를 눌러 상호작용" 같은 말풍선을 띄우면 더 좋습니다.
        }
    }

    // 3. 플레이어 이탈 감지
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision == null || collision.gameObject == null) return;

        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
            summonPanel.SetActive(false); // 멀어지면 패널도 닫기
        }
    }

    // 4. UI 버튼을 눌렀을 때 실행될 함수
    public void OnSummonButtonClick()
    {
        if (isBossSpawned) return;

        int currentCount = InventoryManager.Instance.GetItemCount(summonItemName);

        if (currentCount >= requiredAmount)
        {
            // 1. 인벤토리에서 아이템 실제 소모
            InventoryManager.Instance.RemoveItem(summonItemName, requiredAmount);

            // 2. 소환 시퀀스 시작
            isBossSpawned = true;
            summonPanel.SetActive(false);
            StartCoroutine(SummonSequence());
        }
        
    }

    System.Collections.IEnumerator SummonSequence()
    {

        // 제단 작동 애니메이션
        if (anim != null) anim.SetTrigger("isActivated");

        yield return new WaitForSeconds(1.5f);

        // 2. 보스 소환 및 데이터 인수인계
        if (bossPrefab != null && spawnPoint != null)
        {
            GameObject spawnedBoss = Instantiate(bossPrefab, spawnPoint.position, Quaternion.identity);

            // [핵심] 생성된 보스의 BossStats를 가져와서 포탈 정보를 넘겨줌
            BossStats stats = spawnedBoss.GetComponent<BossStats>();
            if (stats != null)
            {
                // [인수인계 1] 탈출 포탈 정보 전달
                stats.SetExitPortal(portal);

                // [인수인계 2] 나(제단)를 기억하라고 전달
                stats.SetAltar(this.gameObject);

                // [인수인계 3] 보스 체력바 UI 설정 (싱글톤 활용)
                if (BossHPUI.Instance != null)
                {
                    BossHPUI.Instance.SetupBossHP(stats);
                }
            }
        }

        if (portal != null)
        {
            // 그냥 툭 사라지게 하려면:
            portal.SetActive(false);

            // 만약 포탈도 서서히 사라지게 하고 싶다면 이전에 썼던 페이드 로직을 활용하세요!
        }
        // 제단 비활성화
        gameObject.SetActive(false);
    }

}
