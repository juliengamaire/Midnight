using SpotifyAPI.Web;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// Simple controller script for signing in/out of Spotify using S4U
/// This is an example script of how/what you could/should implement
/// </summary>
public class LoginManager : SpotifyServiceListener
{
    public bool ServiceAuthOnStart = false;

    [SerializeField]
    private Button _signInButton, _signOutButton;
    [SerializeField]
    private TextMeshProUGUI _loadingText;

    public void Start()
    {
        SpotifyService.Instance.AuthorizeUserOnStart = ServiceAuthOnStart;

        if (_signInButton != null)
        {
            _signInButton.onClick.AddListener(() => this.OnSignIn());
        }
        if (_signOutButton != null)
        {
            _signOutButton.onClick.AddListener(() => this.OnSignOut());
        }

        if (!ServiceAuthOnStart)
        {
            _signInButton.gameObject.SetActive(true);
            _signOutButton.gameObject.SetActive(false);
        }

        _loadingText.text = string.Empty;
    }

    protected override void OnSpotifyConnectionChanged(SpotifyClient client)
    {
        base.OnSpotifyConnectionChanged(client);

        bool isConnected = client != null;
        _signInButton.gameObject.SetActive(!isConnected);
        _signOutButton.gameObject.SetActive(isConnected);

        if (isConnected)
        {
            StartCoroutine(LoadSceneAsync("VinylStore"));
        }
    }

    private void OnSignIn()
    {
        SpotifyService service = SpotifyService.Instance;

        if (!service.IsConnected)
        {
            service.AuthorizeUser();
        }
        else
        {
            Debug.LogError("Can't sign in. Already connected");
        }
    }

    private void OnSignOut()
    {
        SpotifyService service = SpotifyService.Instance;
        if (service.IsConnected)
        {
            service.DeauthorizeUser();
        }
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // Start loading the scene asynchronously
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);

        // While the scene is not fully loaded
        while (!asyncOperation.isDone)
        {
            // Update the progress bar or loading text
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
            _loadingText.text = $"Loading : {progress * 100}%";

            // Wait for the next frame
            yield return null;
        }
    }

    
}
