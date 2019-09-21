using UnityEngine;
using RPG.Core;

namespace RPG.Control.Cursor
{
    public interface IRaycastable
    {
        CursorType GetCursorType();
        bool HandleRaycast(GameObject player, ActionMarker attackMarker, ActionMarker waypointMarker, int priority);
    }
}
