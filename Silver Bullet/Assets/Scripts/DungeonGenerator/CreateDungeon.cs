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

[Serializable]
public struct ColouredMats
{
    public Material nonWall;
    public Material wall;
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
    [Tooltip("Tiers of materials (rooms further distance from the beginning uses higher tiered materials)")]
    [SerializeField] private int[] tieredMatValues;
    [SerializeField] private ColouredMats[] tieredMaterials;

    [Header("Enemy Settings")]
    [Tooltip("Length of a chain from the center before enemies start spawning.")]
    [SerializeField] private int enemySpawnDistance;
    [Range(0, 1)]
    [SerializeField] private float chanceOfEnemyPerRoom;
    [SerializeField] private int numEnemiesPerRoomLow;
    [SerializeField] private int numEnemiesPerRoomHigh;
    [SerializeField] private GameObject enemy;

    [Header("Special Rooms")]
    [SerializeField] private GameObject endingRoom;
    [SerializeField] private GameObject deadEndRoom;
    [SerializeField] private GameObject key;
    [Tooltip("For each key, add the distance (in percentage) that you want the key to spawn at")]
    [SerializeField] private float[] keyDistances;

    [Header("Connector Settings")]
    [SerializeField] private GameObject[] connectors;

    [SerializeField] private bool generateOnStart;


    private bool[,] map;
    private Fringe<GameObject> toPlace;
    private GameObject roomParent;
    private GameObject enemyParent;
    private GameObject keyParent;

    private int numRoomsPlaced = 0;
    private List<RoomObject> placedRooms = new List<RoomObject>();
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
        DestroyImmediate(enemyParent);
        DestroyImmediate(keyParent);
        placedRooms.Clear();
        numRoomsPlaced = 0;
        endingRoomPlaced = false;

        createDungeon(true);
    }


    private void createDungeon(bool inEditMode)
    {
        roomParent = new GameObject("Rooms");
        enemyParent = new GameObject("Enemies");
        keyParent = new GameObject("Keys");

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

        placeKeys();
    }


    private void deleteAllRoomsAndReset(bool inEditMode)
    {
        if (inEditMode)
        {
            DestroyImmediate(roomParent);
            DestroyImmediate(enemyParent);
            DestroyImmediate(keyParent);
        }
        else
        {
            Destroy(roomParent);
            Destroy(enemyParent);
            Destroy(keyParent);
        }
        placedRooms.Clear();
        roomParent = new GameObject("Rooms");
        enemyParent = new GameObject("Enemies");
        keyParent = new GameObject("Keys");
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

        // Colour the rooms
        foreach (MeshRenderer obj in newRoom.GetComponentsInChildren<MeshRenderer>())
        {
            for (int tier = tieredMatValues.Length - 1; tier >= 0; tier--)
            {
                if (newRoomData.distanceFromCenter >= tieredMatValues[tier])
                {
                    if (obj.CompareTag("NonWall"))
                    {
                        obj.material = tieredMaterials[tier].nonWall;
                    }
                    else
                    {
                        obj.material = tieredMaterials[tier].wall;
                    }
                    break;
                }
            }
        }

        // Spawn enemies
        int numDirections = 0;
        foreach (bool dir in newRoomData.directions)
        {
            if (dir) { numDirections++; }
        }
        if (newRoomData.distanceFromCenter >= enemySpawnDistance && numDirections > 0 && !newRoomData.isConnector && UnityEngine.Random.Range(0f, 1f) < chanceOfEnemyPerRoom)
        {
            for (int i = 0; i < UnityEngine.Random.Range(numEnemiesPerRoomLow, numEnemiesPerRoomHigh); i++)
            {
                Instantiate(enemy, new Vector3(UnityEngine.Random.Range(newRoom.transform.position.x - roomSize / 2, newRoom.transform.position.x + roomSize / 2), 2.5f, UnityEngine.Random.Range(newRoom.transform.position.z - roomSize / 2, newRoom.transform.position.z + roomSize / 2)), Quaternion.identity, enemyParent.transform);
            }
        }

        placedRooms.Add(newRoomData);
        toPlace.Add(newRoom);
    }

    private void placeKeys()
    {
        List<RoomObject> possiblePlacements = new List<RoomObject>();

        foreach (RoomObject room in placedRooms) // Filter out invalid rooms
        {
            if (room.isConnector || room.gameObject == endingRoom || room.distanceFromCenter <= 1)
            {
                continue;
            }

            possiblePlacements.Add(room);
        }

        if (possiblePlacements.Count < 3)
        {
            Debug.LogError("Cannot place keys, need more valid rooms");
            return;
        }

        // Get furthest room distance
        int maxDist = 0;
        foreach (RoomObject room in possiblePlacements)
        {
            if (room.distanceFromCenter > maxDist)
            {
                maxDist = room.distanceFromCenter;
            }
        }

        // First key placed close, third key placed far
        List<RoomObject> firstArea = possiblePlacements.FindAll(room => room.distanceFromCenter >= maxDist * keyDistances[0] && room.distanceFromCenter <= maxDist * keyDistances[1]);
        List<RoomObject> secondArea = possiblePlacements.FindAll(room => room.distanceFromCenter >= maxDist * keyDistances[1] && room.distanceFromCenter <= maxDist * keyDistances[2]);
        List<RoomObject> thirdArea = possiblePlacements.FindAll(room => room.distanceFromCenter >= maxDist * keyDistances[2] && room.distanceFromCenter <= maxDist * 0.95f);

        RoomObject firstKeyLoc = firstArea[UnityEngine.Random.Range(0, firstArea.Count)];
        RoomObject secondKeyLoc = chooseFurthestFrom(secondArea, new List<RoomObject> { firstKeyLoc });
        RoomObject thirdKeyLoc = chooseFurthestFrom(thirdArea, new List<RoomObject> { firstKeyLoc, secondKeyLoc });

        Instantiate(key, !firstKeyLoc.isDeadEnd 
                    ? new Vector3(firstKeyLoc.transform.position.x, firstKeyLoc.transform.position.y + 3, firstKeyLoc.transform.position.z) 
                    : new Vector3(firstKeyLoc.transform.position.x, firstKeyLoc.transform.position.y + 3, firstKeyLoc.transform.position.z) + firstKeyLoc.transform.right.normalized * 5, Quaternion.identity, keyParent.transform);
        Instantiate(key, !secondKeyLoc.isDeadEnd 
                    ? new Vector3(secondKeyLoc.transform.position.x, secondKeyLoc.transform.position.y + 3, secondKeyLoc.transform.position.z)
                    : new Vector3(secondKeyLoc.transform.position.x, secondKeyLoc.transform.position.y + 3, secondKeyLoc.transform.position.z) + secondKeyLoc.transform.right.normalized * 5, Quaternion.identity, keyParent.transform);
        Instantiate(key, !thirdKeyLoc.isDeadEnd 
                    ? new Vector3(thirdKeyLoc.transform.position.x, thirdKeyLoc.transform.position.y + 3, thirdKeyLoc.transform.position.z)
                    : new Vector3(thirdKeyLoc.transform.position.x, thirdKeyLoc.transform.position.y + 3, thirdKeyLoc.transform.position.z) + thirdKeyLoc.transform.right.normalized * 5, Quaternion.identity, keyParent.transform);
    }

    private RoomObject chooseFurthestFrom(List<RoomObject> rooms, List<RoomObject> chosen)
    {
        RoomObject bestRoom = null;
        float best = -1f;

        foreach (RoomObject room in rooms)
        {
            float minDist = float.MaxValue;

            foreach (RoomObject c in chosen)
            {
                float dist = Vector3.Distance(room.transform.position, c.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                }
            }

            if (minDist > best)
            {
                best = minDist;
                bestRoom = room;
            }
        }

        return bestRoom;
    }
}