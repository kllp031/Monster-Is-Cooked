using System.Collections;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource; 
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clip")]
    [Header("Theme music")]
    public float fadeDuration = 2.0f;
    public AudioClip cookingMusic;
    public AudioClip battleMusic;


    [Header("Player")]
    public AudioClip playerAttack;
    public AudioClip metalHit;
    public AudioClip playerDash;
    public AudioClip playerCollect;
    public AudioClip playerThrow;
    public AudioClip playerHurt;
    public AudioClip playerDie;

    [Header("Customer")]
    public AudioClip customerAppear;
    public AudioClip customerHappy;
    public AudioClip customerSad;


    [Header("Stove")]
    public AudioClip stoveOpen;
    public AudioClip startCooking;
    public AudioClip cookingSuccess;
    public AudioClip cookingFailure;

    [Header("UI")]
    public AudioClip buttonClick;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource.clip == clip) return; 

        musicSource.clip = clip;
        musicSource.loop = true;

        musicSource.volume = 0f;
        musicSource.Play();
        StartCoroutine(FadeIn());
    }

    public void PlayeBattleMusic()
    {
        PlayMusic(battleMusic);
    }    

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        /*float randomPitch = 1f + Random.Range(-pitchVar, pitchVar);
        sfxSource.pitch = Mathf.Clamp(randomPitch, 0.1f, 3f);*/
        sfxSource.PlayOneShot(clip, volume);
    }

    IEnumerator FadeIn()
    {
        float currentTime = 0;

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, 1f, currentTime / fadeDuration);
            yield return null; 
        }

        musicSource.volume = 1f;
    }
}
