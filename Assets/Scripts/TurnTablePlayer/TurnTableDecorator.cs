using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnTableDecorator : MonoBehaviour
{
    private string _previousCoverUrl;

    public void Awake()
    {
        _previousCoverUrl = string.Empty;
    }

    public void UpdatePlayerInfo(string trackName, string artistNames, string coverUrl)
    {
        UpdateTrackName(trackName);
        UpdateArtistNames(artistNames);
        UpdateCover(coverUrl);
    }

    private void UpdateCover(string coverUrl)
    {
        if (string.IsNullOrEmpty(coverUrl)) return;

        if (string.IsNullOrEmpty(_previousCoverUrl) || !(_previousCoverUrl.Equals(coverUrl)))
        {
            _previousCoverUrl = coverUrl;

            // TODO JULIEN
            // DL cover texture and apply
        }
    }

    private void UpdateArtistNames(string artistNames)
    {
        // TODO JULIEN
        // Update text component
    }

    private void UpdateTrackName(string trackName)
    {
        // TODO JULIEN
        // Update text component
    }
}
