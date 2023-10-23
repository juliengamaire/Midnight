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
    [SerializeField]
    private TMP_InputField _clientIdInputField;
    [SerializeField]
    private TMP_InputField _clientSecretInputField;
    [SerializeField]
    private Toggle _rememberMeToggle;
    [SerializeField]
    private Button _signInButton;
    [SerializeField]
    private string _sceneNameToLoad;

    private const string RememberMeKey = "Midnight_RememberMe";
    private const string SpotifyClientIDKey = "Midnight_SpotifyClientID";
    private const string SpotifyClientSecretKey = "Midnight_SpotifyClientSecret";

    public void Start()
    {
        if (_signInButton != null)
        {
            _signInButton.onClick.AddListener(() => this.OnLoginButtonClicked());
        }

        LoadRememberMe();
    }

    protected override void OnSpotifyConnectionChanged(SpotifyClient client)
    {
        base.OnSpotifyConnectionChanged(client);

        bool isConnected = client != null;

        if (isConnected)
        {
            StartCoroutine(LoadSceneAsync(_sceneNameToLoad));
        }
    }

    private void SignIn()
    {
        SpotifyService service = SpotifyService.Instance;

        if (!service.IsConnected)
        {
            service._authMethodConfig.ClientID = _clientIdInputField.text;
            if (service.AuthType == AuthenticationType.ClientCredentials)
            {
                (service._authMethodConfig as ClientCredentials_AuthConfig).ClientSecret = _clientSecretInputField.text;
            }
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
            // Wait for the next frame
            yield return null;
        }
    }

    private void OnLoginButtonClicked()
    {
        if (_rememberMeToggle.isOn)
        {
            PlayerPrefs.SetInt(RememberMeKey, 1);
            PlayerPrefs.SetString(SpotifyClientIDKey, _clientIdInputField.text);
            PlayerPrefs.SetString(SpotifyClientSecretKey, _clientSecretInputField.text);
        }
        else
        {
            PlayerPrefs.SetInt(RememberMeKey, 0);
            PlayerPrefs.DeleteKey(SpotifyClientIDKey);
            PlayerPrefs.DeleteKey(SpotifyClientSecretKey);
        }

        PlayerPrefs.Save();

        SignIn();
    }

    private void LoadRememberMe()
    {
        if (PlayerPrefs.HasKey(RememberMeKey) && PlayerPrefs.GetInt(RememberMeKey) == 1)
        {
            _rememberMeToggle.isOn = true;

            if (PlayerPrefs.HasKey(SpotifyClientIDKey))
            {
                _clientIdInputField.text = PlayerPrefs.GetString(SpotifyClientIDKey);
            }
            if (PlayerPrefs.HasKey(SpotifyClientSecretKey))
            {
                _clientSecretInputField.text = PlayerPrefs.GetString(SpotifyClientSecretKey);
            }
        }
        else
        {
            _rememberMeToggle.isOn = false;
        }
    }    
}
