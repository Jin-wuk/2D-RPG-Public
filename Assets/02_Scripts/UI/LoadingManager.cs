using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance;

    public bool IsLoading { get; private set; } = false;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;

    private void Awake()
    {
        Instance = this;

        canvasGroup.alpha = 0;
        canvasGroup.gameObject.SetActive(false);
    }

    public IEnumerator FadeAndAction(System.Action action)
    {
        IsLoading = true;
        canvasGroup.gameObject.SetActive(true);
        Debug.Log("1. 페이드 인 시작");
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0,1, timer/fadeDuration);
            yield return null;
        }
        Debug.Log("열림");

        try
        {
            if (action != null) action.Invoke();
            Debug.Log("3. 이동 로직 실행 성공");
        }
        catch (System.Exception e)
        {
            // 여기서 빨간 에러의 진짜 원인이 찍힐 것입니다.
            Debug.LogError($"이동 중 에러 발생! 원인: {e.Message}\n상세정보: {e.StackTrace}");
        }

        yield return new WaitForSeconds(0.5f);
        Debug.Log("4. 대기 시간 종료, 페이드 아웃 시작");
        // 화면 밝게
        timer = 0;
        while(timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1,0,timer / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0;
        canvasGroup.gameObject.SetActive(false);
        IsLoading = false;
        Debug.Log("닫힘");
    }
}
