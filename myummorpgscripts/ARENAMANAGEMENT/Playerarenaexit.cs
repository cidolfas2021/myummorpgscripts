using UnityEngine;
using Mirror;
using System.Collections.Generic;
using GameZero;
//using HeathenEngineering.SteamworksIntegration;

namespace uMMORPG
{
   
    [DisallowMultipleComponent]
    public class Playerarenaexit : NetworkBehaviour
    {
        [Header("Components")]
        private Vector3 position = new Vector3(0f, 3f, 0f);
        public Player player;

        [Command]
        public void CmdWarpToExit(Vector3 position)
        {
            player.movement.Warp(position); // Use the provided 'position' parameter
            Debug.Log("Warping to position: " + position);
        }


    }
}