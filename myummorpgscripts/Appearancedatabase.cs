using Mirror;
using System.Collections.Generic;
using uMMORPG;
using UnityEngine;

namespace uMMORPG
{
    [System.Serializable]
    public class WeaponAppearanceGroup
    {
        public string type; // e.g., "Sword", "Axe", etc.
        public ScriptableItemAndAmount[] appearances;
    }

    public partial class Appearancedatabase : Appearances
    {
        public ScriptableItemAndAmount[] helmetAppearances;
        public ScriptableItemAndAmount[] maskAppearances;
        public ScriptableItemAndAmount[] shoulderAppearances;
        public ScriptableItemAndAmount[] chestAppearances;
        public ScriptableItemAndAmount[] capesAppearances;
        public ScriptableItemAndAmount[] beltsAppearances;
        public ScriptableItemAndAmount[] legsAppearances;
        public ScriptableItemAndAmount[] bootsAppearances;
        public ScriptableItemAndAmount[] glovesAppearances;

        public WeaponAppearanceGroup[] mainhandGroups;
        public WeaponAppearanceGroup[] offhandGroups;

        public static Appearancedatabase Instance;

        void Awake()
        {
            Instance = this;

            helmetAppearances ??= new ScriptableItemAndAmount[0];
            maskAppearances ??= new ScriptableItemAndAmount[0];
            shoulderAppearances ??= new ScriptableItemAndAmount[0];
            chestAppearances ??= new ScriptableItemAndAmount[0];
            capesAppearances ??= new ScriptableItemAndAmount[0];
            beltsAppearances ??= new ScriptableItemAndAmount[0];
            legsAppearances ??= new ScriptableItemAndAmount[0];
            bootsAppearances ??= new ScriptableItemAndAmount[0];
            glovesAppearances ??= new ScriptableItemAndAmount[0];
            mainhandGroups ??= new WeaponAppearanceGroup[0];
            offhandGroups ??= new WeaponAppearanceGroup[0];

            // Fix amounts early so InitializeDefaultAppearances uses valid data
            EnsureAmounts(helmetAppearances);
            EnsureAmounts(maskAppearances);
            EnsureAmounts(shoulderAppearances);
            EnsureAmounts(chestAppearances);
            EnsureAmounts(capesAppearances);
            EnsureAmounts(beltsAppearances);
            EnsureAmounts(legsAppearances);
            EnsureAmounts(bootsAppearances);
            EnsureAmounts(glovesAppearances);

            foreach (var group in mainhandGroups)
                EnsureAmounts(group.appearances);

            foreach (var group in offhandGroups)
                EnsureAmounts(group.appearances);
        }

        // Make sure all items have at least amount = 1 if they have an item
        void EnsureAmounts(ScriptableItemAndAmount[] list)
        {
            if (list == null) return;
            for (int i = 0; i < list.Length; ++i)
            {
                if (list[i].item != null && list[i].amount == 0)
                    list[i].amount = 1;
            }
        }

        public void InitializeDefaultAppearances()
        {
            slots.Clear();

            List<ScriptableItemAndAmount> allAppearances = new List<ScriptableItemAndAmount>();

            allAppearances.AddRange(helmetAppearances);
            allAppearances.AddRange(maskAppearances);
            allAppearances.AddRange(shoulderAppearances);
            allAppearances.AddRange(chestAppearances);
            allAppearances.AddRange(capesAppearances);
            allAppearances.AddRange(beltsAppearances);
            allAppearances.AddRange(legsAppearances);
            allAppearances.AddRange(bootsAppearances);
            allAppearances.AddRange(glovesAppearances);

            foreach (var group in mainhandGroups)
                if (group?.appearances != null)
                    allAppearances.AddRange(group.appearances);

            foreach (var group in offhandGroups)
                if (group?.appearances != null)
                    allAppearances.AddRange(group.appearances);

            for (int i = 0; i < allAppearances.Count; ++i)
            {
                var appearance = allAppearances[i];
                if (appearance.item != null && appearance.amount > 0)
                    slots.Add(new ItemSlot(new Item(appearance.item), appearance.amount));
                else
                    slots.Add(new ItemSlot()); // add empty slot if needed for indexing consistency
            }

            Debug.Log($"Initialized slots with {slots.Count} items");
        }

        public override void OnStartServer()
        {
            // Initialize slots with all appearances (amounts already ensured)
            InitializeDefaultAppearances();
            
            // Make sure syncing mode is observers
            if (syncMode != SyncMode.Observers)
                syncMode = SyncMode.Observers;
        }
    }
}
