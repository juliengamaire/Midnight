using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnTableTransitionButton : TurnTableBaseButton
{
    [SerializeField]
    private Texture2D _transitionOnTexture, _transitionOffTexture;
    [SerializeField]
    private MeshRenderer _meshRenderer;

    public void UpdateButtonIcon(bool isTransitioning)
    {
        if (isTransitioning)
        {
            _meshRenderer.material.SetTexture("_BaseMap", _transitionOnTexture);
        }
        else
        {
            _meshRenderer.material.SetTexture("_BaseMap", _transitionOffTexture);
        }
    }
}
