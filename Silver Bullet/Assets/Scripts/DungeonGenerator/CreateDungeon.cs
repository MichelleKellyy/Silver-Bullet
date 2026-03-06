using System;
using UnityEngine;
using System.Collections.Generic;


[Serializable]
public struct DecayValues
{
    public int upperIndex;
    public int lowerIndex;
    public int decayAt;
}

public enum PlacementOrder
{
    Stack, 
    Queue
}
public interface Fringe<T>
{
    int Count { get; }
    void Add(T item);
    T Remove();
    void Clear();
}
public class StackFringe<T> : Fringe<T>
{
    private Stack<T> stack = new Stack<T>();
    public int Count => stack.Count;

    public void Add(T item) => stack.Push(item);
    public T Remove() => stack.Pop();
    public void Clear() => stack.Clear();
}
public class QueueFringe<T> : Fringe<T>
{
    private Queue<T> queue = new Queue<T>();
    public int Count => queue.Count;

    public void Add(T item) => queue.Enqueue(item);
    public T Remove() => queue.Dequeue();
    public void Clear() => queue.Clear();
}

public class CreateDungeon : MonoBehaviour
{
    [Header("Dungeon Settings")]
    [OddRange(1, 100)]
    [Tooltip("Actual length of dungeon in a single dimension.")]
    [SerializeField] private int dungeonSize;
    [Tooltip("Indexes choose what rooms you want within the range. decayAt is the max number of rooms placed before moving to the next decay increment (so decayAt should increase after each increment).")]
    [SerializeField] private DecayValues[] decayIncrements;
    [SerializeField] private PlacementOrder placementOrder;
    [Tooltip("Length of a chain of rooms before the ending room is placed.")]
    [SerializeField] private int lengthOfDungeon;
    [SerializeField] private GameObject dungeonStartingPoint;

    [Header("Room Settings")]
    [Tooltip("Order it from lowest number of connectors to highest.")] 
    [SerializeField] private GameObject[] rooms;
    [Tooltip("The rooms do not HAVE to be the size of room size, but they cannot be larger. If they are smaller, make sure that they are positioned correctly so that they line up.")]
    [SerializeField] private int roomSize;

    [Header("Special Rooms")]
    [SerializeField] private GameObject endingRoom;
    [SerializeField] private GameObject deadEndRoom;

    [Header("Connector Settings")]
    [SerializeField] private GameObject[] connectors;

    [SerializeField] private bool generateOnStart;


    private bool[,] map;
    private Fringe<GameObject> toPlace;
    private GameObject roomParent;

    private int numRoomsPlaced = 0;
    private bool endingRoomPlaced = false;

    private int maxRetries = 50;

    private enum Directions // North is +1 on the Z, East is +1 on the X
    {
        North = 0,
        East = 1,
        South = 2,
        West = 3
    }
    private static (Directions dir, int xChange, int zChange)[] directionsInfo = { // Needed to simplify if statements
        (Directions.North,  0,  1),
        (Directions.East,   1,  0),
        (Directions.South,  0, -1),
        (Directions.West,  -1,  0),
    };

    

    private void Start()
    {
        toPlace = placementOrder == PlacementOrder.Stack ? new StackFringe<GameObject>() : new QueueFringe<GameObject>();

        if (generateOnStart)
        {
            createDungeon(false);
        }
    }

    public void editorCreateDungeon()
    {
        toPlace = placementOrder == PlacementOrder.Stack ? new StackFringe<GameObject>() : new QueueFringe<GameObject>();

        DestroyImmediate(roomParent);
        numRoomsPlaced = 0;
        endingRoomPlaced = false;

        createDungeon(true);
    }


    private void createDungeon(bool inEditMode)
    {
        roomParent = new GameObject("Rooms");

        map = new bool[dungeonSize, dungeonSize];
        map[dungeonSize / 2, dungeonSize / 2] = true; // Initial room will always be at the center

        dungeonStartingPoint.GetComponent<RoomObject>().x = dungeonSize / 2;
        dungeonStartingPoint.GetComponent<RoomObject>().z = dungeonSize / 2;
        toPlace.Add(dungeonStartingPoint);

        int retries = 0;
        while ((toPlace.Count > 0 || !endingRoomPlaced) && retries < maxRetries) // This will be done for every room that is placed
        {
            if (toPlace.Count > 0)
            {
                GameObject curRoom = toPlace.Remove();
                RoomObject curRoomData = curRoom.GetComponent<RoomObject>();

                foreach (var (placingDirection, xChange, zChange) in directionsInfo) // Go through all directions and see if cur room has any of those directions
                {
                    attemptPlacementAt(curRoom, curRoomData, placingDirection, xChange, zChange);
                }
            }
            else // We placed all rooms but no ending room was placed, retry
            {
                deleteAllRoomsAndReset(inEditMode);
                retries++;
            }
        }

        if (!endingRoomPlaced)
        {
            Debug.LogError("Failed to place ending room. Change settings");
        }
    }


    private void deleteAllRoomsAndReset(bool inEditMode)
    {
        if (inEditMode)
        {
            DestroyImmediate(roomParent);
        }
        else
        {
            Destroy(roomParent);
        }
        roomParent = new GameObject("Rooms");
        map = new bool[dungeonSize, dungeonSize];
        map[dungeonSize / 2, dungeonSize / 2] = true;
        numRoomsPlaced = 0;
        toPlace.Add(dungeonStartingPoint);
    }


    private void attemptPlacementAt(GameObject curRoom, RoomObject curRoomData, Directions placingDirection, int xChange, int zChange)
    {
        if (!curRoomData.directions[(int)placingDirection]) // Nothing to place in this direction
        {
            return;
        }

        // xNew and zNew is the space we are placing into
        int xNew = xChange + curRoomData.x;
        int zNew = zChange + curRoomData.z;

        GameObject chosenRoom = chooseNewRoom(curRoomData, xNew + xChange, zNew + zChange);

        placeNewRoom(chosenRoom, curRoom, curRoomData, placingDirection, xNew, zNew);
        numRoomsPlaced++;
    }


    private GameObject chooseNewRoom(RoomObject curRoomData, int xNew, int zNew)
    {
        if (xNew < 0 || xNew > dungeonSize - 1 || zNew < 0 || zNew > dungeonSize - 1 || (map[xNew, zNew] && !curRoomData.isConnector)) // Out of bounds or we are about to hit another room
        {
            return deadEndRoom;
        }
        if (!curRoomData.isConnector) // Connectors must follow a room
        {
            return connectors[UnityEngine.Random.Range(0, connectors.Length)];
        }
        if (!endingRoomPlaced && curRoomData.distanceFromCenter >= lengthOfDungeon)
        {
            endingRoomPlaced = true;
            return endingRoom;
        }

        int upper = 0;
        int lower = 0;
        foreach (DecayValues incr in decayIncrements) // As more rooms are placed, stop placing rooms with more branches
        {
            if (numRoomsPlaced < incr.decayAt)
            {
                upper = incr.upperIndex;
                lower = incr.lowerIndex;
                break;
            }
        }
        int random = UnityEngine.Random.Range(lower, upper + 1); // Only start placing dead ends when we have enough of other rooms
        return rooms[random];
    }


    private Directions opposite(Directions direction) // Returns the opposite direction of whats passed
    {
        return (Directions)(((int)direction + 2) % 4);
    }

    private void placeNewRoom(GameObject chosenRoom, GameObject curRoom, RoomObject curRoomData, Directions placingDirection, int xNew, int zNew)
    {
        GameObject newRoom = null;
        RoomObject newRoomData = null;

        foreach (var (placingDir, xChange, zChange) in directionsInfo)
        {
            if (placingDir == placingDirection)
            {
                newRoom = Instantiate(chosenRoom, new Vector3(curRoom.transform.position.x + xChange * roomSize, 0, curRoom.transform.position.z + zChange * roomSize), Quaternion.identity, roomParent.transform);
                newRoomData = newRoom.GetComponent<RoomObject>();
                newRoomData.x = xNew;
                newRoomData.z = zNew;

                map[xNew, zNew] = true;
                if (chosenRoom.GetComponent<RoomObject>().isConnector && xNew + xChange > 0 && xNew + xChange < dungeonSize - 1 && zNew + zChange > 0 && zNew + zChange < dungeonSize - 1) // This allows connectors to 'claim' the space ahead of them so that another room doesn't take it
                {
                    map[xNew + xChange, zNew + zChange] = true;
                }
            }
        }

        while (!newRoomData.directions[(int)opposite(placingDirection)]) // Rotate the room until some door lines up the the open door
        {
            newRoomData.rotateRoom();
        }
        newRoomData.directions[(int)opposite(placingDirection)] = false;
        newRoomData.distanceFromCenter = curRoomData.distanceFromCenter + 1;

        toPlace.Add(newRoom);
    }
}