using Midnight.Utils;
using SpotifyAPI.Web;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DataBaseController : MonoBehaviour
{
    [SerializeField]
    private Button _feedByDataBaseInputButton;
    [SerializeField]
    private Button _saveButton;
    [SerializeField]
    private Button _loadDataBaseButton;
    [SerializeField]
    private TextMeshProUGUI _loadingText;
    [SerializeField]
    private TextMeshProUGUI _jsonPathText;

    private SpotifyClient _client;
    private DataBaseManager _dataBaseManager;

    public void Awake()
    {
        _feedByDataBaseInputButton.interactable = false;
        _saveButton.interactable = false;
        _loadDataBaseButton.interactable = false;
    }

    // Start is called before the first frame update
    public void Start()
    {
        _client = SpotifyService.Instance.GetSpotifyClient();

        if (_client == null)
        {
            _loadingText.text = "Client Spotify not connected !";
        }

        else
        {
            _dataBaseManager = new DataBaseManager();

            _dataBaseManager.DataBaseLoaded += OnDataBaseLoaded;

            _loadDataBaseButton.interactable = true;
            _loadDataBaseButton.onClick.AddListener(this.LoadDataBase);

            _feedByDataBaseInputButton.onClick.AddListener(this.FeedByDataBaseInput);

        }
    }

    private void LoadDataBase()
    {
        string jsonPath = Utils.GetPathViaFileExplorer("Choose JSON File", "json");

        if (!String.IsNullOrEmpty(jsonPath))
        {
            if (_dataBaseManager != null)
            {
                _dataBaseManager.LoadJSONDataBase(jsonPath);
            }
        }
    }

    private void OnDataBaseLoaded(bool isCorrectlyLoaded)
    {
        if (isCorrectlyLoaded)
        {
            _feedByDataBaseInputButton.interactable = true;
            _saveButton.interactable = true;
            _loadDataBaseButton.interactable = false;

            _loadingText.text = "DataBase correctly loaded !";
            _loadingText.color = Color.green;
        }
        else
        {
            _loadingText.text = "DataBase not correctly loaded !";
            _loadingText.color = Color.red;
        }
    }

    private void FeedByDataBaseInput()
    {
        
    }
}
