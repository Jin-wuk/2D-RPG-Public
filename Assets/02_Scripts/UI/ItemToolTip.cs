using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemToolTip : MonoBehaviour
{
    public static ItemToolTip Instance;

    [Header("UI 연결(패널 및 텍스트")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI nameText;
    //public TextMeshProUGUI descText;

    [Header("공통 정보")]
    public GameObject priceArea;
    public TextMeshProUGUI priceText;     // 판매가 표시

    [Header("장비 전용 정보")]
    public GameObject equipmentStatsArea;
    public TextMeshProUGUI statsText;      // 공격력, 방어력, 강화수치 표시

    [Header("기타 아이템 정보")]
    public GameObject etcArea;


    private RectTransform rectTransform;
    private Canvas parentCanvas; // 부모 캔버스 참조 추가

    private void Awake()
    {
        if (Instance == null) Instance = this;
        rectTransform = tooltipPanel.GetComponent<RectTransform>();

        // 툴팁이 속한 캔버스를 찾습니다.
        parentCanvas = GetComponentInParent<Canvas>();

        tooltipPanel.SetActive(false);
    }

    private void Update()
    {
        if (tooltipPanel.activeSelf)
        {
            FollowMouseSmart();
        }
    }
    private void FollowMouseSmart()
    {
        // 1. 마우스 위치 가져오기
        Vector3 mousePos = Input.mousePosition;

        // 2. 피벗을 (0, 1) 즉, 왼쪽 상단으로 고정 (코드에서 강제 설정)
        rectTransform.pivot = new Vector2(0f, 1f);

        // 3. 캔버스의 Scale을 고려한 실제 툴팁의 가로/세로 크기 계산
        // parentCanvas는 Awake에서 GetComponentInParent<Canvas>()로 가져온 상태여야 합니다.
        float scaledWidth = rectTransform.rect.width * parentCanvas.scaleFactor;
        float scaledHeight = rectTransform.rect.height * parentCanvas.scaleFactor;

        // 4. 마우스로부터 살짝 떨어뜨릴 오프셋(여백)
        float xOffset = 20f;
        float yOffset = -20f;

        float finalX = mousePos.x + xOffset;
        float finalY = mousePos.y + yOffset;

        // 5. [화면 오른쪽 제한] 툴팁이 오른쪽 벽을 뚫고 나간다면 마우스 왼쪽으로 이동
        if (finalX + scaledWidth > Screen.width)
        {
            finalX = mousePos.x - scaledWidth - xOffset;
        }

        // 6. [화면 아래쪽 제한] 툴팁이 바닥을 뚫고 나간다면 마우스 위쪽으로 이동
        if (finalY - scaledHeight < 0)
        {
            finalY = mousePos.y + scaledHeight - yOffset;
        }

        // 7. 최종 좌표를 'position'에 대입 (Overlay 캔버스에서는 이게 곧 스크린 좌표입니다)
        rectTransform.position = new Vector3(finalX, finalY, 0);
    }

    public void ShowTooltip(ItemData item)
    {
        if (item == null) return;

        tooltipPanel.SetActive(true);

        //공통 정보 입력
        nameText.text = item.name;
        //descText.text = itemdescription;

        priceArea.SetActive(true);
        priceText.text = $"{item.sellPrice} G";

        if (item.itemType == ItemType.Equipment)
        {
            //장비 아이템일때
            equipmentStatsArea.SetActive(true);
            etcArea.SetActive(false);
            priceArea.SetActive(true);

            statsText.text = $"Level: <color=#FFFF00>+{item.level}</color>\n" +
                             $"Atk: {item.atkBonus}\n" +
                             $"Def: {item.defBonus}";

        }
        else
        {
            //기타 아이템일때
            equipmentStatsArea.SetActive(false);
            etcArea.SetActive(true);

        }
        // 3. 레이아웃 강제 갱신 (Content Size Fitter가 즉시 반응하게 함)
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }
}
