using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Music")]
    public AudioClip musicClip;

    [Header("Player")]
    public AudioClip attackClip;
    public AudioClip hurtClip;
    public AudioClip stepClip;
    public AudioClip loseClip;
    public AudioClip winClip;

    [Header("Enemies")]
    public AudioClip enemyDieClip;
    public AudioClip bossDieClip;

    [Header("Resource nodes")]
    public AudioClip oreBreakClip;
    public AudioClip cottonBreakClip;

    [Header("Menus")]
    public AudioClip menuOpenClip;
    public AudioClip menuCloseClip;
    public AudioClip craftClip;

    [Header("Door")]
    public AudioClip warningClip;
    public AudioClip clickClip;
    public AudioClip warpClip;
    public AudioClip doorOpenClip;

    [Range(0f, 1f)] public float musicVolume = 0.4f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;
    [Range(0f, 1f)] public float stepVolume = 0.5f; // footsteps are quieter than other sounds

    AudioSource musicSource;
    AudioSource sfxSource;
    AudioSource stepSource;
    float lastHurtTime = -10f;
    float lastEnemyDieTime = -10f;

    void Awake()
    {
        // [1] one AudioSource for the music (loops forever)
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.playOnAwake = false;

        // [2] one AudioSource for the sound effects
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        // [3] one AudioSource only for footsteps (so we can stop it at once)
        CreateStepSource();
    }

    void Start()
    {
        // start the music when the game starts
        if (musicClip != null)
        {
            musicSource.Play();
        }
    }

    // make the footstep AudioSource if it does not exist yet
    void CreateStepSource()
    {
        if (stepSource != null)
        {
            return;
        }
        stepSource = gameObject.AddComponent<AudioSource>();
        stepSource.loop = false;
        stepSource.playOnAwake = false;
    }

    // ---------- player ----------

    public void PlayAttack()
    {
        // small random pitch so the sound is not the same every time
        PlaySfx(attackClip, Random.Range(0.95f, 1.05f), 1f);
    }

    public void PlayHurt()
    {
        // many enemies can hit at the same time: wait 0.15 seconds between two hurt sounds
        if (Time.unscaledTime - lastHurtTime < 0.15f)
        {
            return;
        }
        lastHurtTime = Time.unscaledTime;
        PlaySfx(hurtClip, Random.Range(0.95f, 1.05f), 1f);
    }

    public void PlayStep()
    {
        if (stepClip == null)
        {
            return;
        }
        CreateStepSource();

        // Play() restarts the same source, so footsteps never pile up on top of each other
        stepSource.clip = stepClip;
        stepSource.pitch = Random.Range(0.9f, 1.1f);
        stepSource.volume = sfxVolume * stepVolume;
        stepSource.Play();
    }

    // cut the footstep sound at once (when we stop walking or the game is paused)
    public void StopStep()
    {
        if (stepSource != null && stepSource.isPlaying)
        {
            stepSource.Stop();
        }
    }

    public void PlayLose()
    {
        musicSource.Stop();
        StopStep();
        PlaySfx(loseClip, 1f, 1f);
    }

    public void PlayWin()
    {
        musicSource.Stop();
        StopStep();
        PlaySfx(winClip, 1f, 1f);
    }

    // ---------- enemies ----------

    public void PlayEnemyDie()
    {
        // many enemies can die in the same frame: wait a little between two sounds
        if (Time.unscaledTime - lastEnemyDieTime < 0.05f)
        {
            return;
        }
        lastEnemyDieTime = Time.unscaledTime;
        PlaySfx(enemyDieClip, Random.Range(0.9f, 1.1f), 1f);
    }

    public void PlayBossDie()
    {
        PlaySfx(bossDieClip, 1f, 1f);
    }

    // ---------- resource nodes ----------

    // choose the sound by resource type: Iron Ore = rock, Cotton = soft
    public void PlayNodeBreak(string resourceType)
    {
        if (resourceType == "Iron Ore")
        {
            PlaySfx(oreBreakClip, Random.Range(0.95f, 1.05f), 1f);
        }
        else if (resourceType == "Cotton")
        {
            PlaySfx(cottonBreakClip, Random.Range(0.95f, 1.05f), 1f);
        }
    }

    // ---------- menus ----------

    public void PlayMenuOpen()
    {
        PlaySfx(menuOpenClip, 1f, 1f);
    }

    public void PlayMenuClose()
    {
        PlaySfx(menuCloseClip, 1f, 1f);
    }

    public void PlayCraft()
    {
        PlaySfx(craftClip, 1f, 1f);
    }

    // ---------- door and buttons ----------

    public void PlayWarning()
    {
        PlaySfx(warningClip, 1f, 1f);
    }

    public void PlayClick()
    {
        PlaySfx(clickClip, 1f, 1f);
    }

    public void PlayWarp()
    {
        PlaySfx(warpClip, 1f, 1f);
    }

    public void PlayDoorOpen()
    {
        PlaySfx(doorOpenClip, 1f, 1f);
    }

    void PlaySfx(AudioClip clip, float pitch, float volumeScale)
    {
        // no clip yet = do nothing (so the game does not break)
        if (clip == null)
        {
            return;
        }
        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(clip, sfxVolume * volumeScale);
    }
}