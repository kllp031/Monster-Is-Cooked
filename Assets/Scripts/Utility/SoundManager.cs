using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource; 
    [SerializeField] private AudioSource sfxSource;

    [Header("Music")]
    //[SerializeField] public float pitchVar;
    [SerializeField] public AudioClip homeMusic;
    [SerializeField] public AudioClip battleMusic;

    [Header("Sound")]
    [SerializeField] public AudioClip playerAttackSound;

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
