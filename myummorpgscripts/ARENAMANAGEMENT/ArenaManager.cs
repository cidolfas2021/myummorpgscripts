using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Collections.Generic;
using GameZero;

namespace uMMORPG
{
    [DisallowMultipleComponent]
    public class ArenaManager : NetworkBehaviour
    {
        [Header("Instances")]
        [SerializeField] private List<Instance> dungeonInstances = new List<Instance>();
        [SerializeField] private List<Instance> arenaInstances = new List<Instance>();

        [Header("UI")]
        [SerializeField] private Dropdown dungeonDropdown;
        [SerializeField] private Dropdown arenaDropdown;
        [SerializeField] private List<RawImage> dungeonPreviews = new List<RawImage>();
        [SerializeField] private List<RawImage> arenaPreviews = new List<RawImage>();

        [SyncVar] public int instanceId = -1;
        public Instance selectedArena;

        public static ArenaManager singleton;

        private void Awake()
        {
            if (singleton == null) singleton = this;
        }

        private void Start()
        {
            HideAllPreviews();
            PopulateDropdowns();
        }

        private void PopulateDropdowns()
        {
            dungeonDropdown.ClearOptions();
            arenaDropdown.ClearOptions();

            List<string> dungeonOptions = dungeonInstances.ConvertAll(i => i.name);
            List<string> arenaOptions = arenaInstances.ConvertAll(i => i.name);

            dungeonDropdown.AddOptions(dungeonOptions);
            arenaDropdown.AddOptions(arenaOptions);

            dungeonDropdown.onValueChanged.AddListener(OnDungeonDropdownChanged);
            arenaDropdown.onValueChanged.AddListener(OnArenaDropdownChanged);
        }

        private void OnDungeonDropdownChanged(int index)
        {
            if (index >= 0 && index < dungeonInstances.Count)
            {
                Instance selected = dungeonInstances[index];
                int id = GetInstanceId(selected);
                CmdSetSelectedArena(id);
            }
        }

        private void OnArenaDropdownChanged(int index)
        {
            if (index >= 0 && index < arenaInstances.Count)
            {
                Instance selected = arenaInstances[index];
                int id = GetInstanceId(selected);
                CmdSetSelectedArena(id);
            }
        }

        private int GetInstanceId(Instance instance)
        {
            InstanceId comp = instance.GetComponent<InstanceId>();
            return comp != null ? comp.instanceId : -1;
        }

        [Command(requiresAuthority = false)]
        public void CmdSetSelectedArena(int instanceId)
        {
            Instance arena = GetInstanceTemplate(instanceId);
            if (arena != null)
            {
                selectedArena = arena;
                this.instanceId = instanceId;
                RpcUpdatePreviews(instanceId);
            }
        }

        [ClientRpc]
        private void RpcUpdatePreviews(int instanceId)
        {
            HideAllPreviews();

            Instance instance = GetInstanceTemplate(instanceId);
            if (instance != null)
            {
                if (dungeonInstances.Contains(instance))
                {
                    int index = dungeonInstances.IndexOf(instance);
                    if (index >= 0 && index < dungeonPreviews.Count)
                        dungeonPreviews[index].gameObject.SetActive(true);
                }
                else if (arenaInstances.Contains(instance))
                {
                    int index = arenaInstances.IndexOf(instance);
                    if (index >= 0 && index < arenaPreviews.Count)
                        arenaPreviews[index].gameObject.SetActive(true);
                }
            }
        }

        private void HideAllPreviews()
        {
            foreach (var preview in dungeonPreviews)
                preview.gameObject.SetActive(false);

            foreach (var preview in arenaPreviews)
                preview.gameObject.SetActive(false);
        }

        public Instance GetInstanceTemplate(int instanceId)
        {
            foreach (var instance in dungeonInstances)
            {
                InstanceId comp = instance.GetComponent<InstanceId>();
                if (comp != null && comp.instanceId == instanceId)
                    return instance;
            }

            foreach (var instance in arenaInstances)
            {
                InstanceId comp = instance.GetComponent<InstanceId>();
                if (comp != null && comp.instanceId == instanceId)
                    return instance;
            }

            return null;
        }
    }
}
