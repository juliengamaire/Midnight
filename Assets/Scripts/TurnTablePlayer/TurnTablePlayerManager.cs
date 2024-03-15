using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class TurnTablePlayerManager : SpotifyPlayerListener
{
    // Player middle media controls
    [SerializeField]
    private TurnTableBaseButton _previousButton, _nextButton;
    [SerializeField]
    private TurnTablePlayPauseButton _playPauseButton;
    [SerializeField]
    private TurnTableShuffleButton _shuffleButton;
    [SerializeField]
    private TurnTableTransitionButton _transitionButton;
    private bool _isTransitioning;

    [SerializeField]
    private TurnTableDecorator _turnTableDecorator;

    //// Button to add/remove track to user's library
    //[SerializeField]
    //private Button _addToLibraryButton;

    //// Player middle track progress left & right text
    //[SerializeField]
    //private Text _currentProgressText, _totalProgressText;
    //// Player middle track progress bar
    //[SerializeField]
    //private Slider _currentProgressSlider;

    //// Player right volume slider
    //[SerializeField]
    //private Slider _volumeSlider;

    //[SerializeField]
    //private Sprite _playSprite, _pauseSprite, _muteSprite, _unmuteSprite;

    //// Is the current track in the user's library?
    //private bool _currentItemIsInLibrary;
    //// Did the user mouse down on the progress slider to edit the progress
    //private bool _progressStartDrag = false;
    //// Current progress value when user is sliding the progress
    //private float _progressDragNewValue = -1.0f;
    //// Last volume value before mute/unmute
    //private int _volumeLastValue = -1;

    protected override void Awake()
    {
        base.Awake();

        // Listen to needed events on Awake
        this.OnPlayingItemChanged += this.PlayingItemChanged;
    }

    private void Start()
    {
        if (_playPauseButton != null)
        {
            _playPauseButton.OnButtonPressed += this.OnPlayPauseClicked;
        }
        if (_previousButton != null)
        {
            _previousButton.OnButtonPressed += this.OnPreviousClicked;
        }
        if (_nextButton != null)
        {
            _nextButton.OnButtonPressed += this.OnNextClicked;
        }
        if (_shuffleButton != null)
        {
            _shuffleButton.OnButtonPressed += this.OnShuffleClicked;
        }
        if (_transitionButton != null)
        {
            _transitionButton.OnButtonPressed += this.OnTransitionClicked;
        }

        //// Configure progress slider
        //if (_currentProgressSlider != null)
        //{
        //    // Enable only whole numbers, interaction
        //    _currentProgressSlider.wholeNumbers = true;
        //    _currentProgressSlider.interactable = true;

        //    // Listen to value change on slider
        //    _currentProgressSlider.onValueChanged.AddListener(this.OnProgressSliderValueChanged);
        //    // Add EventTrigger component, listen to mouse up/down events
        //    EventTrigger eventTrigger = _currentProgressSlider.gameObject.AddComponent<EventTrigger>();
        //    // Mouse Down event
        //    EventTrigger.Entry entry = new EventTrigger.Entry
        //    {
        //        eventID = EventTriggerType.PointerDown
        //    };
        //    entry.callback.AddListener(this.OnProgressSliderMouseDown);
        //    eventTrigger.triggers.Add(entry);
        //    // Mouse Up event
        //    entry = new EventTrigger.Entry()
        //    {
        //        eventID = EventTriggerType.PointerUp
        //    };
        //    entry.callback.AddListener(this.OnProgressSliderMouseUp);
        //    eventTrigger.triggers.Add(entry);
        //}
    }

    private void Update()
    {
        CurrentlyPlayingContext context = GetCurrentContext();
        if (context != null)
        {
            //// Update current position to context position when user is not dragging
            //if (_currentProgressText != null && !_progressStartDrag)
            //{
            //    _currentProgressText.text = S4UUtility.MsToTimeString(context.ProgressMs);
            //}

            //// Update Volume slider
            //if (_volumeSlider != null)
            //{
            //    _volumeSlider.minValue = 0;
            //    _volumeSlider.maxValue = 100;
            //    _volumeSlider.value = context.Device.VolumePercent.Value;
            //}

            // Update play/pause btn sprite with correct play/pause sprite
            if (_playPauseButton != null)
            {
                _playPauseButton.UpdateButtonIcon(context);                
            }

            // Update shuffle btn sprite 
            if (_shuffleButton != null)
            {
                _shuffleButton.UpdateButtonIcon(context);
            }

            //FullTrack track = context.Item as FullTrack;
            //if (track != null)
            //{
            //    if (_totalProgressText != null)
            //    {
            //        _totalProgressText.text = S4UUtility.MsToTimeString(track.DurationMs);
            //    }
            //    if (_currentProgressSlider != null)
            //    {
            //        _currentProgressSlider.minValue = 0;
            //        _currentProgressSlider.maxValue = track.DurationMs;

            //        // Update position when user is not dragging slider
            //        if (!_progressStartDrag)
            //            _currentProgressSlider.value = context.ProgressMs;
            //    }
            //}
        }
    }

    private void OnDestroy()
    {
        this.OnPlayingItemChanged -= this.PlayingItemChanged;

        if (_playPauseButton != null)
        {
            _playPauseButton.OnButtonPressed -= this.OnPlayPauseClicked;
        }
        if (_previousButton != null)
        {
            _previousButton.OnButtonPressed -= this.OnPreviousClicked;
        }
        if (_nextButton != null)
        {
            _nextButton.OnButtonPressed -= this.OnNextClicked;
        }
        if (_shuffleButton != null)
        {
            _shuffleButton.OnButtonPressed -= this.OnShuffleClicked;
        }
    }

    protected override async void PlayingItemChanged(IPlayableItem newPlayingItem)
    {
        if (newPlayingItem == null)
        {
            // No new item playing, reset UI
            UpdatePlayerInfo("", "", "");
            // TODO JULIEN 
            // Hide vinyl and reset arm

            //SetLibraryBtnIsLiked(false);
        }
        else
        {
            if (newPlayingItem.Type == ItemType.Track)
            {
                if (newPlayingItem is FullTrack track)
                {
                    // Update player information with track info
                    string allArtists = S4UUtility.ArtistsToSeparatedString(", ", track.Artists);
                    SpotifyAPI.Web.Image image = S4UUtility.GetHighestResolutionImage(track.Album.Images);
                    UpdatePlayerInfo(track.Name, allArtists, image?.Url);

                    // Make request to see if track is part of user's library
                    //var client = SpotifyService.Instance.GetSpotifyClient();
                    //LibraryCheckTracksRequest request = new LibraryCheckTracksRequest(new List<string>() { track.Id });
                    //var result = await client.Library.CheckTracks(request);
                    //if (result.Count > 0)
                    //{
                    //    SetLibraryBtnIsLiked(result[0]);
                    //}
                }
            }
            else if (newPlayingItem.Type == ItemType.Episode)
            {
                // TODO JULIEN
                // What is the behaviour if it's an episode ?

                //if (newPlayingItem is FullEpisode episode)
                //{
                //    string creators = episode.Show.Publisher;
                //    SpotifyAPI.Web.Image image = S4UUtility.GetHighestResolutionImage(episode.Images);
                //    UpdatePlayerInfo(episode.Name, creators, image?.Url);
                //}
            }
        }
    }

    // Updates the left hand side of the player (Artwork, track name, artists)
    private void UpdatePlayerInfo(string trackName, string artistNames, string artUrl)
    {
        _turnTableDecorator.UpdatePlayerInfo(trackName, artistNames, artUrl);
    }

    private void OnPlayPauseClicked()
    {
        // Get current context & client, check if null
        CurrentlyPlayingContext context = GetCurrentContext();
        SpotifyClient client = SpotifyService.Instance.GetSpotifyClient();
        if (context != null && client != null)
        {
            if (context.IsPlaying)
            {
                client.Player.PausePlayback();
            }
            else
            {
                client.Player.ResumePlayback();
            }
        }
    }

    private void OnPreviousClicked()
    {
        SpotifyClient client = SpotifyService.Instance.GetSpotifyClient();
        if (client != null)
        {
            client.Player.SkipPrevious();
        }
    }

    private void OnNextClicked()
    {
        SpotifyClient client = SpotifyService.Instance.GetSpotifyClient();
        if (client != null)
        {
            client.Player.SkipNext();
        }
    }

    private void OnShuffleClicked()
    {
        SpotifyClient client = SpotifyService.Instance.GetSpotifyClient();
        if (client != null)
        {
            // get current shuffle state
            bool currentShuffleState = GetCurrentContext().ShuffleState;
            // Create request, invert state
            PlayerShuffleRequest request = new PlayerShuffleRequest(!currentShuffleState);

            client.Player.SetShuffle(request);
        }
    }

    private void OnTransitionClicked()
    {
        _isTransitioning = !_isTransitioning;
        if (_transitionButton != null)
        {
            _transitionButton.UpdateButtonIcon(_isTransitioning);
        }
    }

    //private void OnToggleMute()
    //{
    //    SpotifyClient client = SpotifyService.Instance.GetSpotifyClient();
    //    var context = GetCurrentContext();
    //    if (context != null && client != null)
    //    {
    //        int? volume = context.Device.VolumePercent;
    //        int targetVolume;
    //        Image muteImg = _muteButton.transform.GetChild(0).GetComponent<Image>();
    //        if (volume.HasValue && volume > 0)
    //        {
    //            // Set target volume to 0, sprite to muted
    //            targetVolume = 0;
    //            muteImg.sprite = _muteSprite;
    //            // Save current volume for unmute press
    //            _volumeLastValue = volume.Value;
    //        }
    //        else
    //        {
    //            // Set target to last volume value before mute
    //            if (_volumeLastValue > 0)
    //            {
    //                targetVolume = _volumeLastValue;
    //                _volumeLastValue = -1;
    //            }
    //            else
    //            {
    //                // If no value, use default value
    //                targetVolume = 25;
    //            }

    //            // Update sprite
    //            muteImg.sprite = _unmuteSprite;
    //        }

    //        // Send request
    //        PlayerVolumeRequest request = new PlayerVolumeRequest(targetVolume);
    //        client.Player.SetVolume(request);
    //    }
    //}

    //private async void OnToggleAddToLibrary()
    //{
    //    SpotifyClient client = SpotifyService.Instance.GetSpotifyClient();

    //    // Get current context and check any are null
    //    CurrentlyPlayingContext context = this.GetCurrentContext();
    //    if (client != null && context != null)
    //    {
    //        List<string> ids = new List<string>();
    //        // Cast Item to correct type, add it's URI add make request
    //        if (context.Item.Type == ItemType.Track)
    //        {
    //            FullTrack track = context.Item as FullTrack;
    //            ids.Add(track.Id);

    //            if (_currentItemIsInLibrary)
    //            {
    //                // Is in library, remove
    //                LibraryRemoveTracksRequest removeRequest = new LibraryRemoveTracksRequest(ids);
    //                await client.Library.RemoveTracks(removeRequest);

    //                SetLibraryBtnIsLiked(false);
    //            }
    //            else
    //            {
    //                // Not in library, add to user's library
    //                LibrarySaveTracksRequest removeRequest = new LibrarySaveTracksRequest(ids);
    //                await client.Library.SaveTracks(removeRequest);

    //                SetLibraryBtnIsLiked(true);
    //            }
    //        }
    //        else if (context.Item.Type == ItemType.Episode)
    //        {
    //            FullEpisode episode = context.Item as FullEpisode;
    //            ids.Add(episode.Id);

    //            if (_currentItemIsInLibrary)
    //            {
    //                LibraryRemoveShowsRequest request = new LibraryRemoveShowsRequest(ids);
    //                await client.Library.RemoveShows(request);

    //                SetLibraryBtnIsLiked(false);
    //            }
    //            else
    //            {
    //                LibrarySaveShowsRequest request = new LibrarySaveShowsRequest(ids);
    //                await client.Library.SaveShows(request);

    //                SetLibraryBtnIsLiked(true);
    //            }
    //        }


    //    }
    //}

    //private void OnProgressSliderMouseDown(BaseEventData arg0)
    //{
    //    _progressStartDrag = true;
    //}

    //private void OnProgressSliderValueChanged(float newValueMs)
    //{
    //    _progressDragNewValue = newValueMs;

    //    _currentProgressText.text = S4UUtility.MsToTimeString((int)_progressDragNewValue);
    //}

    //private void OnProgressSliderMouseUp(BaseEventData arg0)
    //{
    //    if (_progressStartDrag && _progressDragNewValue > 0)
    //    {
    //        SpotifyClient client = SpotifyService.Instance.GetSpotifyClient();

    //        // Build request to set new ms position
    //        PlayerSeekToRequest request = new PlayerSeekToRequest((long)_progressDragNewValue);
    //        client.Player.SeekTo(request);

    //        // Set value in slider
    //        _currentProgressSlider.value = _progressDragNewValue;

    //        // Reset variables
    //        _progressStartDrag = false;
    //        _progressDragNewValue = -1.0f;
    //    }
    //}

    //private void SetLibraryBtnIsLiked(bool isLiked)
    //{
    //    _currentItemIsInLibrary = isLiked;

    //    if (_addToLibraryButton != null)
    //    {
    //        Image img = _addToLibraryButton.GetComponent<Image>();
    //        img.color = isLiked ? Color.green : Color.white;
    //    }
    //}
}
