using UnityEngine;
using Mirror;
using System.Collections.Generic;
using HeathenEngineering.SteamworksIntegration;

namespace GameZero
{
    [DisallowMultipleComponent]
    public class ArenaExitScript : NetworkBehaviour
    {
        [Header("Components")]
        private Vector3 warpPosition = new Vector3(0f, 0f, 0f);
        public Player player;
        [SyncVar]public PlayerArenaNpcTeleport npcArenaTeleport;

        public void Update()
        {
            if (UIArenaNpcDialogue.singleton.gameEnded == true)
            {
                gameEnder();
            }
            if (UIArenaNpcDialogue.singleton.warpall == true)
            {
                warpall();
            }
        }

        public void gameEnder()
        {
            CheckPartyInstance();
            Invoke(nameof(SetBoolBack), .01f);
        }

        public void warpall()
        {
            CheckWarpall();
            Invoke(nameof(SetwarpallBoolBack), .01f);
        }

        private void SetBoolBack()
        {
            if (UIArenaNpcDialogue.singleton.gameEnded == true)
            {
                UIArenaNpcDialogue.singleton.gameEnded = false;
                Debug.Log("game ended reset to false");
            }
        }

        private void SetwarpallBoolBack()
        {
            if (UIArenaNpcDialogue.singleton.warpall == true)
            {
                UIArenaNpcDialogue.singleton.warpall = false;
                Debug.Log("warpall reset to false");
            }
        }

        public void CheckWarpall()
        {
            Debug.Log("we made it this far");
            if (player != null)
            {
                // check party again, just to be sure.
                Debug.Log("player is not null");

                // is there an instance for the player's party yet?
                CmdWarpAllToExit(warpPosition, PlayerArenaNpcTeleport.partyId);
            }
        }

        public void CheckPartyInstance()
        {
            Debug.Log("we made it this far");
            if (player != null)
            {
                // check party again, just to be sure.
                Debug.Log("player is not null");

                // is there an instance for the player's party yet?
                CmdWarpToExit(new Vector3(0, 0, 0));
            }
        }

        [Command]
        void CmdWarpToExit(Vector3 position)
        {
            player.movement.Warp(new Vector3(0, 0, 0));
        }

        [Command(requiresAuthority = false)]
        void CmdWarpAllToExit(Vector3 warpPosition, int partyId)
        {
            Debug.Log("cmdwarpalltoexit reached");
            Debug.Log($"partyId = {partyId}");
            Player[] players = PartySystem.GetPlayersInParty(partyId);
            Debug.Log("players in party defined");
            if (players != null && players.Length > 0)
            {
                foreach (Player player in players)
                {
                    player.movement.Warp(warpPosition);
                    Debug.Log("warping all");
                }
            }
            else
            {
                Debug.Log("No players found in the party.");
            }
        }
    }
}