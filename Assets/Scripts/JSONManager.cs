using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Threading.Tasks;

public class JSONManager : MonoBehaviour
{
    [SerializeField]
    private LoginManager _loginManager;

    [SerializeField]
    private TextAsset jsonFile;

    [SerializeField]
    private TMP_InputField _searchField;
    [SerializeField]
    private TMP_InputField _genreField;
    [SerializeField]
    private Button _feedButton;
    [SerializeField]
    private Button _saveButton;
    [SerializeField]
    private TextMeshProUGUI _loadingText;

    private SpotifyClient client;

    private List<PlaylistTrack<IPlayableItem>> _lastTracksList;
    private Dictionary<string, Album> _albumsInDataBase = new Dictionary<string, Album>();
    private Dictionary<string, Album> _newAlbums = new Dictionary<string, Album>();
    private HashSet<string> _playlistIdsInDataBase = new HashSet<string>();
    private HashSet<string> _newPlaylistIds = new HashSet<string>();

    private int newGenreForAlbums = 0;

    public void Start()
    {
        _loginManager.OnClientConnected += _loginManager_OnClientConnected;

        _searchField.interactable = false;
        _genreField.interactable = false;
        _feedButton.interactable = false;
        _saveButton.interactable = false;
    }

    public void OnDestroy()
    {
        _loginManager.OnClientConnected -= _loginManager_OnClientConnected;
    }

    private void _loginManager_OnClientConnected()
    {
        LoadJSON();

        client = SpotifyService.Instance.GetSpotifyClient();

        _searchField.interactable = true;
        _genreField.interactable = true;
        _feedButton.interactable = true;

        if (_feedButton != null)
        {
            _feedButton.onClick.AddListener(this.OnPerformSearch);
        }

        if (_saveButton != null)
        {
            _feedButton.onClick.AddListener(this.SaveUpdatedDataBase);
        }
    }

    

    private void LoadJSON()
    {
        _albumsInDataBase.Clear();

        DataBase dataBase = JsonUtility.FromJson<DataBase>(jsonFile.text);

        foreach (string playlistId in dataBase.PlaylistIdsAlreadyFetched)
        {
            _playlistIdsInDataBase.Add(playlistId);
        }

        foreach (Album album in dataBase.Albums)
        {
            _albumsInDataBase.Add(album.Id, album);
        }
    }

    private void WriteUpdatedJSON(DataBase updatedDataBase)
    {
        string jsonUpdated = JsonUtility.ToJson(updatedDataBase);
        File.WriteAllText(Application.dataPath + "/Data/ma_base_de_donnees.json", jsonUpdated);
    }

    private void SaveUpdatedDataBase()
    {
        DataBase updatedDataBase = new DataBase();

        AddNewPlaylistIds(updatedDataBase);
        AddNewAlbums(updatedDataBase);
        WriteUpdatedJSON(updatedDataBase);
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

        string[] updatedPlaylistIds = new string[totalPlaylistIdsCount];

        int i = 0;
        foreach (string playlistId in _newPlaylistIds)
        {
            updatedPlaylistIds[i] = playlistId;
            i++;
        }
        foreach (string playlistId in _playlistIdsInDataBase)
        {
            updatedPlaylistIds[i] = playlistId;
            i++;
        }

        updatedDataBase.PlaylistIdsAlreadyFetched = updatedPlaylistIds;
    }

    private async void OnPerformSearch()
    {
        if (client != null && _searchField != null && _genreField != null)
        {
            if (_searchField.text.Equals(string.Empty))
            {
                Debug.LogWarning("There's no playlist ID to search");
                return;
            }
            if (_genreField.text.Equals(string.Empty))
            {
                Debug.LogWarning("There's no genre to attribute");
                return;
            }
            if (_playlistIdsInDataBase.Contains(_searchField.text))
            {
                Debug.LogWarning("This playlist has already been fetch");
                return;
            }
            

            string query = _searchField.text;
            string genre = _genreField.text;

            _newPlaylistIds.Add(query);

            _lastTracksList = await GetAllTracks(query);

            CreateAlbumSelection(genre);

            UpdateIHM();
        }
    }

    private void UpdateIHM()
    {
        _loadingText.text = $"{_newAlbums.Count} new albums to save in data base and {newGenreForAlbums} new genres for Albums.";

        if (_newAlbums.Count > 0 || newGenreForAlbums > 0)
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
                newGenreForAlbums++;
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
