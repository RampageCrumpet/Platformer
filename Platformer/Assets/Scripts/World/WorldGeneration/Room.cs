using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Room
{
    [Tooltip("The number of tiles wide the room is.")]
    public int roomWidth = 1;

    [Tooltip("The number of tiles  tall our room is.")]
    public int roomHeight = 1;

    public string roomName = "TestRoom";

    [Tooltip("The image our room will use when displayed on the map.")]
    public Object mapImage;

    //Whether a tile is open to world. Starts at the top left and counts clockwise.
    public List<RoomConnection> connections = new List<RoomConnection>();


    //Copy Constructor
    public Room(Room room)
    {
        roomWidth = room.roomWidth;
        roomHeight = room.roomHeight;

        roomName = room.roomName;

        mapImage = room.mapImage;
        //Copy all of the connections
        foreach(RoomConnection connection in room.connections)
        {
            connections.Add(new RoomConnection(connection));
        }
    }
}
