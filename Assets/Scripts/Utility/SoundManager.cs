using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource; 
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clip")]
    [Header("Theme music")]
    public AudioClip cookingMusic;
    public AudioClip battleMusic;


    [Header("Player")]
    public AudioClip playerAttack;
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
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        /*float randomPitch = 1f + Random.Range(-pitchVar, pitchVar);
        sfxSource.pitch = Mathf.Clamp(randomPitch, 0.1f, 3f);*/
        sfxSource.PlayOneShot(clip, volume);
    }

}
