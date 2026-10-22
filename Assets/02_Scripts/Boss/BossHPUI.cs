using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossHPUI : MonoBehaviour
{
    public static BossHPUI Instance;
    [SerializeField] private Slider hpSlider;

    // 현재 감시 중인 보스 스탯을 보관 (구독 해제용)
    private BossStats currentBossStats;

    void Awake()
    {
        if (Instance == null) Instance = this;
        gameObject.SetActive(false); // 처음엔 숨김
    }

    // 보스가 생성될 때 연결해주는 함수
    public void SetupBossHP(BossStats stats)
    {
        currentBossStats = stats; // 참조 보관
        gameObject.SetActive(true);

        hpSlider.maxValue = stats.maxHp;
        hpSlider.value = stats.currentHp;

        // 보스의 체력 변화 이벤트에 UI 갱신 함수를 구독시킵니다.
        stats.OnHpChanged += UpdateHPBar;
    }

    // [핵심] 보스가 죽었을 때 호출할 함수
    public void ClearUI()
    {
        if (currentBossStats != null)
        {
            // 중요: 더 이상 체력 변화를 감시하지 않도록 구독 해제
            currentBossStats.OnHpChanged -= UpdateHPBar;
            currentBossStats = null;
        }

        gameObject.SetActive(false); // UI 숨기기
    }

    private void UpdateHPBar(int currentHp, int maxHp)
    {
        hpSlider.value = currentHp;
    }
}
