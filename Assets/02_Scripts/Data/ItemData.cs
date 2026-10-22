using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ItemType { Gold, Equipment, Etc, Consumable }   // 아이템 종류 구분
public enum EquipType { None, Helmet, Top, Bottom, Weapon } //장비 부위 구분
public enum ConsumableType { Health, Mana } // 포션 종류 구분

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]

public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon; //인벤토리에 보일 이미지
    public ItemType itemType;   //아이템 대분류
    public EquipType equipType; //무기, 투구, 상의, 하의 구분
    public GameObject itemPrefab;   //필드에 생성될 실제 모델

    [Header("장비 스텟")]
    public int atkBonus;
    public int defBonus;

    [Header("경제 설정")]
    [Tooltip("0이나 -1이면 상점에서 판매하지 않는 아이템입니다. ")]
    public int buyPrice;    //상점 구매가
    public int sellPrice;   //상점 판매가

    [Header("강화 설정")]
    public int level = 0;   //현재 강화 단계
    public int atkUpgrade = 2;  //강화시 증가할 공격력 수치
    public int defUpgrade = 1;  //강화시 증갈할 방어력 수치
    public int upgradePrice = 100;  //강화 비용

    [Header("소모품 설정")]
    public ConsumableType consumableType;
    public int restoreAmount;   //회복량
    public int maxStack = 99;   //최대 중첩 개수
}
