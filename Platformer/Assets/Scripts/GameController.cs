using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    //The world map.
    World world = new World();

    //Our character in the world.
    WorldCharacter worldCharacter = new WorldCharacter(World.GetStartRoom());

    
}
