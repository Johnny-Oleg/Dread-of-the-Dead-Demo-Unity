using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleMenuManager : MonoBehaviour
{
    [Header("Scene Settings")]
    public string firstLevelName = "MainHall";
    
    [Header("UI References")]
    public Image fadeOverlay; // Drag the Image_FadeOverlay here
    public Button startButton;
    public Button quitButton;
    
    [Header("Audio")]
    public AudioSource titleMusic; // Drag the TitleMusic GameObject here
    public float fadeDuration = 1.5f;

    private void Start()
    {
        // Ensure the screen starts completely transparent
        fadeOverlay.color = new Color(0, 0, 0, 0);
        
        // Link the buttons to the functions
        startButton.onClick.AddListener(OnStartClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void OnStartClicked()
    {
        // Disable buttons so the player can't spam them during the fade
        startButton.interactable = false;
        quitButton.interactable = false;
        
        StartCoroutine(TransitionToGame());
    }

    private void OnQuitClicked()
    {
        Debug.Log("Quit Game Initiated...");
        Application.Quit(); // Note: This only works in the built game, not the Editor
    }

    private IEnumerator TransitionToGame()
    {
        float timer = 0f;
        float startVolume = titleMusic.volume;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / fadeDuration;

            // Fade screen to black
            fadeOverlay.color = new Color(0, 0, 0, normalizedTime);
            
            // Fade out music
            if (titleMusic != null)
            {
                titleMusic.volume = Mathf.Lerp(startVolume, 0f, normalizedTime);
            }

            yield return null; // Wait for the next frame
        }

        // Load the actual game room
        SceneManager.LoadScene(firstLevelName);
    }
}
