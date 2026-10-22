using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackIndicator : MonoBehaviour
{
    public Transform fillTransform;
    private float timer = 0f;
    private float duration = 1.0f; // 기본값

    // 보스 상태(State)에서 호출할 함수
    public void SetIndicator(float time)
    {
        duration = time; // 여기서 전달받은 시간이 이 공격의 예고 시간이 됩니다.
        timer = 0f;
        if (fillTransform != null)
            fillTransform.localScale = new Vector3(0, 0, 1f);
    }

    private void Update()
    {
        if (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);

            if (fillTransform != null)
            {
                // 1. 크기 확장 (기존 로직 유지)
                fillTransform.localScale = new Vector3(progress, progress, 1f);

                // 2. [색상 개선] 투명한 빨강 -> 진하고 불투명한 빨강
                SpriteRenderer sr = fillTransform.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    // 시작 색상: 매우 투명한 빨간색 (0.2f 정도의 Alpha)
                    Color startColor = new Color(1f, 0f, 0f, 0.2f);
                    // 목표 색상: 아주 진하고 선명한 빨간색 (1.0f Alpha)
                    Color endColor = new Color(1f, 0f, 0f, 0.4f);

                    sr.color = Color.Lerp(startColor, endColor, progress);
                }
            }
        }
    }
}
