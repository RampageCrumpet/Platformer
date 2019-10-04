using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class RoomConnection
{
    public Vector2Int originPosition;
    public Vector2Int endPosition;

    [Tooltip("Whether or not another room can connect here.")]
    public bool isOpen = false;

    public RoomConnection (RoomConnection roomConnection)
    {
        originPosition = roomConnection.originPosition;
        endPosition = roomConnection.endPosition;

        isOpen = roomConnection.isOpen;
    }
  
}
