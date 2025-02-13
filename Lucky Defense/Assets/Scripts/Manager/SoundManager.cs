using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;
    
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioSource sfxSource; 

    private bool isBgmPlaying;
    private bool isSfxPlaying;
    
    private MainTitleUIManager mainTitleUIManager;
    private InGameUIManager inGameUIManager;

    [SerializeField] private List<AudioClip> sfxClips;
    
    [SerializeField] private List<Sprite> soundSpriteList;
    private List<Image> soundImageList;
    
    void Awake()
    {
        if (instance is null)
        {
            instance = this;

            DontDestroyOnLoad(gameObject);
        }
        else
        {
           
            Destroy(gameObject);
        }
        
        isBgmPlaying = true;
        isSfxPlaying = true;
        
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
    }
    
    public static SoundManager Instance
    {
        get
        {
            if (instance is null)
            {
                return null;
            }
            
            return instance;
        }
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0)
        {
            GameObject.FindGameObjectWithTag("MainTitleUIManager").TryGetComponent(out mainTitleUIManager);
            
            mainTitleUIManager.bgmButton.onClick.RemoveAllListeners();
            mainTitleUIManager.bgmButton.onClick.AddListener(SwitchBgmVolume);
            
            mainTitleUIManager.sfxButton.onClick.RemoveAllListeners();
            mainTitleUIManager.sfxButton.onClick.AddListener(SwitchSfxVolume);
            
            soundImageList = mainTitleUIManager.soundImageList;
        }
        else if (scene.buildIndex == 1)
        {
            GameObject.FindGameObjectWithTag("InGameUIManager").TryGetComponent(out inGameUIManager);
            
            inGameUIManager.bgmButton.onClick.RemoveAllListeners();
            inGameUIManager.bgmButton.onClick.AddListener(SwitchBgmVolume);
            
            inGameUIManager.sfxButton.onClick.RemoveAllListeners();
            inGameUIManager.sfxButton.onClick.AddListener(SwitchSfxVolume);
            
            soundImageList = inGameUIManager.soundImageList;
        }

        if (isBgmPlaying)
        {
            audioMixer.SetFloat("BGM", Mathf.Log10(1) * 20);
            
            soundImageList[0].sprite = soundSpriteList[0];
        }
        else
        {
            audioMixer.SetFloat("BGM", -80f);
            
            soundImageList[0].sprite = soundSpriteList[1];
        }

        if (isSfxPlaying)
        {
            audioMixer.SetFloat("SFX", Mathf.Log10(1) * 20);
            
            soundImageList[1].sprite = soundSpriteList[0];
        }
        else
        {
            audioMixer.SetFloat("SFX", -80f);
            
            soundImageList[1].sprite = soundSpriteList[1];
        }
    }
    
    public void SwitchBgmVolume()
    {
        if (!isBgmPlaying)
        {
            isBgmPlaying = true;
            audioMixer.SetFloat("BGM", Mathf.Log10(1) * 20);
            
            soundImageList[0].sprite = soundSpriteList[0];
        }
        else
        {
            isBgmPlaying = false;
            audioMixer.SetFloat("BGM", -80f);
            
            soundImageList[0].sprite = soundSpriteList[1];
        }
    }
 
    public void SwitchSfxVolume()
    { 
        if (!isSfxPlaying)
        {
            isSfxPlaying = true;
            audioMixer.SetFloat("SFX", Mathf.Log10(1) * 20);
            
            soundImageList[1].sprite = soundSpriteList[0];
        }
        else
        {
            isSfxPlaying = false;
            audioMixer.SetFloat("SFX", -80f);
            
            soundImageList[1].sprite = soundSpriteList[1];
        }
    }

    public void PlaySfx(SfxClipId clipId)
    {
        PlaySfx(sfxClips[(int)clipId]);
    }
    
    private void PlaySfx(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
}