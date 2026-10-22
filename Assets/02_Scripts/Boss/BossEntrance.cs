using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class BossEntrance : MonoBehaviour
{
    private SpriteRenderer sr;
    private CinemachineImpulseSource impulseSource;
    private BossStats stats;

    [SerializeField] private float fadeDuration = 2.0f; // 서서히 나타나는 시간

    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
        stats = GetComponent<BossStats>();

        // 시작할 때는 투명하게 설정
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 0;
            sr.color = c;
        }
    }

    private void Start()
    {
        // 보스가 태어나자마자 연출 시작
        StartCoroutine(EntranceSequence());
    }

    IEnumerator EntranceSequence()
    {
        // 1. 화면 흔들기
        if (impulseSource != null) impulseSource.GenerateImpulse();

        // 2. 서서히 등장 (Fade-in)
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Clamp01(elapsed / fadeDuration);
                sr.color = c;
            }
            yield return null;
        }

        // 3. 모습이 다 보이면 체력 바 띄우기
        if (BossHPUI.Instance != null && stats != null)
        {
            BossHPUI.Instance.SetupBossHP(stats);
        }
    }
}
