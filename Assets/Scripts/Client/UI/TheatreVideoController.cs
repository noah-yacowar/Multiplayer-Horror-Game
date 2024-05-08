using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class TheatreVideoController : MonoBehaviour
{
    public static TheatreVideoController Instance { get; private set; }

    public VideoPlayer theatreVideoPlayer;
    public AudioSource theatreAudioSource;

    public VideoClip lobbyOptionsVideo;
    public VideoClip lobbyIntermissionVideo;
    public VideoClip lobbySurvivorVideo;
    public VideoClip lobbyKillerVideo;

    public AudioClip lobbyOptionsAudio;
    public AudioClip lobbyIntermissionAudio;
    public AudioClip lobbySurvivorAudio;
    public AudioClip lobbyKillerAudio;

    private const float INTERMISSION_TIMER_MAX = 2.5f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ChangeScreenToLobbyOptions();
    }

    private void ChangeScreen(VideoClip video, AudioClip audio)
    {
        theatreVideoPlayer.clip = video;
        theatreAudioSource.clip = audio;

        theatreVideoPlayer.Play();
        theatreAudioSource.Play();
    }

    public IEnumerator ChangeScreenToLobbyOptions()
    {
        yield return StartCoroutine(PlayLobbyIntermission());

        ChangeScreen(lobbyOptionsVideo, lobbyOptionsAudio);
    }

    public void ChangeScreenToLobbyIntermission()
    {
        ChangeScreen(lobbyIntermissionVideo, lobbyIntermissionAudio);
    }

    public IEnumerator ChangeScreenToLobbySurvivor()
    {
        yield return StartCoroutine(PlayLobbyIntermission());

        ChangeScreen(lobbySurvivorVideo, lobbySurvivorAudio);
    }

    public IEnumerator ChangeScreenToLobbyKiller() 
    {
        yield return StartCoroutine(PlayLobbyIntermission());

        ChangeScreen(lobbyKillerVideo, lobbyKillerAudio);
    }

    IEnumerator PlayLobbyIntermission()
    {
        ChangeScreenToLobbyIntermission();
        float intermissionTimer = 0f;

        while (intermissionTimer < INTERMISSION_TIMER_MAX)
        {
            intermissionTimer += Time.deltaTime;
            yield return null; // Yielding null means wait until the next frame
        }
    }
}
