using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DataManager : ViewControllerBase
{
    [SerializeField]
    private TMP_InputField _searchField;
    [SerializeField]
    private Button _searchBtn;
    [SerializeField]
    private TextMeshProUGUI _loadingText;
    [SerializeField]
    private int _maxAlbumsCount = 100;
    [SerializeField]
    private int _yearMin;
    [SerializeField]
    private int _yearMax;

    private SpotifyClient client;
    private SearchResponse _lastSearchResponse;

    private Dictionary<string, SimpleAlbum> _albumsSelection = new Dictionary<string, SimpleAlbum>();
    private bool isSearching = false;

    void Start()
    {
        client = SpotifyService.Instance.GetSpotifyClient();

        if (_searchBtn != null)
        {
            _searchBtn.onClick.AddListener(this.OnPerformSearch);
        }
    }

    private void Update()
    {
        if (isSearching)
        {
            _loadingText.text = $"Loading : {_albumsSelection.Count}%";
        }
    }

    private async void OnPerformSearch()
    {
        if (client != null && _searchField != null)
        {
            _albumsSelection.Clear();

            isSearching = true;

            string query = _searchField.text;
            SearchRequest request = new SearchRequest(SearchRequest.Types.Artist, query);

            _lastSearchResponse = await client.Search.Item(request);

            CreateAlbumSelection();
        }
    }

    private async void ContinueSearching()
    {
        _lastSearchResponse = await client.NextPage(_lastSearchResponse.Artists);
        CreateAlbumSelection();
    }

    private void CreateAlbumSelection()
    {
        foreach (FullArtist artist in _lastSearchResponse.Artists.Items)
        {
            GetTopTracksByArtist(artist);
        }

        if (_albumsSelection.Count >= _maxAlbumsCount)
        {
            isSearching = false;
            DisplayAlbumsNameConsole();
        }
        else
        {
            ContinueSearching();
        }
    }

    private void DisplayAlbumsNameConsole()
    {
        string response = string.Empty;
        foreach (SimpleAlbum album in _albumsSelection.Values)
        {
            response += $"-{album.Name}";
        }

        Debug.Log(response);
    }

    private async void GetTopTracksByArtist(FullArtist artist)
    {
        List<FullTrack> topTracks = new List<FullTrack>();

        ArtistsTopTracksRequest request = new ArtistsTopTracksRequest("FR");
        ArtistsTopTracksResponse response = await client.Artists.GetTopTracks(artist.Id, request);

        topTracks = response.Tracks;

        foreach (FullTrack track in topTracks)
        {
            SimpleAlbum album = track.Album;

            if (IsAlbumReleaseDateValid(album.ReleaseDate, _yearMin, _yearMax))
            {
                if (!_albumsSelection.ContainsKey(album.Id))
                {
                    _albumsSelection.Add(album.Id, album);
                }
            }
        }
    }

    private bool IsAlbumReleaseDateValid(string releaseDate, object yearMin, object yearMax)
    {
        throw new NotImplementedException();
    }

    private bool IsAlbumReleaseDateValid(string date, int yearA, int yearB)
    {
        int year;
        if (int.TryParse(date.Substring(0, 4), out year))
        {
            return year >= yearA && year <= yearB;
        }
        else
        {
            throw new ArgumentException("Le format de la date n'est pas valide.");
        }
    }
}
