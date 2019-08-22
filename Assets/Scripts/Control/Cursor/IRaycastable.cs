using UnityEngine;

namespace RPG.Control.Cursor
{
    public interface IRaycastable
    {
        CursorType GetCursorType();
        bool HandleRaycast(GameObject player);
    }
}
