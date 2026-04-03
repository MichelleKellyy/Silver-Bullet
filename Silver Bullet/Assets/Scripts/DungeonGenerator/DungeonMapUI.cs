using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class DungeonMapUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CreateDungeon dungeonGenerator;
    [SerializeField] private Transform player;
    [SerializeField] private Transform orientation;
    [SerializeField] private GameObject mapPanel;
    [SerializeField] private RawImage mapImage;
    [SerializeField] private RectTransform playerIcon;

    [Header("Map Settings")]
    [SerializeField] private KeyCode mapKey = KeyCode.M;
    [SerializeField] private int pixelsPerCell = 12;
    [SerializeField] private Color hiddenColour = new Color(0f, 0f, 0f, 0f);
    [SerializeField] private Color roomColour = Color.white;
    [SerializeField] private Color corridorColour = new Color(0.7f, 0.7f, 0.7f, 1f);
    [SerializeField] private Color startRoomColour = new Color(0.6f, 1f, 0.6f, 1f);
    [SerializeField] private Color endRoomColour = new Color(1f, 0.7f, 0.4f, 1f);
    [SerializeField] private int corridorThickness = 4;

    private Texture2D mapTex;
    private Dictionary<Vector2Int, RoomObject> roomDict = new Dictionary<Vector2Int, RoomObject>();
    private HashSet<Vector2Int> discoveredCells = new HashSet<Vector2Int>();

    private RoomObject currentRoom;
    private int dungeonSize;
    private bool initialized;

    private void OnEnable()
    {
        dungeonGenerator.OnDungeonGenerated += buildMapFromDungeon;
    }
    private void OnDisable()
    {
        dungeonGenerator.OnDungeonGenerated -= buildMapFromDungeon;
    }

    private void Start()
    {
        buildMapFromDungeon();
    }

    private void Update()
    {
        if (Input.GetKeyDown(mapKey))
        {
            mapPanel.SetActive(!mapPanel.activeSelf);
        }

        if (!initialized)
        {
            return;
        }

        updateCurrentRoom();
        updatePlayerIcon();
    }

    public void buildMapFromDungeon()
    {
        RoomObject startRoom = dungeonGenerator.getStartingRoom();

        dungeonSize = dungeonGenerator.getDungeonSize();

        roomDict.Clear();
        discoveredCells.Clear();

        addRoomToDict(startRoom);

        IReadOnlyList<RoomObject> rooms = dungeonGenerator.getPlacedRooms();
        for (int i = 0; i < rooms.Count; i++)
        {
            addRoomToDict(rooms[i]);
        }

        int texSize = dungeonSize * pixelsPerCell;
        mapTex = new Texture2D(texSize, texSize, TextureFormat.RGBA32, false);
        mapTex.filterMode = FilterMode.Point;
        mapTex.wrapMode = TextureWrapMode.Clamp;

        clearTexture();
        mapTex.Apply();

        mapImage.texture = mapTex;

        discoverCell(new Vector2Int(startRoom.x, startRoom.z));

        currentRoom = startRoom;
        initialized = true;

        updateCurrentRoom();
        updatePlayerIcon();
    }

    private void addRoomToDict(RoomObject room)
    {
        Vector2Int key = new Vector2Int(room.x, room.z);
        if (!roomDict.ContainsKey(key))
        {
            roomDict.Add(key, room);
        }
        else
        {
            roomDict[key] = room;
        }
    }

    private void clearTexture()
    {
        Color[] pixels = new Color[mapTex.width * mapTex.height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = hiddenColour;
        }

        mapTex.SetPixels(pixels);
    }

    private void updateCurrentRoom()
    {
        RoomObject nearest = getNearestRoomToPlayer();

        if (nearest != currentRoom)
        {
            currentRoom = nearest;
            discoverCell(new Vector2Int(currentRoom.x, currentRoom.z));
        }
    }

    private RoomObject getNearestRoomToPlayer()
    {
        RoomObject bestRoom = null;
        float bestDist = float.MaxValue;

        foreach (RoomObject room in roomDict.Values)
        {
            Vector3 roomPos = room.transform.position;
            Vector3 playerPos = player.position;

            float dx = roomPos.x - playerPos.x;
            float dz = roomPos.z - playerPos.z;
            float sqrDist = dx * dx + dz * dz;

            if (sqrDist < bestDist)
            {
                bestDist = sqrDist;
                bestRoom = room;
            }
        }

        return bestRoom;
    }

    private static readonly Vector2Int[] directionOffsets =
    {
        new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0)
    };

    private void revealConnectedCorridors(RoomObject room)
    {
        if (room == null || room.isConnector)
        {
            return;
        }

        for (int i = 0; i < room.directions.Length; i++)
        {
            if (!room.directions[i])
            {
                continue;
            }

            Vector2Int neighborCell = new Vector2Int(room.x, room.z) + directionOffsets[i];

            if (roomDict.TryGetValue(neighborCell, out RoomObject neighbor) && neighbor != null && neighbor.isConnector)
            {
                if (!discoveredCells.Contains(neighborCell))
                {
                    discoveredCells.Add(neighborCell);
                    drawCell(neighborCell);
                }
            }
        }
    }

    private void discoverCell(Vector2Int cell)
    {
        if (!roomDict.TryGetValue(cell, out RoomObject room))
        {
            return;
        }

        bool changed = false;

        if (!discoveredCells.Contains(cell))
        {
            discoveredCells.Add(cell);
            drawCell(cell);
            changed = true;
        }

        if (!room.isConnector)
        {
            int before = discoveredCells.Count;
            revealConnectedCorridors(room);
            if (discoveredCells.Count > before)
            {
                changed = true;
            }
        }

        if (changed)
        {
            mapTex.Apply();
        }
    }

    private void drawCell(Vector2Int cell)
    {
        if (!roomDict.TryGetValue(cell, out RoomObject room))
        {
            return;
        }

        Color drawColour = roomColour;

        if (room == dungeonGenerator.getStartingRoom())
        {
            drawColour = startRoomColour;
        }
        else if (room.isEndingRoom)
        {
            drawColour = endRoomColour;
        }
        else if (room.isConnector)
        {
            drawColour = corridorColour;
        }

        int startX = cell.x * pixelsPerCell;
        int startY = cell.y * pixelsPerCell;

        if (!room.isConnector)
        {
            for (int x = 0; x < pixelsPerCell; x++)
            {
                for (int y = 0; y < pixelsPerCell; y++)
                {
                    mapTex.SetPixel(startX + x, startY + y, drawColour);
                }
            }

            return;
        }

        int thickness = Mathf.Clamp(corridorThickness, 1, pixelsPerCell);
        int centerOffset = (pixelsPerCell - thickness) / 2;

        bool north = room.directions[0];
        bool east = room.directions[1];
        bool south = room.directions[2];
        bool west = room.directions[3];

        bool vertical = north || south;
        bool horizontal = east || west;

        if (vertical && !horizontal)
        {
            for (int x = 0; x < thickness; x++)
            {
                for (int y = 0; y < pixelsPerCell; y++)
                {
                    mapTex.SetPixel(startX + centerOffset + x, startY + y, drawColour);
                }
            }
        }
        else if (horizontal && !vertical)
        {
            for (int x = 0; x < pixelsPerCell; x++)
            {
                for (int y = 0; y < thickness; y++)
                {
                    mapTex.SetPixel(startX + x, startY + centerOffset + y, drawColour);
                }
            }
        }
        else
        {
            for (int x = 0; x < pixelsPerCell; x++)
            {
                for (int y = 0; y < pixelsPerCell; y++)
                {
                    mapTex.SetPixel(startX + x, startY + y, drawColour);
                }
            }
        }
    }

    private void updatePlayerIcon()
    {
        Rect rect = mapImage.rectTransform.rect;
        float width = rect.width;
        float height = rect.height;

        float cellWidth = width / dungeonSize;
        float cellHeight = height / dungeonSize;

        float x = (currentRoom.x + 0.5f) * cellWidth - width * 0.5f;
        float y = (currentRoom.z + 0.5f) * cellHeight - height * 0.5f;

        playerIcon.anchoredPosition = new Vector2(x, y);

        float rot = orientation.eulerAngles.y;
        playerIcon.localEulerAngles = new Vector3(180f, 0f, rot);
    }
}
