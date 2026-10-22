using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 에디터 메뉴에서 파일을 생성할 수 있게 해줍니다.
[CreateAssetMenu(fileName = "NewDropTable", menuName = "ScriptableObjects/DropTable")]
public class ItemDropTable : ScriptableObject
{
    public List<DropItem> dropList; // 여러 개의 아이템 정보를 담는 리스트
}
