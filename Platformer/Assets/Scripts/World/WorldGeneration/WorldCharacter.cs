using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldCharacter
{
    //The room the player is currently in.
    Room currentRoom = null;

    public room CurrentRoom{
        get {return currentRoom;}
        set {currentRoom = CurrentRoom;} 
    }

    World world;

    // Start is called before the first frame update

    public WorldCharacter(Room room)
    {
        SetRoom(room);
    }

    public void SetRoom(Room room)
    {
        currentRoom = room;
    }
}
