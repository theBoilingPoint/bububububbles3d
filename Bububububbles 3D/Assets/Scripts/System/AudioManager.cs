using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private bool loopBackgroundMusic = false;
    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private string startClip;
    [SerializeField] private GameObject[] clips;
    
    public static AudioManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (!backgroundMusic.IsUnityNull())
        {
            backgroundMusic.loop = loopBackgroundMusic;
            backgroundMusic.Play();
        }

        if (startClip != "")
        {
            PlayClip(startClip);
        }
    }

    public void PlayClip(string clipName)
    {
        var src = FindSource(clipName);
        if (src == null) return;
        src.Stop();
        src.Play();
    }
    
    public IEnumerator PlayClipAndWait(string clipName)
    {
        var src = FindSource(clipName);
        if (src == null || src.clip == null)
            yield break;

        src.Stop();   // ensure we start from the beginning
        src.Play();

        // duration adjusted by pitch; use DSP clock so timescale / pauses don't matter
        double endDsp = AudioSettings.dspTime + (src.clip.length / Mathf.Max(0.001f, src.pitch));
        while (AudioSettings.dspTime < endDsp)
            yield return null;
    }

    private AudioSource FindSource(string clipName)
    {
        for (int i = 0; i < clips.Length; i++)
        {
            var go = clips[i];
            if (go != null && go.name == clipName)
            {
                var src = go.GetComponent<AudioSource>();
                if (src == null) Debug.LogError($"{go.name} is missing an AudioSource");
                return src;
            }
        }
        Debug.LogWarning($"Audio clip GameObject '{clipName}' not found in AudioManager.clips");
        return null;
    }
}
