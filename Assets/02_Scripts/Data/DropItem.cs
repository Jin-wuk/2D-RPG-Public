using UnityEngine;

// [System.Serializable]이 있어야 인스펙터 창에서 리스트 형태로 보입니다.
[System.Serializable]
public class DropItem
{
    public string itemName;      // 아이템 이름
    public GameObject itemPrefab; // 실제 소환될 아이템 프리팹
    [Range(0f, 100f)]
    public float dropRate;       // 드롭 확률 (0~100)
}