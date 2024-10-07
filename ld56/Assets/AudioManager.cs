using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [SerializeField] private GameObject prefabAudioSource;
    public List<AudioClip> musicTracks; // Liste der AudioClips, die abgespielt werden sollen
    private AudioSource audioSource; // AudioSource-Komponente zum Abspielen der Musik
    private int currentClipIndex = 0; // Der Index des aktuell abgespielten AudioClips
    [SerializeField] private bool _randomize = false;

    public bool mute = false;

    // AudioClips for SoundEffects
    [SerializeField] private AudioClip mushroomBounceClip;
    [SerializeField] private AudioClip grabCollectableClip;
    [SerializeField] private AudioClip bullettimeMushroomBounceClip;
    [SerializeField] private AudioClip jumpClip;
    

    private void Awake()
    {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (musicTracks.Count > 0) {
            if (_randomize)
                currentClipIndex = Random.Range(0, musicTracks.Count);
            if(!mute) StartCoroutine(PlayAudioClips());
        } else {
            Debug.LogWarning("Keine AudioClips in der Liste vorhanden.");
        }
    }

    IEnumerator PlayAudioClips() {
        while (true) {
            audioSource.clip = musicTracks[currentClipIndex];
            audioSource.Play();

            yield return new WaitForSeconds(audioSource.clip.length);

            currentClipIndex++;
            if (currentClipIndex >= musicTracks.Count) {
                currentClipIndex = 0; 
            }
        }
    }

    public void Update () {
        mute = GameManager.instance.muteState;
        audioSource.mute = mute;
    }

    public void PlaySound (AudioClip clip, float pitchModifier = 0f, float volumeModifier = 0f) {
        if(!mute) {
            GameObject soundEffect = Instantiate(prefabAudioSource, Camera.main.transform, false);
            soundEffect.GetComponent<AudioSource>().clip = clip;
            soundEffect.GetComponent<AudioSource>().pitch += pitchModifier;
            soundEffect.GetComponent<AudioSource>().volume += volumeModifier;
            soundEffect.GetComponent<AudioSource>().Play();
        }
    }
}
