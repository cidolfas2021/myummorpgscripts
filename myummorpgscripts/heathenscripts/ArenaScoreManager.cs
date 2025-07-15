using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Collections.Generic;
using HeathenEngineering.SteamworksIntegration;
using UnityEngine.UIElements;
using GameZero;
using System.Text.RegularExpressions;
using System;
using TMPro;
using uMMORPG;
[DisallowMultipleComponent]
public class ArenaScoreManager : NetworkBehaviour
{
   
    //public static ArenaExitScript arenaexit;
    public static ArenaScoreManager singleton;
    private Player player;
    [SerializeField] private float arenaDuration = 1200f; //20 minute duration
    public Instance instance;                                                  //[SerializeField] private GameObject panel;
    [SerializeField] private int maxScore = 5; //Maximum number of kills required to win
    private float timeRemaining;
    private int redTeamScore = 0;
    private int blueTeamScore = 0;
    public bool gameOver = false;
    //public bool inmatch;
    //public Vector3 targetPos;
    private void Awake()
    {
        // Initialize singleton
        if (singleton == null) singleton = this;
    }
    private void Update()
    {
        

    }
    private void Start()
    {
        Debug.Log("has authority");
        // Get the entry positions for the instance
        

        
        //panel.SetActive(false);
        timeRemaining = arenaDuration;
        InvokeRepeating("UpdateTimer", 1f, 1f); //Update timer every second
    }

    [Command]
    public void CmdSetInMatch(Player player, bool value)
    {
        if (player != null)
        {
            //player.inmatch = value;
            Debug.Log("in match set on server");
        }
        else
        {
            Debug.LogWarning("Player is null in CmdSetInMatch");
        }
    }

    public void OnTriggerEnter(Collider co)
    {
        Debug.Log("Entered trigger zone");
        Player player = co.GetComponentInParent<Player>();

        if (player != null)
        {
            //player.inmatch = true;
            // Call the [Command] method to set inmatch on the server
            CmdSetInMatch(player, true); // Set inmatch to true
            Debug.Log("Player inmatch state set to true");
        }
        else
        {
            Debug.LogWarning("Player not found in OnTriggerEnter");
        }
        // Rest of your logic...
    }

    public void OnTriggerExit(Collider co)
    {
        Debug.Log("Exited trigger zone");
        Player player = co.GetComponentInParent<Player>();

        if (player != null)
        {
            /*player.inmatch = true;
            player.thismatchpks = 0;
            player.thismatchdeaths = 0;
            player.redkill = 0;
            player.bluekill = 0;
            player.blackkill = 0;
            player.whitekill = 0;*/
            CmdSetInMatch(player, false);
            Debug.Log("Player inmatch state set to false");
        }
        else
        {
            Debug.LogWarning("Player not found in OnTriggerExit");
        }
    }

  
    public void HandleDeath()
    {
        Player player = Player.localPlayer;
        Debug.Log("you died in the arena");
        /*
        // Ensure that entry1 and entry2 are properly initialized
        if (instance.entry1 != null && instance.entry2 != null)
        {
            Vector3 entry1Pos = instance.entry1.position;
            Debug.Log("entry 1 pos");
            Vector3 entry2Pos = instance.entry2.position;
            Debug.Log("entry 2 pos");

            // show while player is dead
            if (player != null && player.health.current == 0)
            {
                Vector3 targetPos = entry1Pos;
                Vector3 targetPos2 = entry2Pos;
                arenaexit.CmdWarpToExit(targetPos);
            }
        }
        else
        {
            Debug.LogError("Entry points are not properly initialized.");
        }*/
    }


    private void UpdateTimer()
    {
        if (gameOver) return;
        timeRemaining -= 1f;

        if (timeRemaining <= 0f)
        {
            // Display score panel when time is up
            gameOver = true;
            DisplayScorePanel();
        }
    }

   /* public void RegisterKill(string team)
    {
        
        Player player = Player.localPlayer;
        if (gameOver) return;



        if (player.redkill >= maxScore)
        {
            // Display score panel when red team reaches max score
            gameOver = true;
            DisplayScorePanel();
            Debug.Log("Red wins!");
        }



        else if (player.bluekill >= maxScore)
        {
            // Display score panel when blue team reaches max score
            gameOver = true;
            DisplayScorePanel();
            Debug.Log("Blue wins!");
        }

        else if (player.blackkill >= maxScore)
        {
            // Display score panel when blue team reaches max score
            gameOver = true;
            DisplayScorePanel();
            Debug.Log("Black wins!");
        }
        else if (player.whitekill >= maxScore)
        {
            // Display score panel when blue team reaches max score
            gameOver = true;
            DisplayScorePanel();
            Debug.Log("White wins!");
        }
    }*/

    private void DisplayScorePanel()
    {
        //panel.SetActive(true);
    }

    public void LeaveArena()
    {
        // TODO: Add code to handle leaving the arena
    }
}
