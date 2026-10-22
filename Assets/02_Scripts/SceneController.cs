using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리를 위한 네임스페이스


public class SceneController : MonoBehaviour
{
    public void StartGame()
    {
        // 1. LoadingManager의 페이드 코루틴을 시작합니다.
        // 2. 람다식() => { } 을 사용하여 화면이 완전히 어두워졌을 때 실행할 로직을 전달합니다.
        StartCoroutine(LoadingManager.Instance.FadeAndAction(() => {
            SceneManager.LoadScene("MainScene"); // 실제 게임 플레이 씬 이름
        }));
    }
}
