using UnityEngine;

public class RoomObject : MonoBehaviour
{
    [Header("North (+Z), East (+X), South, West")]
    public bool[] directions = { false, false, false, false }; // { North, East, South, West }
    public bool isConnector; // Is a hallway between rooms
    public bool isDeadEnd;
    [SerializeField] private bool isStartingRoom;
    [HideInInspector] public bool isEndingRoom;

    // Map positions
    [HideInInspector] public int x;
    [HideInInspector] public int z;

    // Number of rooms between this and the starting room
    public int distanceFromCenter;

    private void Awake()
    {
        if (!isConnector && !isStartingRoom)
        {
            for (int i = 0; i < Random.Range(0, 4); i++)
            {
                rotateRoom();
            }
        }
    }

    // Rotates the room 90 degrees clockwise
    public void rotateRoom()
    {
        bool originalWest = directions[directions.Length - 1];
        for (int i = directions.Length - 1; i > 0; i--)
        {
            directions[i] = directions[i - 1];
        }
        directions[0] = originalWest;

        transform.eulerAngles += new Vector3(0, 90, 0);
    }
}
