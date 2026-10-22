using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLevelUpSystem : MonoBehaviour
{
    [Header("레벨업시 증가할 스탯")]
    [SerializeField] private int addMaxHp = 10;
    [SerializeField] private int addMaxMp = 50;
    [SerializeField] private int addAttackPower = 5;
    [SerializeField] private int addDefensePower = 2;

    [Header("레벨업 연출")]
    [SerializeField] private GameObject levelUpEffectPrefab;

    private PlayerStats stats;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    //경험치 획득 로직
    public void AddExp(int amount)
    {
        int exp = stats.currentExp + amount;
        int nextExp = stats.expToNextLevel;
        int level = stats.currentLevel;

        bool didLevelUp = false;

        //경험치 가득 찼다면 레벨업 반복체크(경험치 총량보다 많이 획득시 증발되는거 방지)
        while (exp >= nextExp)
        {
            exp -= nextExp;
            level++;

            //레벨업 보상 로직
            stats.UpdateAttackPower(addAttackPower);    //공격력 상승
            stats.UpdateDefensePower(addDefensePower);  //방어력 상승

            stats.UpdateMaxHp(addMaxHp);   //최대체력 상승
            stats.UpdateHp(stats.maxHp);    //레벨업시 풀회복

            stats.UpdateMaxMp(addMaxMp);    //최대마나 상승
            stats.UpdateMp(stats.maxMp);    //  레벨업 시 마나 풀회복

            nextExp = Mathf.RoundToInt(nextExp * 1.2f); //요구 경험치 상승

            didLevelUp = true;
        }

        if (didLevelUp)
        {
            // 레벨업 텍스트 프리팹 생성
            if (levelUpEffectPrefab != null)
            {
                // 플레이어 위치에서 약간 위(머리 위)에 생성하기 위해 오프셋(Vector3.up * 2f)을 더합니다.
                Vector3 spawnPosition = transform.position + Vector3.up * 2.0f;

                // 텍스트는 보통 정면을 바라봐야 하므로 Rotation은 기본값(identity)을 사용합니다.
                Instantiate(levelUpEffectPrefab, spawnPosition, Quaternion.identity);
            }
        }

        stats.UpdateLevel(level, exp, nextExp);
    }

}