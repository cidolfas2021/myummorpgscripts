using UnityEngine;
using Mirror;
using System;


namespace GameZero
{
    [DisallowMultipleComponent]
    public class PlayerArenaNpcTeleport : NetworkBehaviour
    {
        [Header("Components")]
        public Player player;

        public static int partyId = -1; // initialize to -1 to indicate that it is not set yet
        
        private Vector3 targetPos;

        // Static variable to store the creator player
        private static Player creator;

        [Command(requiresAuthority = false)]
        public void CmdLobbyCreator()
        {
            Debug.Log("Player assigned: " + player.name);
            if (player != null)
            {
                PartySystem.FormSoloParty(player.name);
                partyId = player.party.party.partyId;
                Debug.Log($"partyId = {partyId}");
            }
        }

        [Command(requiresAuthority = false)]
        public void CmdLobbyJoiner()
        {
            if (player != null)
            {
                PartySystem.AddToParty(partyId, player.name); 
            }
        }





        [Command(requiresAuthority = false)]
        public void CmdPartySetup()
        {
            //ArenaManager am = GameObject.FindWithTag("GameManager").GetComponent<ArenaManager>();
            Instance instanceTemplate = ArenaManager.singleton.GetInstanceTemplate(ArenaManager.singleton.instanceId);

            Debug.Log("Cmdpartysetup called");
            Debug.Log("instanceTemplate = " + instanceTemplate);
            Player[] playersInParty = PartySystem.GetPlayersInParty(partyId);
            Debug.Log("Number of players in party: " + playersInParty.Length);
            foreach (Player player in playersInParty)
            {
                Debug.Log("player name = " + player.name);


                // loop through each player and do something with them
                Debug.Log("player isServer: " + player.isServer + " hasAuthority: " + player.isOwned + " isClient: " + player.isClient + " isLocalPlayer: " + player.isLocalPlayer);


                // collider might be in player's bone structure. look in parents.

                if (player != null)
                {
                    Debug.Log("player is not null");
                    // only call this for server and for local player. not for other
                    // players on the client. no need in locally creating their
                    // instances too.
                    if (player.isServer || player.isLocalPlayer)
                    {
                        Debug.Log("player is server or local player");
                        // required level?
                        if (player.level.current >= instanceTemplate.requiredLevel)
                        {
                            Debug.Log("player met level requirement");
                            // can only enter with a party
                            if (player.party.InParty())
                            {
                                Debug.Log("player is in a party");
                                // is there an instance for the player's party yet?
                                if (instanceTemplate.instances.TryGetValue(partyId, out Instance existingInstance))
                                {
                                    // teleport player to instance entry
                                    if (player.isServer)
                                    {
                                        Debug.Log("player has authority!!");
                                        Vector3 entry1Pos = existingInstance.entry1?.position ?? Vector3.zero;
                                        Vector3 entry2Pos = existingInstance.entry2?.position ?? Vector3.zero;

                                        // Determine the target position for the player based on the player index

                                        int playerIndex = Array.IndexOf(playersInParty, player);
                                        Vector3 targetPos = (playerIndex % 2 == 0) ? entry2Pos : entry1Pos;

                                        // Call the CmdWarpToEntry() method to move the player to the target position
                                        if (player.isServer || (player.isClient && player.isOwned))
                                        {
                                            // Call the CmdWarpDrive() method only if the player is owned by a client
                                            player.movement.Warp(targetPos);
                                        }
                                        Debug.Log("Teleporting " + player.name + " to existing instance=" + existingInstance.name + " with partyId=" + partyId);
                                    }

                                }
                                // otherwise create a new one
                                else
                                {
                                    Instance instance = Instance.CreateInstance(instanceTemplate, player.party.party.partyId);
                                    NetworkServer.Spawn(instance.gameObject);
                                    if (instance != null)
                                    {
                                        Debug.Log("instance is not null");
                                        // teleport player to instance entry
                                        Debug.Log("player isServer: " + player.isServer + " hasAuthority: " + player.isOwned + " isClient: " + player.isClient + " isLocalPlayer: " + player.isLocalPlayer + "isowned" + player.isOwned);

                                        if (player.isServer)
                                        {
                                            Debug.Log("has authority");
                                            // Get the entry positions for the instance
                                            Vector3 entry1Pos = instance.entry1?.position ?? Vector3.zero;
                                            Vector3 entry2Pos = instance.entry2?.position ?? Vector3.zero;

                                            // Determine the target position for the player based on the player index

                                            int playerIndex = Array.IndexOf(playersInParty, player);
                                            Vector3 targetPos = (playerIndex % 2 == 0) ? entry2Pos : entry1Pos;

                                            // Call the CmdWarpToEntry() method to move the player to the target position
                                            if (player.isServer || (player.isClient && player.isOwned))
                                            {
                                                if (player.GetComponent<NetworkIdentity>().isOwned)
                                                {
                                                    Debug.Log("client has authority for cmdwarpdrive");
                                                    // Check if client is still connected
                                                    if (!NetworkServer.connections.ContainsKey(player.GetComponent<NetworkIdentity>().connectionToClient.connectionId))
                                                    {
                                                        // Client has disconnected, do something
                                                        Debug.Log("client has disconnected, do something!");
                                                    }
                                                    else
                                                    {
                                                        // Call the CmdWarpDrive() method only if the player is owned by a client
                                                        player.movement.Warp(targetPos);
                                                        Debug.Log("player passed all checks, warping successfully");
                                                    }
                                                }
                                                Debug.Log("player didn't have authority, still warping just in case");
                                                player.movement.Warp(targetPos);
                                            }
                                            Debug.Log("Teleporting " + player.name + " to new instance=" + instance.name + " with partyId=" + player.party.party.partyId);
                                        }
                                        else { Debug.Log("player is not server!!"); }

                                    }
                                    else if (player.isServer) player.chat.TargetMsgInfo("There are already too many " + instance.name + " instances. Please try again later.");
                                }
                            }

                            else
                            {
                                Debug.LogError("No existing instance found!");
                            }
                        }
                    }

                }
            }

        }
    }
}