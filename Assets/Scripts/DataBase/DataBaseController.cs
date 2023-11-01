using Midnight.Utils;
using SpotifyAPI.Web;
using System;
using System.Threading;
using System.Threading.Tasks;
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
    private TextMeshProUGUI _jsonDataBasePathText;
    [SerializeField]
    private Button _loadInputDataBaseButton;
    [SerializeField]
    private TextMeshProUGUI _jsonInputDataBasePathText;
    [SerializeField]
    private TextMeshProUGUI _loadingText;

    private SpotifyClient _client;
    private DataBaseManager _dataBaseManager;
    private DataBaseInput _dataBaseInput;

    public void Awake()
    {
        _feedByDataBaseInputButton.interactable = false;
        _saveButton.interactable = false;
        _loadDataBaseButton.interactable = false;
        _loadInputDataBaseButton.interactable = false;
    }

    // Start is called before the first frame update
    public void Start()
    {
        _client = SpotifyService.Instance.GetSpotifyClient();

        if (_client == null)
        {
            _loadingText.text = "Client Spotify not connected !";
            _loadingText.color = Color.red;
        }

        else
        {
            _dataBaseManager = new DataBaseManager();

            _dataBaseManager.DataBaseLoaded += OnDataBaseLoaded;
            _dataBaseManager.InputDataBaseLoaded += OnInputDataBaseLoaded;
            _dataBaseManager.FeedPerformed += OnFeedPerformed;
            _dataBaseManager.NewDataBaseSaved += OnNewDataBaseSaved;
            _dataBaseManager.PerformSearchProgressUpdated += OnPerformSearchProgressUpdated;

            _loadDataBaseButton.interactable = true;
            _loadDataBaseButton.onClick.AddListener(this.LoadDataBase);

            _loadInputDataBaseButton.onClick.AddListener(this.LoadInputDataBase);

            _feedByDataBaseInputButton.onClick.AddListener(this.FeedByDataBaseInput);

            _saveButton.onClick.AddListener(this.SaveNewDataBase);
        }
    }

    public void OnDestroy()
    {
        _dataBaseManager.DataBaseLoaded -= OnDataBaseLoaded;
        _dataBaseManager.InputDataBaseLoaded -= OnInputDataBaseLoaded;
        _dataBaseManager.FeedPerformed -= OnFeedPerformed;
        _dataBaseManager.NewDataBaseSaved -= OnNewDataBaseSaved;
        _dataBaseManager.PerformSearchProgressUpdated -= OnPerformSearchProgressUpdated;
    }

    private void LoadDataBase()
    {
        string jsonPath = Utils.GetPathViaFileExplorer("Choose JSON File", "json");

        if (!String.IsNullOrEmpty(jsonPath))
        {
            if (_dataBaseManager != null)
            {
                _dataBaseManager.LoadJSONDataBase(jsonPath);
                _jsonDataBasePathText.text = jsonPath;
            }
        }
    }

    private void OnDataBaseLoaded(bool isCorrectlyLoaded)
    {
        if (isCorrectlyLoaded)
        {
            _loadInputDataBaseButton.interactable = true;
            _loadDataBaseButton.interactable = false;

            _loadingText.text = "DataBase correctly loaded !";
            if (_dataBaseManager.DataBase.Albums != null)
            {
                _loadingText.text = String.Format("{0}\n{1} albums and {2} artists in it !", 
                    _loadingText.text, _dataBaseManager.DataBase.Albums.Length, _dataBaseManager.DataBase.Artists.Length);
            }
            _loadingText.color = Color.green;
        }
        else
        {
            _loadingText.text = "DataBase not correctly loaded !";
            _loadingText.color = Color.red;
        }
    }

    private void LoadInputDataBase()
    {
        string jsonPath = Utils.GetPathViaFileExplorer("Choose JSON File", "json");

        if (!String.IsNullOrEmpty(jsonPath))
        {
            if (_dataBaseManager != null)
            {
                _dataBaseManager.LoadJSONDataBaseInput(jsonPath);
                _jsonInputDataBasePathText.text = jsonPath;
            }
        }
    }

    private void OnInputDataBaseLoaded(DataBaseInput dataBaseInput)
    {
        if (dataBaseInput != null)
        {
            _dataBaseInput = dataBaseInput;
            _feedByDataBaseInputButton.interactable = true;
            _loadInputDataBaseButton.interactable = false;

            _loadingText.text = String.Format("DataBase correctly loaded !\n" +
                "Click on the button 'Feed' to perform {0} playlists, {1} albums and {2} artists searches",
                _dataBaseInput.playlists.Length, _dataBaseInput.albums.Length, _dataBaseInput.artists.Length);
            _loadingText.color = Color.green;
        }
        else
        {
            _loadingText.text = "Input DataBase not correctly loaded !";
            _loadingText.color = Color.red;
        }
    }

    private void FeedByDataBaseInput()
    {
        if (_dataBaseInput != null)
        {
            _dataBaseManager.PerformSearchByDataBaseInput(_dataBaseInput);

            _feedByDataBaseInputButton.interactable = false;
        }
    }

    private void OnPerformSearchProgressUpdated(float progress)
    {
        _loadingText.text = string.Format("Feeding : {0} %", Math.Round(progress, 1));
        _loadingText.color = Color.blue;
    }

    private void OnFeedPerformed()
    {
        _loadingText.text = String.Format("Feed finished ! {0} new albums and {1} new artists.\nDon't forget to save your new DataBase !", 
            _dataBaseManager.GetNewAlbumsAddedCount(), _dataBaseManager.GetNewArtistsAddedCount());
        _loadingText.color = Color.green;

        _saveButton.interactable = true;
    }

    private void SaveNewDataBase()
    {
        string jsonPath = Utils.GetSavePathViaFileExplorer("Save DataBase", "VinylStoreDataBase", "json");
        if (!String.IsNullOrEmpty(jsonPath))
        {
            _dataBaseManager.SaveJSONDataBase(jsonPath);
        }
        else
        {
            _loadingText.text = "Error with the destination path";
            _loadingText.color = Color.red;
        }
    }

    private void OnNewDataBaseSaved(bool isCorrectlySaved)
    {
        if (isCorrectlySaved)
        {
            _saveButton.interactable = false;

            _loadingText.text = "New DataBase correctly saved !";
            _loadingText.color = Color.green;
        }
        else
        {
            _loadingText.text = "New DataBase not correctly saved !";
            _loadingText.color = Color.red;
        }
    }
}
