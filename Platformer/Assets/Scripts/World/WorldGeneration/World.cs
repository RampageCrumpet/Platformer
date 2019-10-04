using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class World 
{
    private enum RoomEdge { Top, Right, Bottom, Left }
    [SerializeField]private List<Room> placedRooms = new List<Room>();

    [Tooltip( "The seed determines what map will be generated. Set -1 to randomize using time." )]
    [SerializeField] private int seed = 0;

    [Tooltip("The number of tiles wide our world is.")]
    public int worldWidth = 50;

    [Tooltip("The number of tiles tall our world is.")]
    public int worldHeight = 50;

    [Tooltip("The number of rooms we want to spawn")]
    [SerializeField] private int roomCount = 15;

    public GameObject testSprite;
    public GameObject openConnectionTestSprite;
    public GameObject closedConnectionTestSprite;

    Room[,] worldGrid;

    //The rooms in this list can be spawned as many times as they need to be to fill out the map.
    [SerializeField] private List<Room> spawnableRooms = new List<Room>();

    //Rooms in this list can only be spawned once and MUST be spawned once.
    [SerializeField] private List<Room> mandatoryRooms = new List<Room>();
    

    private List<RoomConnection> openConnections =  new List<RoomConnection>();

    // Start is called before the first frame update
    void Start()
    {
        //Create a random seed from the current timestamp
        if ( seed < 0 ) seed = Random.Range(0,1000);
        Random.InitState( seed );
 

        worldGrid = new Room[worldWidth, worldHeight];

        PlaceSeedRoom();

        //Place our starting rooms
        for(int i = 0; i < roomCount; i++)
        {
            int roomConnectionIndex;
            Room placedRoom;

            //Initialized to invalid values
            Vector2Int placementPosition;
            do
            {

                //Pick a random open connection to build off of.
                do
                {
                    roomConnectionIndex = Random.Range(0, openConnections.Count);
                }
                while(openConnections[roomConnectionIndex].isOpen == false);
                
    
                //Get the room we want to place.
                placedRoom = PickRoom(openConnections[roomConnectionIndex]);
                placementPosition = GetRoomPlacementPosition(placedRoom, openConnections[roomConnectionIndex]);

                FillConnections(placedRoom, placementPosition);

            }while((ChangeInExteriorConnections(placedRoom, placementPosition) + NumberOfExteriorConnections()) == 0);
            
            PlaceRoom(placedRoom,placementPosition);
        }
        
        //Close off all of our connections by placing more rooms.
        while(openConnections.Count > 0)
        {
            int roomConnectionIndex;
            Room placedRoom;

            //Initialized to invalid values
            Vector2Int placementPosition = new Vector2Int(-1,-1);

            do
            { 

                //Pick a random open connection to build off of.   
                do
                {
                    roomConnectionIndex = Random.Range(0, openConnections.Count);
                }
                while(openConnections[roomConnectionIndex].isOpen == false);
    
                //Get the room we want to place.
                placedRoom = PickRoom(openConnections[roomConnectionIndex]);
                placementPosition = GetRoomPlacementPosition(placedRoom, openConnections[roomConnectionIndex]);

                FillConnections(placedRoom, placementPosition);

            }while(ChangeInExteriorConnections(placedRoom, placementPosition) > 0);
            
            PlaceRoom(placedRoom,placementPosition);
        }

        DebugDrawConnectionPoints();
        CreateUIMap(worldGrid, placedRooms);
        DebugRenderer();
        
    }

    bool checkValidRoomPlacement(Room room, Vector2Int position)
    {
        bool isValidRoomPlacement = true;

        isValidRoomPlacement &= CheckRoomPlacement(room, position);
        isValidRoomPlacement &= CheckValidConnections(room, position);
        isValidRoomPlacement &= CheckIfRoomConnects(room);

        return isValidRoomPlacement;
    }
    
    //Gets the position for the lower left edge of our room.
    Vector2Int GetRoomPlacementPosition(Room room, RoomConnection roomConnection)
    {
        Vector2Int placementPosition = roomConnection.originPosition;

        //See if the connection points to the left
        if(roomConnection.originPosition.x > roomConnection.endPosition.x)
        {
            placementPosition.x -= room.roomWidth;
        }
        else
        //See if the connection points to the right
        if(roomConnection.originPosition.x < roomConnection.endPosition.x)
        {
            placementPosition.x += 1;
        }
        else //See if the connection points down
        if(roomConnection.originPosition.y > roomConnection.endPosition.y)
        {
            placementPosition.y -= room.roomHeight;
            
        }
        else
        //See if the connection points up
        if(roomConnection.originPosition.y < roomConnection.endPosition.y)
        {
            placementPosition.y += 1;
        }

        return placementPosition;
    }

    Room PickRoom(RoomConnection roomConnection, List<Room> placeableRooms)
    {
        
        List<Room> placeableRooms = new List<Room>();
        Vector2Int placementPosition;

        foreach(Room room in spawnableRooms)
        {
            placementPosition = GetRoomPlacementPosition(room, roomConnection);

            FillConnections(room, placementPosition);

            if(checkValidRoomPlacement(room, placementPosition))
            {
                placeableRooms.Add(room);
            }
        }

        if(placeableRooms.Count == 0)
        {
            DebugDrawConnectionPoints();
            CreateUIMap(worldGrid, placedRooms);
            DebugRenderer();

            Debug.Log(worldGrid[roomConnection.endPosition.x, roomConnection.endPosition.y]);

            Debug.Log(roomConnection.originPosition + ":" + roomConnection.endPosition);
            
            Debug.LogError("There's no placeable rooms for this spot.");
            Application.Quit();
        }
 
        return placeableRooms[Random.Range(0, placeableRooms.Count)];
    }


    //Places the spawned room with the lower left corner at the given x and y conditions
    void PlaceRoom(Room roomTemplate, Vector2Int position)
    {
        Room room = new Room(roomTemplate);

        //Add the room to our spawned rooms list
        placedRooms.Add(room);

        for(int x = 0; x < room.roomWidth; x++)
        {
            for(int y =0; y < room.roomHeight; y++)
            {
                if(worldGrid[x,y] != null)
                    Debug.LogError("Something has gone terribly wrong and we're placing a room overtop of another one!");

                worldGrid[position.x + x, position.y + y] = room;
            }
        }
    
        //Fills our connections with the correct world Coordinates
        FillConnections(room,position);

        //Add the rooms connections to the open connections list.
        foreach(RoomConnection roomConnection in room.connections)
        {
            if(!openConnections.Contains(roomConnection) && roomConnection.isOpen == true && worldGrid[roomConnection.endPosition.x, roomConnection.endPosition.y] == null)
            {
                openConnections.Add(roomConnection);
            }    
        }   

        CheckForFullConnections();
    }

    bool CheckRoomPlacement(Room room, Vector2Int position)
    {
        //If we're out of the world maps bounds our placement is invalid.
        if(position.x < 0 || position.y < 0 || position.x + room.roomWidth >= worldWidth || position.y + room.roomHeight >= worldHeight)
        {
            return false;
        }

        for(int x =0; x<room.roomWidth; x++)
        {
            for(int y = 0; y<room.roomHeight; y++)
            {
                if(worldGrid[x + position.x, y + position.y] != null)
                {
                    return false;   
                }
            }
        }

        return true;
    }

    void PlaceSeedRoom()
    {
        int randomRoomIndex = Random.Range(0, spawnableRooms.Count); 

        Room spawnedRoom = spawnableRooms[randomRoomIndex];

        Vector2Int roomPlacementPosition = new Vector2Int((worldWidth - spawnedRoom.roomWidth)/2,(worldHeight - spawnedRoom.roomHeight)/2);


        //Check to make sure we don't place our seed room outside of bound
        while(CheckRoomPlacement(spawnedRoom,roomPlacementPosition) == false)
        {
            spawnedRoom = spawnableRooms[randomRoomIndex];
            Debug.Log("Picking another seed room.");
        }

        PlaceRoom(spawnedRoom, roomPlacementPosition);
    }

    //Set the connections to their location on the world grid.
    private void FillConnections( Room room, Vector2Int roomPosition ) 
    {
        for ( int i = 0; i < room.connections.Count; i++ ) 
        {
            UpdateRoomConnection( room, i, roomPosition );
        }

    }



    private RoomEdge GetRoomEdge( bool isTopOrRight, bool isTopOrBottom ) 
    {
        // RoomEdge values are 0 to 3 for t,r,b,l
        int index = 0;
        if ( !isTopOrRight  ) index += 2; // t,r is 0,1 and b,l is 2,3
        if ( !isTopOrBottom ) index += 1; // t,b is even, r,l is odd
        return (RoomEdge) index;
    }

    private RoomConnection UpdateRoomConnection( Room room, int connectionIndex, Vector2Int roomPosition ) 
    {
        RoomConnection connection = room.connections[connectionIndex];
        Vector2Int endOffset = Vector2Int.zero;

        int half = room.connections.Count / 2;

        // first half is top or right edge
        // second half is bottom or left
        bool tr = (connectionIndex < half);
        connectionIndex %= half; // make the index relative to whichever half we are in

        // In each half, the horizontal (top or bottom) edge is the first set of connections
        bool tb = (connectionIndex < room.roomWidth );
        if ( !tb ) connectionIndex -= room.roomWidth; // make the index relative to the edge

        RoomEdge edge = GetRoomEdge(tr, tb);

        if ( edge == RoomEdge.Top ) 
        {
            roomPosition.x += connectionIndex;
            roomPosition.y += room.roomHeight - 1;
            endOffset.y = +1;
        } 
        else if ( edge == RoomEdge.Right ) 
        {
            roomPosition.x += room.roomWidth - 1;
            roomPosition.y += room.roomHeight - connectionIndex - 1;
            endOffset.x = +1;
        } 
        else if ( edge == RoomEdge.Bottom ) 
        {
            roomPosition.x += room.roomWidth - connectionIndex - 1;
            endOffset.y = -1;
        } 
        else 
        {//if ( edge == RoomEdge.Left ) {
            roomPosition.y += connectionIndex;
            endOffset.x = -1;
        }

        connection.originPosition = roomPosition;
        connection.endPosition = roomPosition + endOffset;

        return connection;
    }

    //Checks to see if any of the OpenConnections point to a room. If they do then we know that they're no longer open.
    private void CheckForFullConnections()
    {
        for(int i = openConnections.Count-1; i >= 0; i--)
        {
            //If the outside edge of the connection is full then we know the connection is used.
            if(worldGrid[openConnections[i].endPosition.x, openConnections[i].endPosition.y] != null)
            {
                openConnections.RemoveAt(i);  
            }    
        }
    }


    //TODO: Move this back to the UI when you finish testing the level generation.
    public void CreateUIMap(Room[,] worldGrid, List<Room> rooms)
    {
        List<Room> displayedRooms = new List<Room>();
        for(int x = 0; x < worldGrid.GetLength(0); x++)
        {
            for(int y = 0; y < worldGrid.GetLength(1); y++)
            {
                if(!displayedRooms.Contains(worldGrid[x,y]) && rooms.Contains(worldGrid[x,y]))
                {
                    //Remove the found room from the list of rooms to instantiate
                    displayedRooms.Add(worldGrid[x,y]);
                    Instantiate(worldGrid[x,y].mapImage, new Vector3(x,y,0), Quaternion.identity);
                }
            }
        }
    }

    //Checks if any of the rooms connections end in the connection given. 
    private bool CheckIfRoomConnects(Room room)
    {
        foreach(RoomConnection roomConnection in room.connections)
        {
            if (!roomConnection.isOpen) continue;
            foreach(RoomConnection worldConnection in openConnections)
            {
                if(!worldConnection.isOpen) continue;
                if (roomConnection.endPosition == worldConnection.originPosition) return true;
            }
        }

        return false; 
    }


    //Returns false is the room will block off any existing connections
    private bool CheckValidConnections( Room room, Vector2Int position ) 
    {


        foreach (RoomConnection roomConnection in room.connections) 
        {
            Room targetRoom = worldGrid[roomConnection.endPosition.x, roomConnection.endPosition.y];
            if(targetRoom != null)
            {
                foreach(RoomConnection targetConnection in targetRoom.connections)
                {
                    bool isConnected = targetConnection.endPosition == roomConnection.originPosition;
                    if(isConnected && (roomConnection.isOpen != targetConnection.isOpen))
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    //Draws a sprite in each square of the grid.
    private void DebugRenderer()
    {
        for(int x = 0; x < worldGrid.GetLength(0); x++)
        {
            for(int y = 0; y < worldGrid.GetLength(1); y++)
            {
                if(worldGrid[x,y] != null)
                {
                    Instantiate(testSprite, new Vector3(x+0.5f,y+0.5f,0), Quaternion.identity);
                }
            }
        }
    }

    //Draws the connection points slightly offset so we can see both.
    private void DebugDrawConnectionPoints()
    {
        foreach(Room room in placedRooms)
        { 
            foreach(RoomConnection connection in room.connections)
            { 
                GameObject connectionSprite;
                Vector2 originCorrectionOffset = new Vector2(0.5f, 0.5f);
                float offsetPercent = 0.95f;

                Vector2 prefabSpawnPosition = connection.originPosition + originCorrectionOffset;
                prefabSpawnPosition += (((Vector2)(connection.endPosition - connection.originPosition)/2.0F)*offsetPercent);

                connectionSprite = (connection.isOpen) ? openConnectionTestSprite : closedConnectionTestSprite;

                Instantiate(connectionSprite,prefabSpawnPosition, Quaternion.identity);
            }
        }
    }

    //Returns how many open connections would be remaining after a room was placed at a specific position
    private int ChangeInExteriorConnections(Room room, Vector2 position)
    {
        int newExteriorConnections = 0;

        //The new exterior connections created by this room.
        foreach(RoomConnection connection in room.connections)
        {
            if(!connection.isOpen) continue;

            //Check to see if the end position of our connection doesn't point to an existing room;
            if(worldGrid[connection.endPosition.x,connection.endPosition.y] == null)
            {
                newExteriorConnections++;
            }
        }

        //The old exterior connections closed by this room.
        foreach(RoomConnection connection in openConnections)
        {
            if(!connection.isOpen) continue;

            //Check to see if the end point of our connection points to our room;
            if(connection.endPosition.x >= position.x && connection.endPosition.x < position.x + room.roomWidth)
            {
                if(connection.endPosition.y >= position.y && connection.endPosition.y < position.y + room.roomHeight)
                {
                    newExteriorConnections--;
                }
            }
        }

        return newExteriorConnections;
    }

    //Returns the nunmber of open exterior connections in our world.
    private int NumberOfExteriorConnections()
    {
        int exteriorConnections = 0;

        foreach(Room room in placedRooms)
        {
            foreach(RoomConnection roomConnection in room.connections)
            {
                if(roomConnection.isOpen && worldGrid[roomConnection.endPosition.x, roomConnection.endPosition.y] == null)
                {
                    exteriorConnections++;
                }
            }
        }

        return exteriorConnections;
    }

    //Supposed to return the starting room for our game. Currently returns the first room placed.
    Room GetStartRoom()
    {
        return placedRooms[0];
    }
}
