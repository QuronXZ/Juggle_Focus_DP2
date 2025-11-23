using UnityEngine;


public class sounds : MonoBehaviour
{
    public static sounds Instance1;

    public AudioSource bgmSource1;
    public AudioSource sfxSource1;
    public AudioSource voiceSource1;
    public AudioSource twodee_sounds1;

    public AudioClip[] bgms1;
    public AudioClip[] sfxs1;
    public AudioClip[] voices1;
    public AudioClip[] twodee_sound1;

    void Awake()
    {
        if (Instance1 == null)
        {
            Instance1 = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Play BGM by index
    public void PlayBGM(int index)
    {
        //bgmSource.clip = bgms[index];
        //bgmSource.loop = true;
        //bgmSource.Play();
        bgmSource1.PlayOneShot(bgms1[index]);
    }

    // Play SFX by index
    public void PlaySFX(int index)
    {
        sfxSource1.PlayOneShot(sfxs1[index]);
    }

    // Play Voiceover by index
    public void PlayVoice(int index)
    {
        voiceSource1.PlayOneShot(voices1[index]);
    }
    public void SriLetsFight()
    {
        voiceSource1.PlayOneShot(voices1[3]);
    }


    public void PlayNavigationSFX()
    {
        sfxSource1.PlayOneShot(sfxs1[0]); // Navigate sound at index 0
    }

    public void PlayClickSFX()
    {
        sfxSource1.PlayOneShot(sfxs1[1]); // Click sound at index 1
    }
}

