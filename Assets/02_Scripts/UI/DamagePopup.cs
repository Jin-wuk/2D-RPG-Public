using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class DamagePopup : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    private float disappearTimer;
    private Color textColor;

    [Header("연출 설정")]
    [SerializeField] private float moveYSpeed = 2f;      // 위로 올라가는 속도
    [SerializeField] private float disappearTime = 0.7f;  // 투명해지기 시작하는 시간
    [SerializeField] private float fadeSpeed = 2f;        // 사라지는 속도

    private void Awake()
    {
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        textColor = textMesh.color;
    }

    public void Setup(int value, Color color)
    {
        textMesh.text = value.ToString();
        textMesh.color = color;
        disappearTimer = disappearTime;

        transform.position += new Vector3(Random.Range(-0.5f,0.5f), Random.Range(0, 0.5f), 0);

        StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        //위로 올라가는 연출
        float startY = transform.position.y;
        while (disappearTimer > 0)
        {
            transform.position += new Vector3(0, moveYSpeed) * Time.deltaTime;
            disappearTimer -= Time.deltaTime;
            yield return null;
        }

        //투명해지며 사라지는 연출
        while (textColor.a > 0)
        {
            textColor.a -= fadeSpeed * Time.deltaTime;
            textMesh.color = textColor;
            transform.position += new Vector3(0, moveYSpeed * 0.5f) * Time.deltaTime; // 사라질 땐 천천히 상승
            yield return null;
        }

        Destroy(gameObject);
    }
}

