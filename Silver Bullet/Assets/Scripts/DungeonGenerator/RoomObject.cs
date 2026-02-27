using UnityEngine;

public class RoomObject : MonoBehaviour
{
    public bool[] directions = { false, false, false, false }; // { North, East, South, West }
    public bool isConnector; // Is a hallway between rooms

    // Map positions
    [HideInInspector] public int x;
    [HideInInspector] public int z;

    // Number of rooms between this and the starting room
    [HideInInspector] public int distanceFromCenter;

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
