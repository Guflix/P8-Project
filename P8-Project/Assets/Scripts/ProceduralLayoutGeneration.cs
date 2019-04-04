﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralLayoutGeneration : MonoBehaviour
{
    /// Public variables, visible in the Inspector
    public GameObject[] startRooms, endRooms, rooms; // start rooms only contain EXIT portals, end rooms only contain ENTRY portals, rooms contain both
    public string entryPortalTag = "EntryPortal";   // Public, in case it needs to be changed later
    public string exitPortalTag = "ExitPortal";
    public int maxRooms = 99;    // CURRENTLY OBSELETE, but can be used to limit the max amount of rooms later...

    /// Public variables, hidden from the Inspector. Use keyword [HideInInspector] before every variable!
    //[HideInInspector]
    public List<GameObject> layoutList; // Public, since other scripts need access to it
    [HideInInspector]
    public int currentRoom = 0;

    /// Private variables
    private int roomsUsed = 0;
    private int setNextLayer = 8;
    private int uniqueIterator;

    /* We use awake as it is called before start, and this must always be called exactly once.
     * Note that if we want to be able to call this function multiple times, like if we want to
     * restart without closing, it should be a Start() function instead, since start will run when
     * an object is set to active, while Awake will run when the object has been initialized by Unity.
     */
    private void Awake()
    {
        layoutList = new List<GameObject>();    // We choose to use a list since we don't know the final size of the layout
        GenerateStartRoom(); // Randomly generate a starting room, by instantiating a room from the startRooms array
        GenerateLayout();
        GenerateEndRoom();
    }

    private void GenerateStartRoom() // No need to rotate start room
    {
        layoutList.Add(Instantiate(startRooms[Random.Range(0, startRooms.Length - 1)], new Vector3(0f, 0f, 0f), Quaternion.identity)); // Set a start room
        layoutList[0].layer = setNextLayer;
        Utils.ChangeLayersRecursively(layoutList[0].transform, setNextLayer);
    }

    private void GenerateLayout()
    {
        //Utils.RandomizeArray(rooms);
        for (int i = 1; i < maxRooms; i++) // Iterate over layout
        {
            List<Transform> portalsInLastRoomList = Utils.GetPortalTransformsInRoom(layoutList[i - 1], exitPortalTag);
            for (int j = 0; j < rooms.Length; j++) // Iterate over rooms
            {
                List<Transform> portalsInNewRoomList = Utils.GetPortalTransformsInRoom(rooms[j], entryPortalTag, exitPortalTag);
                List<Vector3> ninetyDegPortalsInNewRoomList = Utils.GetPortalPositionsInRoom(rooms[j], entryPortalTag, exitPortalTag, 90.0f);
                List<Vector3> oneEightyDegPortalsInNewRoomList = Utils.GetPortalPositionsInRoom(rooms[j], entryPortalTag, exitPortalTag, 180.0f);
                List<Vector3> twoSeventyDegPortalsInNewRoomList = Utils.GetPortalPositionsInRoom(rooms[j], entryPortalTag, exitPortalTag, 270.0f);
                int containedPortals = 0;
                float rotationParameter = 0.0f;
                for (int k = 0; k < portalsInLastRoomList.Count; k++)
                {
                    for (int l = 0; l < portalsInNewRoomList.Count; l++)
                    {
                        if (portalsInLastRoomList[k].tag != portalsInNewRoomList[l].tag)
                        {
                            float newPortalRot = portalsInNewRoomList[l].eulerAngles.y;
                            if (portalsInLastRoomList[k].position == portalsInNewRoomList[l].position &&
                                portalsInLastRoomList[k].eulerAngles.y != newPortalRot)          // Check for world position
                            {
                                containedPortals++;
                            }
                            else if (portalsInLastRoomList[k].position == ninetyDegPortalsInNewRoomList[l] &&
                                portalsInLastRoomList[k].eulerAngles.y != newPortalRot + 90.0f)    // Check for world position when rotated by 90 degrees
                            {
                                containedPortals++;
                                rotationParameter = 90.0f;
                            }
                            else if (portalsInLastRoomList[k].position == oneEightyDegPortalsInNewRoomList[l] &&
                                portalsInLastRoomList[k].eulerAngles.y != newPortalRot + 180.0f)
                            {
                                containedPortals++;
                                rotationParameter = 180.0f;
                            }
                            else if (portalsInLastRoomList[k].position == twoSeventyDegPortalsInNewRoomList[l] &&
                                portalsInLastRoomList[k].eulerAngles.y != newPortalRot + 270.0f)
                            {
                                containedPortals++;
                                rotationParameter = 270.0f;
                            }
                        }
                    }
                }
                if (containedPortals == 1)
                {
                    setNextLayer++;
                    layoutList.Add(Instantiate(rooms[j], Utils.worldSpacePoint, Quaternion.Euler(0.0f, rotationParameter, 0.0f)));
                    layoutList[roomsUsed + 1].layer = setNextLayer;
                    Utils.ChangeLayersRecursively(layoutList[roomsUsed + 1].transform, setNextLayer);
                    Utils.SetActiveChild(layoutList[roomsUsed + 1].transform, false, entryPortalTag, exitPortalTag);
                    rooms = Utils.RemoveIndices(rooms, j);
                    roomsUsed++;
                    if (layoutList.Count > 2) // Only the first two rooms should be active on start
                    {
                        layoutList[i].SetActive(false);
                    }
                    break; // Breaks from the current for loop
                }
                if (j == rooms.Length - 1) // Last iteration
                {
                    return; // Breaks from both for loops since they are inside a method
                }
            }
        }
    }

    private void GenerateEndRoom()
    {
        setNextLayer++;
        List<Transform> portalsInLastRoomList = Utils.GetPortalTransformsInRoom(layoutList[roomsUsed], exitPortalTag); // Stores portal from previous room in a list
        Utils.RandomizeArray(endRooms);
        /// End room
        for (int i = 0; i < endRooms.Length; i++) // Iterate over rooms
        {
            List<Transform> portalsInEndRoomList = Utils.GetPortalTransformsInRoom(endRooms[i], entryPortalTag);
            List<Vector3> ninetyDegPortalsInEndRoomList = Utils.GetPortalPositionsInRoom(endRooms[i], entryPortalTag, 90.0f);
            List<Vector3> oneEightDegPortalsInEndRoomList = Utils.GetPortalPositionsInRoom(endRooms[i], entryPortalTag, 180.0f);
            List<Vector3> twoSeventyDegPortalsInEndRoomList = Utils.GetPortalPositionsInRoom(endRooms[i], entryPortalTag, 270.0f);
            float rotationParameter = 0;
            bool connectedPortal = false;
            
            /// Checks whether exactly 1 of the portals in the room has the same position as exactly 1 portal in the previous room in the layout.
            for (int j = 0; j < portalsInLastRoomList.Count; j++)
            {
                for (int k = 0; k < portalsInEndRoomList.Count; k++)
                {
                    if (portalsInLastRoomList[j].eulerAngles != portalsInEndRoomList[k].eulerAngles &&  // Check for rotation
                    portalsInLastRoomList[j].tag != portalsInEndRoomList[k].tag)                        // Check for tag
                    {
                        if (portalsInLastRoomList[j].position == portalsInEndRoomList[k].position)      // Check for world position
                        {
                            connectedPortal = true;
                        }
                        else if (portalsInLastRoomList[j].position == ninetyDegPortalsInEndRoomList[k])
                        {
                            rotationParameter = 90.0f;
                            connectedPortal = true;
                        }
                        else if (portalsInLastRoomList[j].position == oneEightDegPortalsInEndRoomList[k])
                        {
                            rotationParameter = 180.0f;
                            connectedPortal = true;
                        }
                        else if (portalsInLastRoomList[j].position == twoSeventyDegPortalsInEndRoomList[k])
                        {
                            rotationParameter = 270.0f;
                            connectedPortal = true;
                        }
                    }
                    if (connectedPortal)
                    {
                        layoutList.Add(Instantiate(endRooms[i], Utils.worldSpacePoint, Quaternion.Euler(0.0f, rotationParameter, 0.0f)));
                        layoutList[roomsUsed + 1].layer = setNextLayer;
                        Utils.SetActiveChild(layoutList[roomsUsed + 1].transform, false, entryPortalTag, exitPortalTag);
                        Utils.ChangeLayersRecursively(layoutList[roomsUsed + 1].transform, setNextLayer);
                        return; // Breaks from the function
                    }
                }
            }
        }
    }
}