using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameZero
{
    public class arenaonoff : MonoBehaviour
    {
        public static arenaonoff onoff;
        public bool arenaon;
        private GameObject arena;
        private bool isInstantiated = false;
        private bool previousArenaOnState;

        private void Start()
        {
            DontDestroyOnLoad(gameObject); // Ensure this persists across scenes//Do not put objects in DontDestroyOnLoad (DDOL) in Awake. You can do that in Start instead.
        }
        void Awake()
        {
            // Initialize singleton
            if (onoff == null)
            {
                onoff = this;
               
            }
            else
            {
                Destroy(gameObject); // Ensure only one instance exists
            }
        }

        void Update()
        {
            if (!isInstantiated) return; // Skip if the arena hasn't been set

            if (arenaon != previousArenaOnState) // Check if arenaon state has changed
            {
                ForceActivateArena();
                previousArenaOnState = arenaon; // Update previous state
            }
        }

        public void SetArena(string arenaName)
        {
            arena = GameObject.Find(arenaName + "(Clone)");
            if (arena != null)
            {
                isInstantiated = true;
                Debug.Log("Arena set: " + arenaName + "(Clone)");

                ForceActivateArena(); // Activate immediately upon setting the arena
                previousArenaOnState = arenaon; // Initialize previous state
            }
            else
            {
                Debug.LogWarning("Arena not found: " + arenaName + "(Clone)");
            }
        }

        public void StartArena()
        {
            arenaon = true;
        }

        private void ForceActivateArena()
        {
            if (arena == null) return;

            // Ensure arena and all its children are active
            arena.SetActive(arenaon);
            Transform[] children = arena.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                child.gameObject.SetActive(arenaon);
                MeshRenderer renderer = child.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.enabled = arenaon;
                    Debug.Log("Renderer " + (arenaon ? "enabled" : "disabled") + " for: " + child.name);
                }
            }
        }
    }
}