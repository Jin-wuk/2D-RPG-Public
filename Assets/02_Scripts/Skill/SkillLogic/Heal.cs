using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heal : MonoBehaviour
{
    private PlayerStats targetStats; // 전달받을 스탯 참조
    private int healAmount;
    private int totalTicks;
    [SerializeField] private float yHeight = 2f;

    [Header("회복 UI")]
    public GameObject healTextPrefab;

    public void Setup(PlayerStats stats, int _amount, int _ticks)
    {
        targetStats = stats;
        healAmount = _amount;
        totalTicks = _ticks;
        StartCoroutine(ProcessHeal());
    }

    private IEnumerator ProcessHeal()
    {
        for (int i = 0; i < totalTicks; i++)
        {
            if (targetStats != null) // 찾을 필요 없이 이미 가지고 있는 참조 사용
            {
                targetStats.UpdateHp(healAmount);
                SpawnHealText();
            }
            yield return new WaitForSeconds(0.5f);
        }
        Destroy(gameObject);
    }

    private void SpawnHealText()
    {
        if (healTextPrefab == null) return;

        // 플레이어 위치 근처에 생성
        Vector3 spawnPos = transform.position + Vector3.up * yHeight;

        GameObject popupObj = Instantiate(healTextPrefab, spawnPos, Quaternion.identity);
        DamagePopup popupScript = popupObj.GetComponent<DamagePopup>();

        if (popupScript != null)
        {
            // [수정된 부분] 회복량이므로 Color.green을 전달
            popupScript.Setup(healAmount, Color.green);
        }
    }

}

