using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Mirror;
namespace GameZero
{
    [DisallowMultipleComponent]
    public class ArenaManager : NetworkBehaviour
    {
        [SerializeField] private bool allowSolo;
        [SerializeField] private Instance arena1;
        [SerializeField] private Instance arena2;
        [SerializeField] private Instance arena3;
        [SerializeField] private Instance arena4;
        [SerializeField] private GameObject preview1Prefab;
        [SerializeField] private GameObject preview2Prefab;
        [SerializeField] private GameObject preview3Prefab;
        [SerializeField] private GameObject preview4Prefab;
        [SyncVar] public int instanceId = -1;
        private RawImage selectedPreview;
        private Instance selectedArena;
        public static ArenaManager singleton;

        private void Awake()
        {
            // Initialize singleton
            if (singleton == null) singleton = this;
        }

        private void Start()
        {
            // Hide all preview images at start
            preview1Prefab.SetActive(false);
            preview2Prefab.SetActive(false);
            preview3Prefab.SetActive(false);
            preview4Prefab.SetActive(false);
        }

        [Command(requiresAuthority = false)]
        public void CmdSetSelectedArena(int instanceId)
        {
            Instance arena = GetInstanceTemplate(instanceId);
            this.instanceId = instanceId;
            if (arena != null)
            {
                // Update the preview images on all clients
                RpcUpdatePreviewImages(instanceId);
            }
        }

        [ClientRpc]
        public void RpcUpdatePreviewImages(int instanceId)
        {
            Instance arena = GetInstanceTemplate(instanceId);
            if (arena != null)
            {
                GameObject previewPrefab = GetPreviewPrefabForArena(arena);

                // Update the selected arena on the client only
                selectedArena = arena;
                selectedPreview = previewPrefab.GetComponent<RawImage>();
                

                // Show the preview image of the selected arena
                previewPrefab.SetActive(true);

                // Hide the preview images of other arenas
                if (arena != arena1) preview1Prefab.SetActive(false);
                if (arena != arena2) preview2Prefab.SetActive(false);
                if (arena != arena3) preview3Prefab.SetActive(false);
                if (arena != arena4) preview4Prefab.SetActive(false);
            }
        }


        public void OnArena1ButtonClicked()
        {
            Debug.Log("Arena 1 selected");
            CmdSetSelectedArena(arena1.instanceId);

        }

        public void OnArena2ButtonClicked()
        {
            Debug.Log("Arena 2 selected");
            CmdSetSelectedArena(arena2.instanceId);

        }

        public void OnArena3ButtonClicked()
        {
            Debug.Log("Arena 3 selected");
            CmdSetSelectedArena(arena3.instanceId);

        }

        public void OnArena4ButtonClicked()
        {
            Debug.Log("Arena 4 selected");
            CmdSetSelectedArena(arena4.instanceId);

        }

        public void OnRandomArenaButtonClicked()
        {
            Instance[] arenas = new Instance[] { arena1, arena2, arena3, arena4 };

            int index = Random.Range(0, arenas.Length);

            CmdSetSelectedArena(arenas[index].instanceId);

        }
        private GameObject GetPreviewPrefabForArena(Instance arena)
        {
            if (arena == arena1)
                return preview1Prefab;
            else if (arena == arena2)
                return preview2Prefab;
            else if (arena == arena3)
                return preview3Prefab;
            else if (arena == arena4)
                return preview4Prefab;
            else
                return null;
        }

        public void CreateInstance()
        {
            if (selectedArena != null)
            {
                arenaonoff.onoff.SetArena(selectedArena.arenaName);
                arenaonoff.onoff.StartArena();
            }
            else
            {
                Debug.LogError("No selected arena!");
            }
        }

        public Instance GetInstanceTemplate(int instanceId)
        {
            Debug.Log("GetInstanceTemplate called with instanceId = " + instanceId);
            List<Instance> availableInstances = new List<Instance> { arena1, arena2, arena3, arena4 };

            foreach (Instance availableInstance in availableInstances)
            {
                Debug.Log("Checking instance: " + availableInstance.instanceId);
                if (availableInstance.instanceId == instanceId)
                {
                    Debug.Log("Found instance: " + availableInstance);
                    return availableInstance;
                }
            }

            Debug.Log("Instance not found for instanceId = " + instanceId);
            return null;
        }


    }
}