using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Collections.Generic;
using HeathenEngineering.SteamworksIntegration;
namespace GameZero
{
    [DisallowMultipleComponent]
    public class ArenaScoreManager : NetworkBehaviour
    {
        public static ArenaScoreManager singleton;
        public Player player;
        [SerializeField] private float arenaDuration = 1200f; //20 minute duration
        [SerializeField] private GameObject scorePanelPrefab;
        [SerializeField] private int maxScore = 5; //Maximum number of kills required to win
        private float timeRemaining;
        private int redTeamScore = 0;
        private int blueTeamScore = 0;
        private bool gameOver = false;
        
        private void Awake()
        {
            // Initialize singleton
            if (singleton == null) singleton = this;
        }

        private void Start()
        {
            timeRemaining = arenaDuration;
            InvokeRepeating("UpdateTimer", 1f, 1f); //Update timer every second
        }
        void HandleDeath(int partyId)
        {
            if (partyId == 0)
            {
                // Team 1 has no players left, so team 2 wins
                Debug.Log("Team 2 wins!");
            }
            else if (partyId == 0)
            {
                // Team 2 has no players left, so team 1 wins
                Debug.Log("Team 1 wins!");
            }
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

        public void RegisterKill(string team)
        {
            if (gameOver) return;

            if (team == "red")
            {
                redTeamScore++;
                if (redTeamScore >= maxScore)
                {
                    // Display score panel when red team reaches max score
                    gameOver = true;
                    DisplayScorePanel();
                }
            }
            else if (team == "blue")
            {
                blueTeamScore++;
                if (blueTeamScore >= maxScore)
                {
                    // Display score panel when blue team reaches max score
                    gameOver = true;
                    DisplayScorePanel();
                }
            }
        }

        private void DisplayScorePanel()
        {
            // Instantiate the score panel prefab
            GameObject scorePanelGO = Instantiate(scorePanelPrefab, transform);

            // Get the ScorePanel component of the instantiated prefab
            Leaderboard scorePanel = scorePanelGO.GetComponent<Leaderboard>();

            // Update the score panel with the final scores
            //scorePanel.UpdateScore(redTeamScore, blueTeamScore);

            //disable movement.  somehow...
        }

        public void LeaveArena()
        {
            // TODO: Add code to handle leaving the arena
        }
    }
}
