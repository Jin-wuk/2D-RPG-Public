using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHpBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0);

    // [추가] 초기 스케일 값을 저장해둡니다.
    private Vector3 initialScale;

    private void Awake()
    {
        initialScale = transform.localScale;
    }

    // Update보다 늦게 실행되는 LateUpdate가 UI 고정에 더 정확합니다.
    private void LateUpdate()
    {
        if (target != null)
        {
            // 1. 위치 추적
            transform.position = target.position + offset;

            // 2. 회전 고정 (세상 기준 정면)
            transform.rotation = Quaternion.identity;

            // 3. [핵심] 부모의 반전(Flip) 상쇄
            // 부모의 scale.x가 마이너스라면 자식인 나도 마이너스를 곱해 1(정방향)로 만듭니다.
            Vector3 parentScale = target.localScale;
            transform.localScale = new Vector3(
                initialScale.x * (parentScale.x > 0 ? 1 : -1),
                initialScale.y,
                initialScale.z
            );
        }
    }

    public void UpdateHealthBar(float currentHp, float maxHp)
    {
        if (slider != null)
        {
            float ratio = currentHp / maxHp;
            slider.value = ratio;
        }
    }
}
