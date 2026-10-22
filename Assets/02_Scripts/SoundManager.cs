using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    public enum SoundType { 
        Jump, Hit, Attack,
        ItemPickup, PlayerHit, PotionUse, 
        Skill_Q, Skill_W, Skill_E, Skill_R, 
        Shop_Transaction, Equip, Unequip, Teleport,
        EnemyDie    
    }

    [System.Serializable]
    public struct SoundData { 
        public SoundType type; 
        public AudioClip clip; 
    }

    [Header("오디오 소스")]
    public AudioSource sfxSource;
    public AudioSource bgmSource;

    [Header("사운드 리스트")]
    public List<SoundData> sfxList;

    private Dictionary<SoundType,AudioClip> sfxDictionary = new Dictionary<SoundType,AudioClip>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);  //씬 전환 시에도 유지
            InitializeDictionary();
        }
    }

    private void Start()
    {
        if (bgmSource != null && bgmSource.clip != null)
        {
            bgmSource.Play();
        }
    }

    private void InitializeDictionary()
    {
        sfxDictionary.Clear();
        foreach (var data in sfxList)
        {
            if (data.clip != null && !sfxDictionary.ContainsKey(data.type))
            {
                sfxDictionary.Add(data.type, data.clip);
            }
        }
    }

    // [방법 1] 공통 사운드 재생 (Enum 기반: 스킬, 아이템 등)
    public void PlaySFX(SoundType type)
    {

        if (sfxDictionary.TryGetValue(type, out AudioClip clip))
        {
            sfxSource.pitch = Random.Range(0.9f, 1.1f); // 변조
            sfxSource.PlayOneShot(clip);
        }
    }

    // [방법 2] 개별 사운드 재생 (Clip 직접 전달: 몬스터 공격용)
    public void PlayClip(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

}
