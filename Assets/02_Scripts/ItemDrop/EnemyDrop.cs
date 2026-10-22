using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDrop : MonoBehaviour
{
    [Header("드롭 데이터 설정")]
    [SerializeField] private ItemDropTable dropTable;

    public void GiveRewards()
    {
        if (dropTable == null)
        {
            Debug.LogWarning($"{gameObject.name}에 Drop Table이 없습니다.");
            return;
        }

        foreach (DropItem item in dropTable.dropList)
        {
            // 확률 계산
            float randomValue = Random.Range(0f, 100f);

            if (randomValue <= item.dropRate)
            {
                SpawnItem(item.itemPrefab);
            }
        }
    }
        private void SpawnItem(GameObject prefab)
        {
        if (prefab == null) return;

        // 몬스터의 현재 위치에 아이템 생성
        GameObject droppedItem = Instantiate(prefab, transform.position, Quaternion.identity);

        // 아이템이 살짝 튀어오르는 물리 효과 (Rigidbody2D가 있는 경우)
        Rigidbody2D rigid = droppedItem.GetComponent<Rigidbody2D>();
        if (rigid != null)
        {
            // 위쪽으로 튀어오름
            Vector2 jumpForce = new Vector2(0f, 4f);
            rigid.AddForce(jumpForce, ForceMode2D.Impulse);
        }
    }
}
