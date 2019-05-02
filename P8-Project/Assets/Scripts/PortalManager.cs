﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalManager : MonoBehaviour
{

    /// Inspector variables
    Shader ClearP;
    Shader ClearN;
    StencilController stencil;
    public ProceduralLayoutGeneration layout;
    public Material skyboxFantasy;
    public Material skyboxScifi;

    //Clear Depth Buffer Materials - Can be Shrunk 
    public Material NextFan;
    public Material NextSci;
    public Material PrevFan;
    public Material PrevSci;


    //public Cubemap skyboxFantasy;
    //public Cubemap skyboxScifi;
    public GameObject SkyBox;
    public GameObject NextDB;
    public GameObject PrevDB;

    /// Public, non-inspector variables

    /// Private variables
    public bool singlePortalCollision = false, fantasy = true, scifi = false, themeChange = false;
    private int portalExitScenario = 0; // Default is 0: do nothing
    private Vector3 backwardPortalPos, lastPortalPos;
    private Vector3 playerExitPosition;

    private float zeroF = 0.0f, ninetyF = 90.0f, oneEightyF = 180.0f, twoSeventyF = 270.0f;
    private string stencilTag = "Stencil", oppositeStencilTag = "OppositeStencil";

    void Start()
    {

        stencil = GetComponent<StencilController>();// Script that handles which layer is rendered by which camera
        RenderSettings.skybox = skyboxFantasy;
    }

    private void OnTriggerEnter(Collider portal)
    {
        if (!singlePortalCollision)
        {
            Utils.SetSiblingPortalActivity(portal.transform, false, layout.entryPortalTag, layout.exitPortalTag);
            if (portal.tag == layout.exitPortalTag && layout.currentRoom < layout.layoutList.Count - 1) // Exit is the exit of the room
            {
                layout.currentRoom++;
                portalExitScenario = 1;
            }
            else if (portal.tag == layout.entryPortalTag && layout.currentRoom > 0) // Entry is the entry of the room
            {
                layout.currentRoom--;
                portalExitScenario = 2;
            }

            if (portalExitScenario == 1)
            {
                if (layout.currentRoom < layout.layoutList.Count - 1)
                    layout.layoutList[layout.currentRoom + 1].SetActive(true);
                if (layout.currentRoom > 1)
                    layout.layoutList[layout.currentRoom - 2].SetActive(false);
            }
            else if (portalExitScenario == 2)
            {
                if (layout.currentRoom > 0)
                    layout.layoutList[layout.currentRoom - 1].SetActive(true);
                if (layout.currentRoom < layout.layoutList.Count - 2)
                    layout.layoutList[layout.currentRoom + 2].SetActive(false);
            }
            stencil.SetStencilShader(layout.currentRoom);
            Utils.SetActiveChild(portal.transform, false, stencilTag);
            Utils.SetActiveChild(portal.transform, true, oppositeStencilTag);
            singlePortalCollision = true;
        }
    }

    private void OnTriggerExit(Collider portal) // Out of portal
    {
        playerExitPosition = transform.position;
        /// Checks for portal's rotation, and the player's exit position to see if they exited on the same side as they entered from. 
        if ((Mathf.Round(portal.transform.eulerAngles.y) == zeroF && playerExitPosition.z >= portal.transform.position.z) ||
            (Mathf.Round(portal.transform.eulerAngles.y) == oneEightyF && playerExitPosition.z <= portal.transform.position.z) ||
            (Mathf.Round(portal.transform.eulerAngles.y) == ninetyF && playerExitPosition.x >= portal.transform.position.x) ||
            (Mathf.Round(portal.transform.eulerAngles.y) == twoSeventyF && playerExitPosition.x <= portal.transform.position.x))
        {
            //Debug.Log("Portal angles: " + Mathf.Round(portal.transform.eulerAngles.y) + ". Player exit pos x: " + playerExitPosition.x + ", portal pos x: " + portal.transform.position.x +
            //   ". Player exit pos z: " + playerExitPosition.z + ", portal pos z: " + portal.transform.position.z);
            //Debug.Log("Correctly passed through portal");
            Transition(portal, true);
        }
        else
        {
            Debug.Log("Portal angles: " + Mathf.Round(portal.transform.eulerAngles.y) + ". Player exit pos x: " + playerExitPosition.x + ", portal pos x: " + portal.transform.position.x +
               ". Player exit pos z: " + playerExitPosition.z + ", portal pos z: " + portal.transform.position.z);
            Debug.Log("Incorrectly passed through portal");
            Transition(portal, false);
        }
        singlePortalCollision = false;
    }

    private void Transition(Collider portal, bool passing)
    {
        /// Reset portal's stencils to default state
        Utils.SetActiveChild(portal.transform, true, stencilTag);
        Utils.SetActiveChild(portal.transform, false, oppositeStencilTag);
        Utils.SetSiblingPortalActivity(portal.transform, true, layout.entryPortalTag, layout.exitPortalTag);
        if (passing)
        {
            Utils.SetActivePortal(layout.layoutList[layout.currentRoom].transform, true, layout.entryPortalTag, layout.exitPortalTag); // Enable portals in new room, in case they are disabled.

            if (portalExitScenario == 1) // Scenario 1: Enter "next-room" portal
            {
                Utils.SetActivePortal(layout.layoutList[layout.currentRoom - 1].transform, false, layout.entryPortalTag, layout.exitPortalTag); // Since we enabled new portals, we should disable the existing ones.
            }
            else if (portalExitScenario == 2) // Scenario 2: Enter "previous-room" portal
            {
                Utils.SetActivePortal(layout.layoutList[layout.currentRoom + 1].transform, false, layout.entryPortalTag, layout.exitPortalTag); // Since we enabled new portals, we should disable the existing ones.

            }

            if (layout.currentRoom >= (layout.layoutList.Count / 2 - 2) && fantasy)
            {
                ThemeChangeScifi();
            }

            if (layout.currentRoom < (layout.layoutList.Count / 2 - 2) && scifi)
            {
                ThemeChangeFantasy();
            }
        }
        else
        {
            Debug.Log("Detected false passing through portal");
            if (portal.tag == layout.exitPortalTag && layout.currentRoom > 0) // Exit is the exit of the room
            {
                layout.currentRoom--;
                portalExitScenario = 2;
            }
            else if (portal.tag == layout.entryPortalTag && layout.currentRoom < layout.layoutList.Count - 1) // Entry is the entry of the room
            {
                layout.currentRoom++;
                portalExitScenario = 1;
            }

            if (portalExitScenario == 1)
            {
                if (layout.currentRoom < layout.layoutList.Count - 1)
                    layout.layoutList[layout.currentRoom + 1].SetActive(true);
                if (layout.currentRoom > 1)
                    layout.layoutList[layout.currentRoom - 2].SetActive(false);
            }
            else if (portalExitScenario == 2)
            {
                if (layout.currentRoom > 0)
                    layout.layoutList[layout.currentRoom - 1].SetActive(true);
                if (layout.currentRoom < layout.layoutList.Count - 2)
                    layout.layoutList[layout.currentRoom + 2].SetActive(false);
            }
            stencil.SetStencilShader(layout.currentRoom);
        }
    }

    private void ThemeChangeScifi()
    {
        SkyBox.GetComponent<Renderer>().material = skyboxScifi;
        NextDB.GetComponent<Renderer>().material = NextSci;
        PrevDB.GetComponent<Renderer>().material = PrevSci;
        //RenderSettings.skybox = skyboxScifi;
        fantasy = false;
        scifi = true;
    }

    private void ThemeChangeFantasy()
    {
        SkyBox.GetComponent<Renderer>().material = skyboxFantasy;
        NextDB.GetComponent<Renderer>().material = NextFan;
        PrevDB.GetComponent<Renderer>().material = PrevFan;
        //RenderSettings.skybox = skyboxFantasy;
        fantasy = true;
        scifi = false;
    }


}

