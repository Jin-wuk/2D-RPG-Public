using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [Header("연결할 데이터")]
    [SerializeField] private PlayerStats playerStats;

    [Header("전투 UI")]
    [SerializeField] private TextMeshProUGUI atkText;
    [SerializeField] private TextMeshProUGUI defText;

    [Header("HP UI")]
    [SerializeField] private Image hpFillImage;
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("MP UI")]
    [SerializeField] private Image mpFillImage;
    [SerializeField] private TextMeshProUGUI mpText;

    [Header("Level UI")]
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("EXP UI")]
    [SerializeField] private Image expFillImage;
    [SerializeField] private TextMeshProUGUI expPercentText;

    [Header("골드 UI")]
    [SerializeField] private TextMeshProUGUI goldText;
    
    private Coroutine expCoroutine;
    private Coroutine hpCoroutine;
    private Coroutine mpCoroutine;

    private void Start()
    {
        // 1. 초기값 설정 (게임 시작 시 한 번 전체 갱신)
        RefreshAllUI();

        // 2. 각 이벤트에 맞는 전용 함수를 구독합니다.
        playerStats.OnHpChanged += UpdateHpUI;
        playerStats.OnMpChanged += UpdateMpUI;

        playerStats.OnAttackChanged += UpdateAtkUI;
        playerStats.OnDefensChanged += UpdateDefUI;

        playerStats.OnExpChanged += UpdateExpUI;
        playerStats.OnLevelChanged += UpdateLevelUI;

        playerStats.OnGoldChanged += UpdateGoldUI;
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제 (메모리 누수 방지)
        if (playerStats != null)
        {
            playerStats.OnHpChanged -= UpdateHpUI;
            playerStats.OnMpChanged -= UpdateMpUI;

            playerStats.OnAttackChanged -= UpdateAtkUI;
            playerStats.OnDefensChanged -= UpdateDefUI;

            playerStats.OnExpChanged -= UpdateExpUI;
            playerStats.OnLevelChanged += UpdateLevelUI;

            playerStats.OnGoldChanged -= UpdateGoldUI;
        }
    }
    // 처음 시작 시 전체를 한 번 그려주는 함수
    private void RefreshAllUI()
    {
        UpdateHpUI();
        UpdateMpUI();
        UpdateAtkUI();
        UpdateDefUI();

        UpdateExpUI();
        UpdateLevelUI();

        UpdateGoldUI();

    }
    // --- 개별 UI 업데이트 함수들 ---

    private void UpdateHpUI()
    {
        if (playerStats == null) return;

        // HP 부드러운 갱신
        if (hpFillImage != null)
        {
            float targetHpRatio = (float)playerStats.currentHp / playerStats.maxHp;
            if (hpCoroutine != null) StopCoroutine(hpCoroutine);
            hpCoroutine = StartCoroutine(AnimateHpBar(targetHpRatio));
        }

        // 체력 텍스트 갱신
        if (hpText != null)
        {
            hpText.text = $"{playerStats.currentHp} / {playerStats.maxHp}";
        }
    }
    private void UpdateMpUI()
    {
        if (playerStats == null) return;

        if (mpFillImage != null)
        {
            float targetMpRatio = (float)playerStats.currentMp / playerStats.maxMp;
            if (mpCoroutine != null) StopCoroutine(mpCoroutine);
            mpCoroutine = StartCoroutine(AnimateMpBar(targetMpRatio));
        }

        if (mpText != null)
        {
            mpText.text = $"{playerStats.currentMp} / {playerStats.maxMp}";
        }
    }

    private void UpdateExpUI()
    {
        if (playerStats == null) return;

        // EXP 부드러운 갱신
        if (expFillImage != null)
        {
            float targetExpRatio = (float)playerStats.currentExp / playerStats.expToNextLevel;
            if (expCoroutine != null) StopCoroutine(expCoroutine);
            expCoroutine = StartCoroutine(AnimateExpBar(targetExpRatio));
        }
    }

    private void UpdateGoldUI()
    {
        if (playerStats == null) return;

        // 골드 UI 갱신 (이제 HP가 깎여도 이 함수는 호출되지 않습니다)
        if (goldText != null)
        {
            goldText.text = playerStats.gold.ToString("N0");
        }
    }

    private void UpdateAtkUI()
    {
        if (playerStats == null || atkText == null) return;
        atkText.text = playerStats.totalAtk.ToString("N0");
    }

    private void UpdateDefUI()
    {
        if (playerStats == null || defText == null) return;
        defText.text = playerStats.totalDef.ToString("N0");
    }
    private void UpdateLevelUI()
    {
        if (playerStats == null || levelText == null) return;

        // "LV. 10" 또는 "10" 등 원하는 형식으로 출력합니다.
        levelText.text = $"LV. {playerStats.currentLevel}";
    }
    // --- 애니메이션 코루틴 (기존 로직 유지) ---

    private IEnumerator AnimateHpBar(float targetAmount)
    {
        float currentAmount = hpFillImage.fillAmount;
        float duration = 0.3f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            hpFillImage.fillAmount = Mathf.Lerp(currentAmount, targetAmount, elapsedTime / duration);
            yield return null;
        }

        hpFillImage.fillAmount = targetAmount;
        hpCoroutine = null;
    }

    private IEnumerator AnimateExpBar(float targetAmount)
    {
        float speed = 1.5f;
        float threshold = 0.001f;

        if (targetAmount < expFillImage.fillAmount)
        {
            while (1.0f - expFillImage.fillAmount > threshold)
            {
                expFillImage.fillAmount = Mathf.MoveTowards(expFillImage.fillAmount, 1f, Time.deltaTime * speed);
                UpdatePercentText(expFillImage.fillAmount);
                yield return null;
            }
            expFillImage.fillAmount = 0f;
        }

        while (Mathf.Abs(targetAmount - expFillImage.fillAmount) > threshold)
        {
            expFillImage.fillAmount = Mathf.MoveTowards(expFillImage.fillAmount, targetAmount, Time.deltaTime * speed);
            UpdatePercentText(expFillImage.fillAmount);
            yield return null;
        }

        expFillImage.fillAmount = targetAmount;
        UpdatePercentText(targetAmount);
        expCoroutine = null;
    }
    private IEnumerator AnimateMpBar(float targetAmount)
    {
        float currentAmount = mpFillImage.fillAmount;
        float duration = 0.2f; // 마나는 조금 더 빠르게 갱신되도록 설정
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            mpFillImage.fillAmount = Mathf.Lerp(currentAmount, targetAmount, elapsedTime / duration);
            yield return null;
        }

        mpFillImage.fillAmount = targetAmount;
        mpCoroutine = null;
    }
    private void UpdatePercentText(float ratio)
    {
        if (expPercentText != null)
            expPercentText.text = $"{(ratio * 100f):F2}%";
    }
}
