﻿using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Threading.Tasks;

public class DataBaseManager
{
    public DataBase DataBase { get; private set; }
    public int NewGenreAddedToArtistsCount { get; private set; }
    public float PerformSearchByDataBaseInputProgress { get; private set; }

    private SpotifyClient _client;
    private Dictionary<string, Album> _albumsInDataBaseByID;
    private Dictionary<string, string> _genresInDataBaseByPlaylistID;

    private Dictionary<string, Album> _newAlbumsByID;
    private Dictionary<string, string> _newGenresByPlaylistID;

    private string _jsonDataBasePath;
    private const int _safeAPIWaitInMs = 2000;

    public DataBaseManager()
    {
        _client = SpotifyService.Instance.GetSpotifyClient();

        NewGenreAddedToArtistsCount = 0;
        PerformSearchByDataBaseInputProgress = 0;
        _jsonDataBasePath = string.Empty;

        _albumsInDataBaseByID = new Dictionary<string, Album>();
        _genresInDataBaseByPlaylistID = new Dictionary<string, string>();
        _newAlbumsByID = new Dictionary<string, Album>();
        _newGenresByPlaylistID = new Dictionary<string, string>();
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
            // TODO JULIEN : Trigger WRONG
        }
        

        if (string.IsNullOrEmpty(jsonContent))
        {
            _albumsInDataBaseByID.Clear();

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

                // TODO JULIEN : Trigger dataBase loaded
            }
            else
            {
                // TODO JULIEN : Trigger WRONG
            }
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
            // TODO JULIEN : Trigger WRONG
        }


        if (string.IsNullOrEmpty(jsonContent))
        {
            DataBaseInput dataBaseInput = JsonUtility.FromJson<DataBaseInput>(jsonContent);

            if (dataBaseInput != null)
            {
                PerformSearchByDataBaseInput(dataBaseInput);
                // TODO JULIEN : Trigger dataBase loaded
            }
            else
            {
                // TODO JULIEN : Trigger WRONG
            }
        }
    }

    public void SaveJSONDataBase(string jsonPath, bool createBackUp = true)
    {
        if (createBackUp && string.IsNullOrEmpty(_jsonDataBasePath))
        {
            try
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(_jsonDataBasePath);

                string timestamp = "Backup";
                string backupPath = $"{fileNameWithoutExtension}_{timestamp}.json";

                // Copier le fichier original vers le fichier de sauvegarde
                File.Copy(_jsonDataBasePath, backupPath);
            }
            catch (Exception e)
            {
                // TODO JULIEN : Trigger Wrong
            }
        }

        DataBase updatedDataBase = new DataBase();

        AddNewPlaylistIdsToDataBase(updatedDataBase);
        AddNewAlbumsToDataBase(updatedDataBase);

        string jsonUpdated = JsonUtility.ToJson(updatedDataBase);

        if (!string.IsNullOrEmpty(jsonUpdated))
        {
            try
            {
                File.WriteAllText(jsonPath, jsonUpdated);
                // TODO JULIEN : TRIGGER OK
            }
            catch (Exception e)
            {
                // TODO JULIEN : TRIGGER WRONG
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

    private async void PerformPlaylistSearchByID(PlaylistInput playlistInput)
    {
        if (_client != null && playlistInput != null)
        {
            if (playlistInput.playlistId.Equals(string.Empty))
            {
                Debug.LogWarning("There's no playlist ID to search");
                return;
            }
            if (playlistInput.genre.ToLowerInvariant().Equals("none"))
            {
                Debug.LogWarning("There's no genre to attribute");
                return;
            }
            if (_genresInDataBaseByPlaylistID.ContainsKey(playlistInput.playlistId))
            {
                Debug.LogWarning("This playlist has already been fetch");
                return;
            }

            List<PlaylistTrack<IPlayableItem>> tracksList = await GetAllTracksByPlaylistID(playlistInput.playlistId);

            if (tracksList != null)
            {
                _newGenresByPlaylistID.Add(playlistInput.playlistId, playlistInput.genre);

                foreach (PlaylistTrack<IPlayableItem> item in tracksList)
                {
                    CreateAndAddNewAlbumByFullTrack(item.Track as FullTrack, playlistInput.genre);
                }
            }
        }
    }

    private async void PerformAlbumSearchByID(AlbumInput albumInput)
    {
        if (_client != null && albumInput != null)
        {
            if (albumInput.albumId.Equals(string.Empty))
            {
                Debug.LogWarning("There's no album ID to search");
                return;
            }
            if (albumInput.genre.ToLowerInvariant().Equals("none"))
            {
                Debug.LogWarning("There's no genre to attribute");
                return;
            }
            if (_albumsInDataBaseByID.ContainsKey(albumInput.albumId))
            {
                if (_albumsInDataBaseByID[albumInput.albumId].TryAddGenre(albumInput.genre))
                {
                    NewGenreAddedToArtistsCount++;
                }
                return;
            }

            FullAlbum fullAlbum = await _client.Albums.Get(albumInput.albumId);

            if (fullAlbum != null)
            {
                CreateAndAddNewAlbumByFullAlbum(fullAlbum, albumInput.genre);
            }
        }
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
        }
    }

    private async void PerformSearchByDataBaseInput(DataBaseInput dataBaseInput)
    {
        float totalOperations = dataBaseInput.playlists.Length + dataBaseInput.albums.Length + dataBaseInput.artists.Length;
        float currentOperation = 0f;


        foreach (PlaylistInput playlistInput in dataBaseInput.playlists)
        {
            PerformPlaylistSearchByID(playlistInput);

            // Wait for 2 seconds to not raise limit of API request
            await Task.Delay(_safeAPIWaitInMs);
            currentOperation++;
            PerformSearchByDataBaseInputProgress = (currentOperation / totalOperations) * 100;
        }

        foreach (AlbumInput albumInput in dataBaseInput.albums)
        {
            PerformAlbumSearchByID(albumInput);

            // Wait for 2 seconds to not raise limit of API request
            await Task.Delay(_safeAPIWaitInMs);
            currentOperation++;
            PerformSearchByDataBaseInputProgress = (currentOperation / totalOperations) * 100;
        }

        foreach (ArtistInput artistInput in dataBaseInput.artists)
        {
            PerformArtistTopTracksSearchByID(artistInput);

            // Wait for 2 seconds to not raise limit of API request
            await Task.Delay(_safeAPIWaitInMs);
            currentOperation++;
            PerformSearchByDataBaseInputProgress = (currentOperation / totalOperations) * 100;
        }

        // TODO JULIEN : Trigger Finished
    }

    #endregion SEARCHING_SPOTIFY_API_METHODS

    #region UTILS_SPOTIFY

    private void CreateAndAddNewAlbumByFullTrack(FullTrack track, string genre)
    {
        if (track.Album.AlbumType.Equals("album"))
        {
            // If album is already in the dataBase
            if (_albumsInDataBaseByID.ContainsKey(track.Album.Id))
            {
                if (_albumsInDataBaseByID[track.Album.Id].TryAddGenre(genre))
                {
                    NewGenreAddedToArtistsCount++;
                }
                return;
            }

            // if album has already been fetch
            if (_newAlbumsByID.ContainsKey(track.Album.Id))
            {
                if (_newAlbumsByID[track.Album.Id].TryAddGenre(genre))
                {
                    NewGenreAddedToArtistsCount++;
                }
                return;
            }

            Album newAlbum = new Album();
            newAlbum.Artists = ArtistsToString(track.Artists);
            newAlbum.TryAddGenre(genre);
            newAlbum.Href = track.Album.Href;
            newAlbum.Id = track.Album.Id;
            newAlbum.ImagesUrls = ImagesToString(track.Album.Images);
            newAlbum.Name = track.Album.Name;
            newAlbum.ReleaseDate = track.Album.ReleaseDate;
            newAlbum.Uri = track.Album.Uri;

            _newAlbumsByID.Add(newAlbum.Id, newAlbum);
        }
    }

    private void CreateAndAddNewAlbumByFullAlbum(FullAlbum fullAlbum, string genre)
    {
        // if album has already been fetch
        if (_newAlbumsByID.ContainsKey(fullAlbum.Id))
        {
            if (_newAlbumsByID[fullAlbum.Id].TryAddGenre(genre))
            {
                NewGenreAddedToArtistsCount++;
            }
            return;
        }

        if (fullAlbum.AlbumType.Equals("album"))
        {
            Album newAlbum = new Album();
            newAlbum.Artists = ArtistsToString(fullAlbum.Artists);
            newAlbum.TryAddGenre(genre);
            newAlbum.Href = fullAlbum.Href;
            newAlbum.Id = fullAlbum.Id;
            newAlbum.ImagesUrls = ImagesToString(fullAlbum.Images);
            newAlbum.Name = fullAlbum.Name;
            newAlbum.ReleaseDate = fullAlbum.ReleaseDate;
            newAlbum.Uri = fullAlbum.Uri;

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

    #endregion UTILS_SPOTIFY
}
