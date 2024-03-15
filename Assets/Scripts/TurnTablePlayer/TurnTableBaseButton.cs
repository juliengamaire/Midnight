using SpotifyAPI.Web;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TurnTableBaseButton : MonoBehaviour
{
    public UnityAction OnButtonPressed;
    
    [SerializeField]
    private Texture2D customCursor;
    private CursorMode cursorMode = CursorMode.Auto;
    private Vector2 hotSpot = Vector2.zero; // Point chaud du curseur

    private void OnMouseDown()
    {
        if (OnButtonPressed != null)
        {
            OnButtonPressed?.Invoke();
        }
    }

    private void OnMouseEnter()
    {
        UnityEngine.Cursor.SetCursor(customCursor, hotSpot, cursorMode); // Change le curseur de la souris
    }

    private void OnMouseExit()
    {
        UnityEngine.Cursor.SetCursor(null, Vector2.zero, cursorMode); // Rétablit le curseur par défaut
    }

    public virtual void UpdateButtonIcon(CurrentlyPlayingContext context)
    {

    }
}
