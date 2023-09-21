using UnityEngine;
using Mirror;
using System.Collections.Generic;
using HeathenEngineering.SteamworksIntegration;

namespace GameZero
{
    [DisallowMultipleComponent]
    public class Arenacompletionscript : NetworkBehaviour
    {
        [Header("Components")]
        private Vector3 warpPosition = new Vector3(0f, 3f, 0f);
        //public Player player;
        //[SyncVar]public PlayerArenaNpcTeleport npcArenaTeleport;

        public void OnButtonClick()
        {
            Player player = Player.localPlayer;
            player.movement.Warp(warpPosition);
        }
    }
}