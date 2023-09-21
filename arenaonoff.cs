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

        void Awake()
        {
            // initialize singleton
            if (onoff == null) onoff = this;
        }

        void Update()
        {
            if (arena != null && arenaon)
            {
                arena.SetActive(true);
            }
            else if (arena != null && !arenaon)
            {
                arena.SetActive(false);
            }
        }

        public void SetArena(string arenaName)
        {
            arena = GameObject.Find(arenaName + "(Clone)");
            isInstantiated = true;
        }

        public void StartArena()
        {
            arenaon = true;
        }
    }
}