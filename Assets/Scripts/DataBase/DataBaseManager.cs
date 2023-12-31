﻿using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

public class DataBaseManager
{
    public delegate void DataBaseLoadedDelegate(bool isCorrectlyLoaded);
    public event DataBaseLoadedDelegate DataBaseLoaded;

    public delegate void InputDataBaseLoadedDelegate(DataBaseInput dataBaseInput);
    public event InputDataBaseLoadedDelegate InputDataBaseLoaded;

    public delegate void FeedPerformedDelegate();
    public event FeedPerformedDelegate FeedPerformed;

    public delegate void NewDataBaseSavedDelegate(bool isCorrectlySaved);
    public event NewDataBaseSavedDelegate NewDataBaseSaved;

    public delegate void PerformSearchProgressUpdatedDelegate(float progress);
    public event PerformSearchProgressUpdatedDelegate PerformSearchProgressUpdated;
    

    public DataBase DataBase { get; private set; }

    private SpotifyClient _client;

    private Dictionary<string, Album> _albumsInDataBaseByID;
    private Dictionary<string, Artist> _artistsInDataBaseByID;
    private Dictionary<string, string> _genresInDataBaseByPlaylistID;
    private Dictionary<string, Album> _newAlbumsByID;
    private Dictionary<string, Artist> _newArtistsByID;
    private Dictionary<string, string> _newGenresByPlaylistID;

    private string _jsonDataBasePath;
    private float _performSearchByDataBaseInputProgress;
    private const int _safeAPIWaitInMs = 1000;

    public DataBaseManager()
    {
        _client = SpotifyService.Instance.GetSpotifyClient();

        _performSearchByDataBaseInputProgress = 0;
        _jsonDataBasePath = string.Empty;

        _albumsInDataBaseByID = new Dictionary<string, Album>();
        _artistsInDataBaseByID = new Dictionary<string, Artist>();
        _genresInDataBaseByPlaylistID = new Dictionary<string, string>();
        _newAlbumsByID = new Dictionary<string, Album>();
        _newArtistsByID = new Dictionary<string, Artist>();
        _newGenresByPlaylistID = new Dictionary<string, string>();
    }

    public int GetNewAlbumsAddedCount() 
    {
        return _newAlbumsByID.Count;
    }

    public int GetNewArtistsAddedCount()
    {
        return _newArtistsByID.Count;
    }

    #region LOAD_SAVE_JSON_METHODS

    public void LoadJSONDataBase(string jsonPath)
    {
        _jsonDataBasePath = jsonPath;

        string jsonContent = string.Empty;

        try
        {
            jsonContent = File.ReadAllText(jsonPath);
        }
        catch (Exception)
        {
            DataBaseLoaded?.Invoke(false);
        }
        

        if (!string.IsNullOrEmpty(jsonContent))
        {
            _albumsInDataBaseByID.Clear();
            _artistsInDataBaseByID.Clear();

            DataBase = JsonUtility.FromJson<DataBase>(jsonContent);

            if (DataBase != null)
            {
                foreach (PlaylistId playlistId in DataBase.PlaylistIdsAlreadyFetched)
                {
                    _genresInDataBaseByPlaylistID.Add(playlistId.Id, playlistId.Genre);
                }

                foreach (Album album in DataBase.Albums)
                {
                    _albumsInDataBaseByID.Add(album.Id, album);
                }

                foreach (Artist artist in DataBase.Artists)
                {
                    _artistsInDataBaseByID.Add(artist.Id, artist);
                }

                DataBaseLoaded?.Invoke(true);
            }
            else
            {
                DataBaseLoaded?.Invoke(false);
            }
        }
        else
        {
            DataBase = new DataBase();
            DataBaseLoaded?.Invoke(true);
        }
    }

    public void LoadJSONDataBaseInput(string jsonPath)
    {
        string jsonContent = string.Empty;

        try
        {
            jsonContent = File.ReadAllText(jsonPath);
        }
        catch (Exception)
        {
            InputDataBaseLoaded?.Invoke(null);
        }


        if (!string.IsNullOrEmpty(jsonContent))
        {
            DataBaseInput dataBaseInput = JsonUtility.FromJson<DataBaseInput>(jsonContent);

            if (dataBaseInput != null)
            {
                InputDataBaseLoaded?.Invoke(dataBaseInput);
            }
            else
            {
                InputDataBaseLoaded?.Invoke(null);
            }
        }
    }

    public void SaveJSONDataBase(string jsonPath, bool createBackUp = true)
    {
        if (createBackUp && !string.IsNullOrEmpty(_jsonDataBasePath))
        {
            try
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(_jsonDataBasePath);
                string folderPath = Path.GetDirectoryName(_jsonDataBasePath);
                string timestamp = "Backup";
                string backupPath = $"{fileNameWithoutExtension}_{timestamp}.json";
                string newPath = Path.Combine(folderPath, backupPath);

                // Copier le fichier original vers le fichier de sauvegarde
                File.Copy(_jsonDataBasePath, newPath, true);
            }
            catch (Exception e)
            {
                NewDataBaseSaved?.Invoke(false);
                Debug.LogError(e);
            }
        }

        DataBase updatedDataBase = new DataBase();

        AddNewPlaylistIdsToDataBase(updatedDataBase);
        AddNewAlbumsToDataBase(updatedDataBase);
        AddNewArtistsToDataBase(updatedDataBase);

        string jsonUpdated = JsonUtility.ToJson(updatedDataBase);

        if (!string.IsNullOrEmpty(jsonUpdated))
        {
            try
            {
                File.WriteAllText(jsonPath, jsonUpdated);
                NewDataBaseSaved?.Invoke(true);
            }
            catch (Exception e)
            {
                NewDataBaseSaved?.Invoke(false);
                Debug.LogError(e);
            }
        }
    }

    private void AddNewAlbumsToDataBase(DataBase updatedDataBase)
    {
        int totalAlbumsCount = _newAlbumsByID.Count + _albumsInDataBaseByID.Count;

        Album[] updatedAlbums = new Album[totalAlbumsCount];

        int i = 0;
        foreach (Album album in _newAlbumsByID.Values)
        {
            updatedAlbums[i] = album;
            i++;
        }
        foreach (Album album in _albumsInDataBaseByID.Values)
        {
            updatedAlbums[i] = album;
            i++;
        }

        updatedDataBase.Albums = updatedAlbums;
    }

    private void AddNewArtistsToDataBase(DataBase updatedDataBase)
    {
        int totalArtistsCount = _newArtistsByID.Count + _artistsInDataBaseByID.Count;

        Artist[] updatedArtists = new Artist[totalArtistsCount];

        int i = 0;
        foreach (Artist Artist in _newArtistsByID.Values)
        {
            updatedArtists[i] = Artist;
            i++;
        }
        foreach (Artist Artist in _artistsInDataBaseByID.Values)
        {
            updatedArtists[i] = Artist;
            i++;
        }

        updatedDataBase.Artists = updatedArtists;
    }

    private void AddNewPlaylistIdsToDataBase(DataBase updatedDataBase)
    {
        int totalPlaylistIdsCount = _newGenresByPlaylistID.Count + _genresInDataBaseByPlaylistID.Count;

        PlaylistId[] updatedPlaylistIds = new PlaylistId[totalPlaylistIdsCount];

        int i = 0;
        foreach (KeyValuePair<string, string> pair in _newGenresByPlaylistID)
        {
            PlaylistId item = new PlaylistId();
            item.Id = pair.Key;
            item.Genre = pair.Value;
            updatedPlaylistIds[i] = item;
            i++;
        }
        foreach (KeyValuePair<string, string> pair in _genresInDataBaseByPlaylistID)
        {
            PlaylistId item = new PlaylistId();
            item.Id = pair.Key;
            item.Genre = pair.Value;
            updatedPlaylistIds[i] = item;
            i++;
        }

        updatedDataBase.PlaylistIdsAlreadyFetched = updatedPlaylistIds;
    }

    #endregion LOAD_SAVE_JSON_METHODS

    #region SEARCHING_SPOTIFY_API_METHODS

    public async void PerformSearchByDataBaseInput(DataBaseInput dataBaseInput)
    {
        float totalOperations = dataBaseInput.playlists.Length + dataBaseInput.albums.Length + dataBaseInput.artists.Length;
        float currentOperation = 0f;
        _performSearchByDataBaseInputProgress = 0;
        bool isRequestPerformed = false;

        foreach (ArtistInput artistInput in dataBaseInput.artists)
        {
            isRequestPerformed = TryAddGenreByArtistID(artistInput);

            // Wait for 2 seconds to not raise limit of API request
            if (isRequestPerformed)
            {
                await Task.Delay(_safeAPIWaitInMs);
            }

            currentOperation++;
            _performSearchByDataBaseInputProgress = (currentOperation / totalOperations) * 100;
            PerformSearchProgressUpdated?.Invoke(_performSearchByDataBaseInputProgress);
        }

        foreach (AlbumInput albumInput in dataBaseInput.albums)
        {
            isRequestPerformed = await PerformAlbumSearchByID(albumInput);

            // Wait for 2 seconds to not raise limit of API request
            if (isRequestPerformed)
            {
                await Task.Delay(_safeAPIWaitInMs);
            }

            currentOperation++;
            _performSearchByDataBaseInputProgress = (currentOperation / totalOperations) * 100;
            PerformSearchProgressUpdated?.Invoke(_performSearchByDataBaseInputProgress);
        }

        foreach (PlaylistInput playlistInput in dataBaseInput.playlists)
        {
            isRequestPerformed = await PerformPlaylistSearchByID(playlistInput);

            // Wait for 2 seconds to not raise limit of API request
            if (isRequestPerformed)
            {
                await Task.Delay(_safeAPIWaitInMs);
            }

            currentOperation++;
            _performSearchByDataBaseInputProgress = (currentOperation / totalOperations) * 100;
            PerformSearchProgressUpdated?.Invoke(_performSearchByDataBaseInputProgress);
        }
        FeedPerformed?.Invoke();
    }

    private async Task<bool> PerformPlaylistSearchByID(PlaylistInput playlistInput)
    {
        if (_client != null && playlistInput != null)
        {
            if (playlistInput.playlistId.Equals(string.Empty))
            {
                Debug.LogWarning("There's no playlist ID to search");
                return false;
            }
            if (playlistInput.genre.ToLowerInvariant().Equals("none"))
            {
                Debug.LogWarning("There's no genre to attribute");
                return false;
            }
            if (_genresInDataBaseByPlaylistID.ContainsKey(playlistInput.playlistId))
            {
                Debug.LogWarning("This playlist has already been fetch");
                return false;
            }

            List<PlaylistTrack<IPlayableItem>> tracksList = await GetAllTracksByPlaylistID(playlistInput.playlistId);

            if (tracksList != null)
            {
                _newGenresByPlaylistID.Add(playlistInput.playlistId, playlistInput.genre);

                foreach (PlaylistTrack<IPlayableItem> item in tracksList)
                {
                    CreateAndAddNewAlbumByFullTrack(item.Track as FullTrack, playlistInput.genre);
                }
                return true;
            }
        }
        return false;
    }

    private async Task<bool> PerformAlbumSearchByID(AlbumInput albumInput)
    {
        if (_client != null && albumInput != null)
        {
            if (albumInput.albumId.Equals(string.Empty))
            {
                Debug.LogWarning("There's no album ID to search");
                return false;
            }
            if (albumInput.genre.ToLowerInvariant().Equals("none"))
            {
                Debug.LogWarning("There's no genre to attribute");
                return false;
            }


            FullAlbum fullAlbum = await _client.Albums.Get(albumInput.albumId);

            if (fullAlbum != null)
            {
                bool hasToPerformArtistTopTracksSearch = true;

                if (hasToPerformArtistTopTracksSearch)
                {
                    foreach (SimpleArtist artist in fullAlbum.Artists)
                    {
                        ArtistInput artistInput = new ArtistInput();
                        artistInput.artistId = artist.Id;
                        artistInput.genre = albumInput.genre;
                        PerformArtistTopTracksSearchByID(artistInput);
                    }
                    return true;
                }

                CreateAndAddNewAlbumByFullAlbum(fullAlbum, albumInput.genre);
            }
            return true;
        }
        return false;
    }

    private async void PerformArtistTopTracksSearchByID(ArtistInput artistInput)
    {
        if (_client != null && artistInput != null)
        {
            if (artistInput.artistId.Equals(string.Empty))
            {
                Debug.LogWarning("There's no artist ID to search");
                return;
            }
            if (artistInput.genre.ToLowerInvariant().Equals("none"))
            {
                Debug.LogWarning("There's no genre to attribute");
                return;
            }

            ArtistsTopTracksResponse topTracksResponse = await _client.Artists.GetTopTracks(artistInput.artistId, new ArtistsTopTracksRequest("FR"));

            if (topTracksResponse != null)
            {
                foreach (FullTrack item in topTracksResponse.Tracks)
                {
                    CreateAndAddNewAlbumByFullTrack(item, artistInput.genre);
                }
            }
            return;
        }
        return;
    }

    private bool TryAddGenreByArtistID(ArtistInput artistInput)
    {
        if (_client != null && artistInput != null)
        {
            if (artistInput.artistId.Equals(string.Empty))
            {
                Debug.LogWarning("There's no artist ID to search");
                return false;
            }
            if (artistInput.genre.ToLowerInvariant().Equals("none"))
            {
                Debug.LogWarning("There's no genre to attribute");
                return false;
            }

            TryUpdateArtistGenre(artistInput);

            bool hasToPerformArtistTopTracksSearch = true;

            if (hasToPerformArtistTopTracksSearch)
            {
                PerformArtistTopTracksSearchByID(artistInput);
                return true;
            }
            return false;
        }
        return false;
    }

    #endregion SEARCHING_SPOTIFY_API_METHODS

    #region UTILS_SPOTIFY

    private void CreateAndAddNewAlbumByFullTrack(FullTrack track, string genre)
    {
        if (track.Album.AlbumType.Equals("album"))
        {
            // If album is already in the dataBase or new albums
            if (_albumsInDataBaseByID.ContainsKey(track.Album.Id) || _newAlbumsByID.ContainsKey(track.Album.Id))
            {
                foreach (SimpleArtist artist in track.Album.Artists)
                {
                    ArtistInput artistInput = new ArtistInput();
                    artistInput.artistId = artist.Id;
                    artistInput.genre = genre;
                    TryUpdateArtistGenre(artistInput);
                }

                return;
            }

            Album newAlbum = new Album();
            newAlbum.ArtistsName = ArtistsNameToString(track.Artists);
            newAlbum.ArtistsId = ArtistsIdToString(track.Artists);
            newAlbum.Href = track.Album.Href;
            newAlbum.Id = track.Album.Id;
            newAlbum.ImagesUrls = ImagesToString(track.Album.Images);
            newAlbum.Name = track.Album.Name;
            newAlbum.ReleaseDate = track.Album.ReleaseDate;
            newAlbum.Uri = track.Album.Uri;

            foreach (string artistId in newAlbum.ArtistsId)
            {
                ArtistInput artistInput = new ArtistInput();
                artistInput.artistId = artistId;
                artistInput.genre = genre;
                TryUpdateArtistGenre(artistInput);
            }

            _newAlbumsByID.Add(newAlbum.Id, newAlbum);
        }
    }

    private void CreateAndAddNewAlbumByFullAlbum(FullAlbum fullAlbum, string genre)
    {
        // If album is already in the dataBase or new albums
        if (_albumsInDataBaseByID.ContainsKey(fullAlbum.Id) || _newAlbumsByID.ContainsKey(fullAlbum.Id))
        {
            foreach (SimpleArtist artist in fullAlbum.Artists)
            {
                ArtistInput artistInput = new ArtistInput();
                artistInput.artistId = artist.Id;
                artistInput.genre = genre;
                TryUpdateArtistGenre(artistInput);
            }

            return;
        }

        if (fullAlbum.AlbumType.Equals("album"))
        {
            Album newAlbum = new Album();
            newAlbum.ArtistsName = ArtistsNameToString(fullAlbum.Artists);
            newAlbum.ArtistsId = ArtistsIdToString(fullAlbum.Artists);
            newAlbum.Href = fullAlbum.Href;
            newAlbum.Id = fullAlbum.Id;
            newAlbum.ImagesUrls = ImagesToString(fullAlbum.Images);
            newAlbum.Name = fullAlbum.Name;
            newAlbum.ReleaseDate = fullAlbum.ReleaseDate;
            newAlbum.Uri = fullAlbum.Uri;

            foreach (string artistId in newAlbum.ArtistsId)
            {
                ArtistInput artistInput = new ArtistInput();
                artistInput.artistId = artistId;
                artistInput.genre = genre;
                TryUpdateArtistGenre(artistInput);
            }

            _newAlbumsByID.Add(newAlbum.Id, newAlbum);
        }
    }

    private async Task<List<PlaylistTrack<IPlayableItem>>> GetAllTracksByPlaylistID(string playlistId)
    {
        if (_client != null)
        {
            List<PlaylistTrack<IPlayableItem>> allTracks = new List<PlaylistTrack<IPlayableItem>>();

            Paging<PlaylistTrack<IPlayableItem>> pUserTracks = await _client.Playlists.GetItems(playlistId, new PlaylistGetItemsRequest { Offset = 0 });
            allTracks.AddRange(pUserTracks.Items);

            int currentOffset = 0;
            int pagingAmount = 100;

            while (currentOffset <= pUserTracks.Total.Value)
            {
                pUserTracks = await _client.Playlists.GetItems(playlistId, new PlaylistGetItemsRequest { Offset = currentOffset + pagingAmount });
                allTracks.AddRange(pUserTracks.Items);

                // Increment by amount + 1 for next segment of tracks
                currentOffset += pagingAmount + 1;
            }

            return allTracks;
        }

        return null;
    }

    private string[] ArtistsNameToString(List<SimpleArtist> artists)
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

    private string[] ArtistsIdToString(List<SimpleArtist> artists)
    {
        int artistsCount = artists.Count;
        string[] artistsString = new string[artistsCount];

        int i = 0;
        foreach (SimpleArtist artist in artists)
        {
            artistsString[i] = artist.Id;
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

    private void TryUpdateArtistGenre(ArtistInput artistInput)
    {
        if (_artistsInDataBaseByID.ContainsKey(artistInput.artistId))
        {
            _artistsInDataBaseByID[artistInput.artistId].TryAddGenre(artistInput.genre);
        }
        else if (_newArtistsByID.ContainsKey(artistInput.artistId))
        {
            _newArtistsByID[artistInput.artistId].TryAddGenre(artistInput.genre);
        }
        else
        {
            Artist newArtist = new Artist();
            newArtist.Id = artistInput.artistId;
            newArtist.TryAddGenre(artistInput.genre);
            _newArtistsByID.Add(newArtist.Id, newArtist);
        }
    }

    #endregion UTILS_SPOTIFY
}
