using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 1.0f;     // 위로 올라가는 속도
    public float fadeSpeed = 1.0f;     // 투명해지는 속도
    public float destroyTime = 2.0f;   // 오브젝트가 파괴될 시간

    private TextMeshProUGUI textMesh;
    private Color textColor;

    void Start()
    {
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        textColor = textMesh.color;

        // 일정 시간 뒤 자동 삭제 (안전장치)
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        // 1. 위로 이동
        transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);

        // 2. 서서히 투명해지기
        textColor.a -= fadeSpeed * Time.deltaTime;
        textMesh.color = textColor;

        // 완전히 투명해지면 삭제
        if (textColor.a <= 0)
        {
            Destroy(gameObject);
        }
    }
}