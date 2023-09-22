using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Threading.Tasks;
using SFB;
using System.Linq;

public class JSONManager : MonoBehaviour
{
    private enum Genre
    {
        NONE,
        ACID_ROCK,
        ALT_ROCK,
        ALT_METAL,
        BLACK,
        BLACKENED_DEATH,
        BLUEGRASS,
        BLUES,
        BLUES_ROCK,
        CELTIC,
        DEATH_METAL,
        DEATHGRIND,
        DJENT,
        DOOM,
        FOLK,
        GARAGE,
        GLAM,
        GOREGRIND,
        GOTH,
        GRINDCORE,
        GROOVE,
        GRUNGE,
        HAIR,
        HARD_ROCK,
        HARDCORE,
        HEAVY_METAL,
        INDUSTRIAL,
        MATH,
        MELODEATH,
        METAL,
        METAL_MISC,
        METALCORE,
        METALSTEP,
        MODERN_PROG,
        MODERN_DEATHCORE,
        NEOCLASSICAL,
        NU,
        PIRATE,
        POP_METAL,
        POST,
        PROG,
        POWER,
        PSYCHEDELIC,
        PUNK,
        PUNK_ROCK,
        ROCK,
        ROCK_N_ROLL,
        ROCKABILLY,
        SLUDGE,
        SOUTHERN,
        STONER,
        SYMPHONIC,
        TECH_DEATH,
        TRASH,
        VIKING
    }

    [SerializeField]
    private LoginManager _loginManager;

    [SerializeField]
    private TMP_InputField _searchPlaylistField;
    [SerializeField]
    private TMP_InputField _searchAlbumField;
    [SerializeField]
    private TMP_InputField _artistField;
    [SerializeField]
    private TMP_Dropdown _genreDropdown;
    [SerializeField]
    private Button _feedPlaylistButton;
    [SerializeField]
    private Button _feedAlbumButton;
    [SerializeField]
    private Button _artistButton;
    [SerializeField]
    private Button _saveButton;
    [SerializeField]
    private Button _jsonImportButton;
    [SerializeField]
    private TextMeshProUGUI _loadingText;
    [SerializeField]
    private TextMeshProUGUI _jsonPathText;

    private SpotifyClient client;

    private List<PlaylistTrack<IPlayableItem>> _lastTracksList;
    private FullAlbum _lastFullAlbum;
    private Dictionary<string, Album> _albumsInDataBase = new Dictionary<string, Album>();
    private Dictionary<string, Album> _newAlbums = new Dictionary<string, Album>();
    private Dictionary<string, string> _playlistIdsInDataBase = new Dictionary<string, string>();
    private Dictionary<string, string> _newPlaylistIds = new Dictionary<string, string>();

    private int _newGenreForAlbums = 0;

    public void Start()
    {
        _loginManager.OnClientConnected += _loginManager_OnClientConnected;

        _searchPlaylistField.interactable = false;
        _searchAlbumField.interactable = false;
        _artistField.interactable = false;
        _genreDropdown.interactable = false;
        _feedPlaylistButton.interactable = false;
        _feedAlbumButton.interactable = false;
        _artistButton.interactable = false;
        _saveButton.interactable = false;
        _jsonImportButton.interactable = false;
    }

    public void OnDestroy()
    {
        _loginManager.OnClientConnected -= _loginManager_OnClientConnected;
    }

    private void _loginManager_OnClientConnected()
    {
        client = SpotifyService.Instance.GetSpotifyClient();

        
        _jsonImportButton.interactable = true;

        if (_feedPlaylistButton != null)
        {
            _feedPlaylistButton.onClick.AddListener(this.OnPerformPlaylistSearch);
        }

        if (_feedAlbumButton != null)
        {
            _feedAlbumButton.onClick.AddListener(this.OnPerformAlbumSearch);
        }

        if (_artistButton != null)
        {
            _artistButton.onClick.AddListener(this.TryAddGenreToArtist);
        }

        if (_saveButton != null)
        {
            _saveButton.onClick.AddListener(this.SaveUpdatedDataBase);
        }

        if (_jsonImportButton != null)
        {
            _jsonImportButton.onClick.AddListener(this.OpenFileExplorer);
        }

        _genreDropdown.ClearOptions();
        var enumValues = Enum.GetValues(typeof(Genre));
        foreach (var value in enumValues)
        {
            _genreDropdown.options.Add(new TMP_Dropdown.OptionData(value.ToString()));
        }
    }

    private void TryAddGenreToArtist()
    {
        if (_artistField != null && _genreDropdown != null)
        {
            if (_artistField.text.Equals(string.Empty))
            {
                Debug.LogWarning("There's no artist to search");
                return;
            }
            if (_genreDropdown.options[_genreDropdown.value].text.ToLowerInvariant().Equals("none"))
            {
                Debug.LogWarning("There's no genre to attribute");
                return;
            }


            string newGenre = _genreDropdown.options[_genreDropdown.value].text.ToLowerInvariant();
            string artist = _artistField.text;
            bool isArtistFound;

            foreach (Album album in _albumsInDataBase.Values)
            {
                isArtistFound = album.Artists.Any(a => a.ToLowerInvariant().Equals(artist.ToLowerInvariant()));
                if (isArtistFound)
                {
                    album.AddGenre(newGenre);
                    _newGenreForAlbums++;
                }
            }

            UpdateIHM();
        }
    }

    private void OpenFileExplorer()
    {
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Choose JSON file", "", "json", false);
        if (paths.Length == 1)
        {
            if (!string.IsNullOrEmpty(paths[0]))
            {
                _jsonPathText.text = $"JSON input path : {paths[0]}";
                LoadJSON(paths[0]);
            }
        }
    }

    private void LoadJSON(string path)
    {
        string jsonContent = File.ReadAllText(path);

        if (jsonContent != null && !jsonContent.Equals(string.Empty))
        {
            _albumsInDataBase.Clear();

            DataBase dataBase = JsonUtility.FromJson<DataBase>(jsonContent);

            if (dataBase != null)
            {
                foreach (PlaylistId playlistId in dataBase.PlaylistIdsAlreadyFetched)
                {
                    _playlistIdsInDataBase.Add(playlistId.Id, playlistId.Genre);
                }

                foreach (Album album in dataBase.Albums)
                {
                    _albumsInDataBase.Add(album.Id, album);
                }

                _searchPlaylistField.interactable = true;
                _searchAlbumField.interactable = true;
                _artistField.interactable = true;
                _genreDropdown.interactable = true;
                _feedPlaylistButton.interactable = true;
                _feedAlbumButton.interactable = true;
                _artistButton.interactable = true;
            }
        }
    }

    private void SaveUpdatedDataBase()
    {
        string path = StandaloneFileBrowser.SaveFilePanel("Save JSON File", "", "VinylStoreDataBase", "json");

        if (!string.IsNullOrEmpty(path))
        {
            SaveJsonFile(path);
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SaveJsonFile(string fileName)
    {
        DataBase updatedDataBase = new DataBase();

        AddNewPlaylistIds(updatedDataBase);
        AddNewAlbums(updatedDataBase);

        string jsonUpdated = JsonUtility.ToJson(updatedDataBase);
        File.WriteAllText(fileName, jsonUpdated);
        _loadingText.text = $"JSON file saved at {fileName}";
        Debug.Log($"JSON file saved at {fileName}");
    }

    private void AddNewAlbums(DataBase updatedDataBase)
    {
        int totalAlbumsCount = _newAlbums.Count + _albumsInDataBase.Count;

        Album[] updatedAlbums = new Album[totalAlbumsCount];

        int i = 0;
        foreach (Album album in _newAlbums.Values)
        {
            updatedAlbums[i] = album;
            i++;
        }
        foreach (Album album in _albumsInDataBase.Values)
        {
            updatedAlbums[i] = album;
            i++;
        }

        updatedDataBase.Albums = updatedAlbums;
    }

    private void AddNewPlaylistIds(DataBase updatedDataBase)
    {
        int totalPlaylistIdsCount = _newPlaylistIds.Count + _playlistIdsInDataBase.Count;

        PlaylistId[] updatedPlaylistIds = new PlaylistId[totalPlaylistIdsCount];

        int i = 0;
        foreach (KeyValuePair<string, string> pair in _newPlaylistIds)
        {
            PlaylistId item = new PlaylistId();
            item.Id = pair.Key;
            item.Genre = pair.Value;
            updatedPlaylistIds[i] = item;
            i++;
        }
        foreach (KeyValuePair<string, string> pair in _playlistIdsInDataBase)
        {
            PlaylistId item = new PlaylistId();
            item.Id = pair.Key;
            item.Genre = pair.Value;
            updatedPlaylistIds[i] = item;
            i++;
        }

        updatedDataBase.PlaylistIdsAlreadyFetched = updatedPlaylistIds;
    }

    private async void OnPerformPlaylistSearch()
    {
        if (client != null && _searchPlaylistField != null && _genreDropdown != null)
        {
            if (_searchPlaylistField.text.Equals(string.Empty))
            {
                Debug.LogWarning("There's no playlist ID to search");
                return;
            }
            if (_genreDropdown.options[_genreDropdown.value].text.ToLowerInvariant().Equals("none"))
            {
                Debug.LogWarning("There's no genre to attribute");
                return;
            }
            if (_playlistIdsInDataBase.ContainsKey(_searchPlaylistField.text))
            {
                Debug.LogWarning("This playlist has already been fetch");
                return;
            }
            

            string query = _searchPlaylistField.text;
            string genre = _genreDropdown.options[_genreDropdown.value].text.ToLowerInvariant();

            _newPlaylistIds.Add(query, genre);

            _lastTracksList = await GetAllTracks(query);

            CreateAlbumSelection(genre);

            UpdateIHM();
        }
    }

    private async void OnPerformAlbumSearch()
    {
        if (client != null && _searchAlbumField != null && _genreDropdown != null)
        {
            if (_searchAlbumField.text.Equals(string.Empty))
            {
                Debug.LogWarning("There's no album ID to search");
                return;
            }
            if (_genreDropdown.options[_genreDropdown.value].text.ToLowerInvariant().Equals("none"))
            {
                Debug.LogWarning("There's no genre to attribute");
                return;
            }
            if (_albumsInDataBase.ContainsKey(_searchAlbumField.text))
            {
                _albumsInDataBase[_searchAlbumField.text].AddGenre(_genreDropdown.options[_genreDropdown.value].text);
                return;
            }


            string query = _searchAlbumField.text;
            string genre = _genreDropdown.options[_genreDropdown.value].text.ToLowerInvariant();

            _lastFullAlbum = await client.Albums.Get(query);

            CreateAndAddNewAlbum(genre);

            UpdateIHM();
        }
    }

    private void UpdateIHM()
    {
        _loadingText.text = $"{_newAlbums.Count} new albums to save in data base and {_newGenreForAlbums} new genres for Albums.";

        if (_newAlbums.Count > 0 || _newGenreForAlbums > 0)
        {
            _saveButton.interactable = true;
        }
    }

    private void CreateAlbumSelection(string genre)
    { 
        foreach (PlaylistTrack<IPlayableItem> item in _lastTracksList)
        {
            CreateAndAddNewAlbum(item.Track as FullTrack, genre);
        }
    }

    private void CreateAndAddNewAlbum(FullTrack track, string genre)
    {
        if (track.Album.AlbumType.Equals("album"))
        {
            // If album is already in the dataBase
            if (_albumsInDataBase.ContainsKey(track.Album.Id))
            {
                _albumsInDataBase[track.Album.Id].AddGenre(genre);
                _newGenreForAlbums++;
                return;
            }

            // if album has already being fecthed
            if (_newAlbums.ContainsKey(track.Album.Id))
            {
                return;
            }

            Album newAlbum = new Album();
            newAlbum.Artists = ArtistsToString(track.Artists);
            newAlbum.AddGenre(genre);
            newAlbum.Href = track.Album.Href;
            newAlbum.Id = track.Album.Id;
            newAlbum.ImagesUrls = ImagesToString(track.Album.Images);
            newAlbum.Name = track.Album.Name;
            newAlbum.ReleaseDate = track.Album.ReleaseDate;
            newAlbum.Uri = track.Album.Uri;

            _newAlbums.Add(newAlbum.Id, newAlbum);
        }
    }

    private void CreateAndAddNewAlbum(string genre)
    {
        if (_lastFullAlbum != null)
        {
            // if album has already being fecthed
            if (_newAlbums.ContainsKey(_lastFullAlbum.Id))
            {
                return;
            }

            if (_lastFullAlbum.AlbumType.Equals("album"))
            {
                Album newAlbum = new Album();
                newAlbum.Artists = ArtistsToString(_lastFullAlbum.Artists);
                newAlbum.AddGenre(genre);
                newAlbum.Href = _lastFullAlbum.Href;
                newAlbum.Id = _lastFullAlbum.Id;
                newAlbum.ImagesUrls = ImagesToString(_lastFullAlbum.Images);
                newAlbum.Name = _lastFullAlbum.Name;
                newAlbum.ReleaseDate = _lastFullAlbum.ReleaseDate;
                newAlbum.Uri = _lastFullAlbum.Uri;

                _newAlbums.Add(newAlbum.Id, newAlbum);
            }
        }
    }

    private async Task<List<PlaylistTrack<IPlayableItem>>> GetAllTracks(string playlistId)
    {
        List<PlaylistTrack<IPlayableItem>> allTracks = new List<PlaylistTrack<IPlayableItem>>();

        Paging<PlaylistTrack<IPlayableItem>> pUserTracks = await client.Playlists.GetItems(playlistId, new PlaylistGetItemsRequest { Offset = 0 });
        allTracks.AddRange(pUserTracks.Items);

        int currentOffset = 0;
        int pagingAmount = 100;

        while (currentOffset <= pUserTracks.Total.Value)
        {
            pUserTracks = await client.Playlists.GetItems(playlistId, new PlaylistGetItemsRequest { Offset = currentOffset + pagingAmount });
            allTracks.AddRange(pUserTracks.Items);

            // Increment by amount + 1 for next segment of tracks
            currentOffset += pagingAmount + 1;
        }

        return allTracks;
    }

    private string[] ArtistsToString(List<SimpleArtist> artists)
    {
        int artistsCount = artists.Count;
        string[] artistsString = new string[artistsCount];

        int i = 0;
        foreach (SimpleArtist artist in artists)
        {
            artistsString[i] = artist.Name;
            i++;
        }

        return artistsString;
    }

    private string[] ImagesToString(List<SpotifyAPI.Web.Image> images)
    {
        int imagesCount = images.Count;
        string[] imagesString = new string[imagesCount];

        int i = 0;
        foreach (SpotifyAPI.Web.Image image in images)
        {
            imagesString[i] = image.Url;
            i++;
        }

        return imagesString;
    }
}
