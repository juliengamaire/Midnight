using SpotifyAPI.Web;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnTableShuffleButton : TurnTableBaseButton
{
    [SerializeField]
    private Texture2D _shuffleOnTexture, _shuffleOffTexture;
    [SerializeField]
    private MeshRenderer _meshRenderer;

    public override void UpdateButtonIcon(CurrentlyPlayingContext context)
    {
        base.UpdateButtonIcon(context);
        if (context.ShuffleState)
        {
            _meshRenderer.material.SetTexture("_BaseMap", _shuffleOnTexture);
        }
        else
        {
            _meshRenderer.material.SetTexture("_BaseMap", _shuffleOffTexture);
        }
    }
}
