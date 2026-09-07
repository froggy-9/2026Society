using UnityEngine;

public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance { get; private set; }

    private const string BgmVolumeKey = "BGMVolume";
    private const string SfxVolumeKey = "SFXVolume";

    [Header("Audio Sources")]
    [Tooltip("BGM을 재생할 AudioSource입니다. 비워두면 자동으로 생성합니다.")]
    [SerializeField] private AudioSource bgmSource;

    [Tooltip("SFX를 재생할 AudioSource입니다. 비워두면 자동으로 생성합니다.")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips")]
    [Tooltip("씬 시작 시 자동 재생할 BGM입니다. 아직 음악이 없으면 비워둬도 됩니다.")]
    [SerializeField] private AudioClip startBgm;

    [Tooltip("버튼 클릭 등 기본 UI 효과음으로 사용할 클립입니다.")]
    [SerializeField] private AudioClip defaultButtonSfx;

    [Header("Initial Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float defaultBgmVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float defaultSfxVolume = 1f;

    public float BgmVolume { get; private set; }
    public float SfxVolume { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureSources();
        LoadVolumes();
        ApplyVolumes();

        if (startBgm != null)
            PlayBgm(startBgm);
    }

    public static GameAudioManager GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        GameAudioManager existing = FindFirstObjectByType<GameAudioManager>();

        if (existing != null)
            return existing;

        GameObject audioRoot = new GameObject("GameAudioManager");
        return audioRoot.AddComponent<GameAudioManager>();
    }

    public void SetBgmVolume(float value)
    {
        BgmVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(BgmVolumeKey, BgmVolume);
        PlayerPrefs.Save();
        ApplyVolumes();
    }

    public void SetSfxVolume(float value)
    {
        SfxVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(SfxVolumeKey, SfxVolume);
        PlayerPrefs.Save();
        ApplyVolumes();
    }

    public void PlayBgm(AudioClip clip)
    {
        if (clip == null)
            return;

        EnsureSources();
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip == null)
            return;

        EnsureSources();
        sfxSource.PlayOneShot(clip, SfxVolume);
    }

    public void PlayDefaultButtonSfx()
    {
        PlaySfx(defaultButtonSfx);
    }

    private void EnsureSources()
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
        }
    }

    private void LoadVolumes()
    {
        BgmVolume = PlayerPrefs.GetFloat(BgmVolumeKey, defaultBgmVolume);
        SfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, defaultSfxVolume);
    }

    private void ApplyVolumes()
    {
        EnsureSources();
        bgmSource.volume = BgmVolume;
        sfxSource.volume = SfxVolume;
    }
}
