using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

// BGM 정보
[System.Serializable]
public class BGM
{
    public string sceneName;
    public AudioClip clip;
    [Range(0.0f, 1.0f)]
    public float volume;
    public bool isLoop;
    public bool isPlayOnStart;
}


public class BGMManager : Singleton<BGMManager>
{
    // 소스
    [SerializeField] private BGM[] bgmSource;
    [SerializeField] private BGM[] bossBGM;
    [SerializeField] private BGM[] eliteBGM;

    // 볼륨 조절
    [SerializeField] private float bgmVolume = 0.5f;

    AudioSource audioSource;

    List<(string sceneName, int index)> sceneBGMList = new List<(string sceneName, int index)>();

    int currentNormalBGMIndex = 0;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // 씬에 맞는 인덱스로 구분지어서 저장
        for (int i = 0; i < bgmSource.Length; i++)
        {
            sceneBGMList.Add((bgmSource[i].sceneName, i));
        }

        PlayNormalBGM();
    }

    void Update()
    {
        // 현재 씬이 바뀌면 브금 변경
        if (SceneManager.GetActiveScene().name != bgmSource[currentNormalBGMIndex].sceneName)
        {
            PlayNormalBGM();
        }
    }

    public void PlayNormalBGM()
    {
        // 현재 씬에 맞는 모든 인덱스 찾기
        var indices = sceneBGMList.Where(x => x.sceneName == SceneManager.GetActiveScene().name)
                                 .Select(x => x.index)
                                 .ToList();
        
        // 랜덤
        currentNormalBGMIndex = indices.Count > 0 ? indices[Random.Range(0, indices.Count)] : 0;

        if (bgmSource[currentNormalBGMIndex].isPlayOnStart)
        {
            PlayBGM(currentNormalBGMIndex);
        }
    }

    public void PlayBGM(int index)
    {
        audioSource.clip = bgmSource[index].clip;
        audioSource.volume = bgmVolume;
        audioSource.Play();
    }

    public void PlayBossBGM()
    {
        audioSource.clip = bossBGM[Random.Range(0, bossBGM.Length)].clip;
        audioSource.volume = bgmVolume;
        audioSource.Play();
    }

    public void PlayEliteBGM()
    {
        audioSource.clip = eliteBGM[Random.Range(0, eliteBGM.Length)].clip;
        audioSource.volume = bgmVolume;
        audioSource.Play();
    }

    public void StopBGM()
    {
        audioSource.Stop();
    }
}
