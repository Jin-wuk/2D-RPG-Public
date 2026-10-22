using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour

{
    public static SettingsManager Instance;

    [Header("설정창 UI")]
    [SerializeField] private GameObject settingsPanel; // 끄고 켤 패널 오브젝트

    [Header("오디오 설정")]
    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    public bool IsSettingsOpen => settingsPanel != null && settingsPanel.activeSelf;
    private void Awake()
    {
        // [필수] 게임이 시작될 때 자기 자신을 인스턴스에 할당합니다.
        if (Instance == null)
        {
            Instance = this;
            // 씬 이동 시에도 파괴되지 않게 하려면 아래 줄 추가 (선택사항)
            // DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject); // 중복 생성 방지
        }
    }

    void Start()
    {
        // 시작할 때 설정창은 꺼둠
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }
    void Update()
    {
        // 1. [추가] 플레이어가 죽었다면 설정창 열기/닫기 차단
        if (PlayerStats.Instance.isDead) return;

        // 1. ESC 키를 누르면 설정창 토글
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettings();
        }
    }

    public void SetBGMVolume()
    {
        float volume = bgmSlider.value;
        // 슬라이더 값(0.0001~1)을 데시벨(-80~0)로 변환
        masterMixer.SetFloat("BGMVolume", Mathf.Log10(volume) * 20);
    }

    public void SetSFXVolume()
    {
        float volume = sfxSlider.value;
        masterMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 40);
    }

    // 설정창 열기/닫기
    public void ToggleSettings()
    {
        if (PlayerStats.Instance != null && PlayerStats.Instance.isDead) return;
        if (settingsPanel == null) return;
        bool isActive = settingsPanel.activeSelf;
        settingsPanel.SetActive(!isActive);
    }

    //플레이어 죽었을때 패널열려있으면 강제로 닫는 함수
    public void ForceCloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            // 설정창이 닫힐 때 커서 상태를 게임용으로 되돌리고 싶다면 여기서 처리
        }
    }
}
