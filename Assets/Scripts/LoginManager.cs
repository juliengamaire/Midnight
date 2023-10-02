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

    public delegate void ClientConnected();
    public event ClientConnected OnClientConnected;

    [SerializeField]
    private TextMeshProUGUI _loadingText;
    [SerializeField]
    private TMP_InputField clientIdInputField;
    [SerializeField]
    private Toggle rememberMeToggle;
    [SerializeField]
    private Button _signInButton;
    [SerializeField]
    private string SceneNameToLoad;

    private const string RememberMeKey = "RememberMe";
    private const string SpotifyClientIDKey = "SpotifyClientID";

    public void Start()
    {
        SpotifyService.Instance.AuthorizeUserOnStart = ServiceAuthOnStart;

        if (_signInButton != null)
        {
            _signInButton.onClick.AddListener(() => this.OnLoginButtonClicked());
        }

        LoadRememberMe();

        _loadingText.text = string.Empty;
    }

    protected override void OnSpotifyConnectionChanged(SpotifyClient client)
    {
        base.OnSpotifyConnectionChanged(client);

        bool isConnected = client != null;
        _signInButton.gameObject.SetActive(!isConnected);

        if (isConnected)
        {
            OnClientConnected?.Invoke();

            LoadSceneAsync(SceneNameToLoad);
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

    private void OnLoginButtonClicked()
    {
        if (rememberMeToggle.isOn)
        {
            PlayerPrefs.SetInt(RememberMeKey, 1);
            PlayerPrefs.SetString(SpotifyClientIDKey, clientIdInputField.text);
        }
        else
        {
            PlayerPrefs.SetInt(RememberMeKey, 0);
            PlayerPrefs.DeleteKey(SpotifyClientIDKey);
        }

        PlayerPrefs.Save();

        OnSignIn();
    }

    private void LoadRememberMe()
    {
        if (PlayerPrefs.HasKey(RememberMeKey) && PlayerPrefs.GetInt(RememberMeKey) == 1)
        {
            rememberMeToggle.isOn = true;

            if (PlayerPrefs.HasKey(SpotifyClientIDKey))
            {
                clientIdInputField.text = PlayerPrefs.GetString(SpotifyClientIDKey);
            }
        }
        else
        {
            rememberMeToggle.isOn = false;
        }
    }    
}
