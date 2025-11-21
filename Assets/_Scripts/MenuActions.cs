#if UNITY_WEBGL && !UNITY_EDITOR
#define USE_WEBGL_URL
#endif

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

public class MenuActions : MonoBehaviour
{
    [Header("Cutscene Config")]
    [SerializeField] private VideoClip cutscene1;
    [SerializeField] private string webglCutsceneUrl;  // <-- URL do vídeo hospedado no itch.io
    [SerializeField] private int gameSceneIndex = 2;
    [SerializeField] private bool allowSkip = true;

    [Header("Menu UI")]
    [SerializeField] private Canvas menuCanvas;
    [SerializeField] private Image blackout;

    private GameObject playerGO;
    private VideoPlayer vp;
    private AudioSource audioSrc;
    private bool playing;

    public void IniciarJogo()
    {
#if USE_WEBGL_URL
        if (string.IsNullOrEmpty(webglCutsceneUrl))
        {
            Debug.LogError("[MenuActions] webglCutsceneUrl não configurado!");
            SceneManager.LoadScene(gameSceneIndex);
            return;
        }
#else
        if (cutscene1 == null)
        {
            SceneManager.LoadScene(gameSceneIndex);
            return;
        }
#endif
        PlayCutscene();
    }

    private void PlayCutscene()
    {
        if (blackout != null) blackout.enabled = true;

        playing = true;

        playerGO = new GameObject("CutscenePlayer");
        DontDestroyOnLoad(playerGO);

        vp = playerGO.AddComponent<VideoPlayer>();
        audioSrc = playerGO.AddComponent<AudioSource>();

        vp.playOnAwake = false;
        vp.isLooping = false;
        vp.waitForFirstFrame = true;

#if USE_WEBGL_URL
        vp.source = VideoSource.Url;
        vp.url = webglCutsceneUrl;
#else
        vp.source = VideoSource.VideoClip;
        vp.clip = cutscene1;
#endif

        vp.renderMode = VideoRenderMode.CameraNearPlane;
        vp.targetCamera = Camera.main ?? FindAnyObjectByType<Camera>();
        vp.aspectRatio = VideoAspectRatio.FitVertically;

        vp.audioOutputMode = VideoAudioOutputMode.AudioSource;
        vp.EnableAudioTrack(0, true);
        vp.SetTargetAudioSource(0, audioSrc);

        vp.prepareCompleted += OnPrepared;
        vp.loopPointReached += OnCutsceneFinished;
        vp.Prepare();
    }

    private void OnPrepared(VideoPlayer _)
    {
        vp.prepareCompleted -= OnPrepared;

        if (menuCanvas) menuCanvas.enabled = false;

        vp.Play();
        StartCoroutine(HideBlackoutNextFrame());
    }

    private IEnumerator HideBlackoutNextFrame()
    {
        yield return null;
        if (blackout != null) blackout.enabled = false;
    }

    private void Update()
    {
        if (!playing || !allowSkip) return;

        if ((Keyboard.current?.escapeKey.wasPressedThisFrame ?? false) ||
            (Gamepad.current?.startButton.wasPressedThisFrame ?? false))
        {
            if (blackout != null) blackout.enabled = true;
            vp.Stop();
            OnCutsceneFinished(vp);
        }
    }

    private IEnumerator LoadGameAsync()
    {
        var op = SceneManager.LoadSceneAsync(gameSceneIndex);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        yield return null;
        op.allowSceneActivation = true;
    }

    private void OnCutsceneFinished(VideoPlayer _)
    {
        playing = false;
        vp.loopPointReached -= OnCutsceneFinished;

        if (blackout != null) blackout.enabled = true;

        if (playerGO) Destroy(playerGO);
        if (Time.timeScale != 1f) Time.timeScale = 1f;

        StartCoroutine(LoadGameAsync());
    }

    public void Menu() => SceneManager.LoadScene(0);
}
