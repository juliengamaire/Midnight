using SpotifyAPI.Web;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnTablePlayPauseButton : TurnTableBaseButton
{
    [SerializeField]
    private Texture2D _playTexture, _pauseTexture;
    [SerializeField]
    private MeshRenderer _meshRenderer;

    public override void UpdateButtonIcon(CurrentlyPlayingContext context)
    {
        base.UpdateButtonIcon(context);
        if (context.IsPlaying)
        {
            _meshRenderer.material.SetTexture("_BaseMap", _pauseTexture);
        }
        else
        {
            _meshRenderer.material.SetTexture("_BaseMap", _playTexture);
        }
    }
}
