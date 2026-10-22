using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("체력")]
    public int maxHp = 100;
    public int currentHp { get; private set; }

    [Header("마나")]
    public int maxMp = 50;
    public int currentMp { get; private set; }

    [Header("레벨업 및 경험치")]
    public int currentLevel = 1;
    public int currentExp = 0;
    public int expToNextLevel = 10;

    [Header("재화")]
    public int gold = 0;

    [Header("기본 전투 스탯")]
    public int attackPower = 1;
    public int defensePower = 0;

    [Header("전투 (장비 보너스)")]
    [SerializeField] private int equipmentAtk; // [수정] 인스펙터에서 보려고 SerializeField 추가
    [SerializeField] private int equipmentDef;

    [Header("전투 (실시간 전체 스탯 - 확인용)")]
    public int totalAtk;
    public int totalDef;

    public int TotalAtk => attackPower + equipmentAtk; // 이제 인스펙터에서 보입니다!
    public int TotalDef => defensePower + equipmentDef;

    [Header("전투 밸런스")]
    [Range(0f, 1f)] public float damageMastery = 0.8f;  //최소 공격력

    [Header("사망 상태")]
    public bool isDead = false;
    public System.Action OnDeath;

    // 데이터 변경 시 UI에 알리기 위한 이벤트
    public System.Action OnHpChanged;
    public System.Action OnMpChanged;

    public System.Action OnAttackChanged;
    public System.Action OnDefensChanged;

    public System.Action OnGoldChanged;

    public System.Action OnExpChanged;
    public System.Action OnLevelChanged;

    //인스펙터 최신화 함수
    private void RefreshTotalStats()
    {
        totalAtk = TotalAtk;
        totalDef = TotalDef;
    }
    private void Awake()
    {
        // [추가] 싱글톤 할당
        if (Instance == null) Instance = this;

        currentHp = maxHp;
        currentMp = maxMp;

        RefreshTotalStats(); // 시작 시 초기값 계산
    }

    //최대체력 증가
    public void UpdateMaxHp(int amount)
    {
        maxHp += amount;
        OnHpChanged?.Invoke();
    }
    public void UpdateHp(int amount)
    {
        if (isDead) return;

        currentHp = Mathf.Clamp(currentHp + amount, 0, maxHp);
        OnHpChanged?.Invoke();

        if (currentHp <= 0)
        {
            isDead = true;
            OnDeath?.Invoke();
            GetComponent<PlayerHealth>()?.SendMessage("Die", SendMessageOptions.DontRequireReceiver);
            Debug.Log("플레이어 사망");
        }
    }

    public void UpdateMaxMp(int amount)
    {
        maxMp += amount;
        OnMpChanged?.Invoke();
    }

    public void UpdateMp(int amount)
    {
        // 0부터 maxMp 사이로 제한하여 안전하게 업데이트
        currentMp = Mathf.Clamp(currentMp + amount, 0, maxMp);
        OnMpChanged?.Invoke(); // 현재치가 변했으므로 UI 갱신
    }

    public void UpdateLevel(int level, int exp, int nextExp)
    {

        // 레벨이 실제로 변했을 때만 레벨 이벤트를 실행하도록 조건 추가
        if (currentLevel != level)
        {
            currentLevel = level;
            OnLevelChanged?.Invoke(); // [추가] 레벨 UI를 위해 실행
        }

        currentExp = exp;
        expToNextLevel = nextExp;
        OnExpChanged?.Invoke(); // 경험치 바 UI를 위해 실행
    }

    public void UpdateAttackPower(int amount)
    {
        attackPower += amount;
        RefreshTotalStats(); // 값이 변할 때마다 인스펙터용 변수 갱신
        OnAttackChanged?.Invoke();
    }

    public void UpdateDefensePower(int amount)
    {
        defensePower += amount;
        RefreshTotalStats(); // 값이 변할 때마다 인스펙터용 변수 갱신
        OnDefensChanged?.Invoke();
    }

    public void UpdateGold(int amount)
    {
        gold += amount;
        OnGoldChanged?.Invoke(); // 골드 이벤트만 발생
    }
    public int GetRandomDamage()
    {
        int currentAtk = totalAtk;

        // 최소 공격력 = 현재 공격력 * 숙련도
        int minAtk = Mathf.RoundToInt(totalAtk * damageMastery);
        // 최대 공격력 = 현재 공격력
        int maxAtk = Mathf.RoundToInt(totalAtk);

        return UnityEngine.Random.Range(minAtk, maxAtk + 1);
    }

    public void SetEquipmentBonus(int atk, int def)
    {
        equipmentAtk = atk;
        equipmentDef = def;

        RefreshTotalStats(); // 장비 보너스 변경 시 인스펙터용 변수 갱신
        // 값이 변했으니 UI 업데이트 이벤트 호출
        OnAttackChanged?.Invoke();
        OnDefensChanged?.Invoke();

    }

    //상점에서 구매버튼 눌렀을때 호출할 함수
    public bool SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            OnGoldChanged?.Invoke();
            return true;
        }
        Debug.Log("구매 실패");
        return false;
    }

    public int TakeDamage(int enemyAtk)
    {
        // 받는 피해량 = 적 공격력 - (기본 + 장비 방어스탯)
        int finalDamage = enemyAtk - (defensePower + equipmentDef);

        //최소데미지 1 보장
        if(finalDamage < 1) finalDamage = 1;

        UpdateHp(-finalDamage);

        return finalDamage;
    }

    // HP 회복 함수
    public void RestoreHP(int amount)
    {
        UpdateHp(amount);
        Debug.Log($"HP {amount} 회복! 현재 HP: {currentHp}");
    }

    // MP 회복 함수
    public void RestoreMP(int amount)
    {
        UpdateMp(amount);
        Debug.Log($"MP {amount} 회복! 현재 MP: {currentMp}");
    }
}
