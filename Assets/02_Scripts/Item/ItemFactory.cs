using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemFactory
{
    /// <summary>
    /// 원본 데이터를 바탕으로 고유한 아이템 인스턴스를 생성합니다.
    /// </summary>
    public static ItemData CreateItemInstance(ItemData original)
    {
        if (original == null) return null;

        // 1. 원본을 복제하여 새로운 메모리 주소를 가진 객체 생성
        ItemData instance = Object.Instantiate(original);

        // 2. 초기화 로직 (필요 시)
        // 예: 생성 시점에 랜덤하게 내구도를 정하거나, 고유 ID를 부여할 수 있음
        // instance.uniqueID = System.Guid.NewGuid().ToString();

        return instance;
    }
}
