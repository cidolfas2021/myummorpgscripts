using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
namespace uMMORPG
{
    [RequireComponent(typeof(PlayerInventory))]
    [DisallowMultipleComponent]
    public class PlayerNpcCostume : NetworkBehaviour
    {
        [Header("Components")]
        public Player player;
        public PlayerInventory inventory;
        [SyncVar] public int costumeHash;
        public SyncHashSet<int> unlockedCostume = new SyncHashSet<int>();
        public List<int> sortedCostume;

        public override void OnStartServer()
        {
            foreach (ItemSlot its in inventory.slots)
                if (its.amount > 0)
                    AddCostumeHash(its.item.name.GetStableHashCode());

            foreach (ItemSlot its in player.equipment.slots)
                if (its.amount > 0)
                    AddCostumeHash(its.item.name.GetStableHashCode());

            inventory.slots.Callback += (q, w, e, r) =>
            {
                if (r.amount > 0)
                {
                    int hashCode = r.item.data.name.GetStableHashCode();
                    AddCostumeHash(hashCode);
                }
            };

            // TEST ONLY
            //foreach (ScriptableItem item in ScriptableItem.All.Values)
            //    if (item is EquipmentItem)
            //        unlockedCostume.Add(item.name.GetStableHashCode());
        }

        void AddCostumeHash(int h)
        {
            ScriptableItem itemData = ScriptableItem.All[h];
            if (!(itemData is EquipmentItem))
                return;

            if (!unlockedCostume.Contains(h))
                unlockedCostume.Add(h);

        }

        private void Update()
        {
            //if(Input.GetKeyDown(KeyCode.L))
            //{
            //    ScriptableItem[] randomItem = ScriptableItem.All.Values.Where(x=>x is EquipmentItem).ToArray();
            //    ScriptableItem random = randomItem[Random.Range(0, randomItem.Length)];
            //    if (!unlockedCostume.Contains(random.name.GetStableHashCode()))
            //        unlockedCostume.Add(random.name.GetStableHashCode());
            //}
        }

        void Start()
        {
            unlockedCostume.Callback += UnlockedCallback;
            UnlockedCallback(SyncSet<int>.Operation.OP_ADD, 1);
        }

        void UnlockedCallback(SyncHashSet<int>.Operation op, int q)
        {
            sortedCostume = unlockedCostume.ToList();
        }

        // trading /////////////////////////////////////////////////////////////////
        [Command]
        public void CmdCustomizeEquip(int index, int costumeIndex)
        {
            Debug.Log("🧵 Saving costume equipment & inventory after customization...");
            // validate: close enough, npc alive and valid index?
            // use collider point(s) to also work with big entities
            costumeHash = sortedCostume[costumeIndex];

            if (player.state == "IDLE" &&
                player.target != null &&
                player.target.health.current > 0 &&
                player.target is Npc npc &&
                npc.costume != null && // only if Npc offers trading
                Utils.ClosestDistance(player, npc) <= player.interactionRange &&
                0 <= index && index < player.inventory.slots.Count &&
                unlockedCostume.Contains(costumeHash))
            {
                ScriptableItem costumeItem = ScriptableItem.All[costumeHash];

                ItemSlot slot = player.inventory.slots[index];

                // Update both costumeHash and costumeCategoryOverride
                slot.item.costumeHash = costumeHash;
                slot.item.costumeCategoryOverride = costumeItem is EquipmentItem equipCostume
                    ? equipCostume.category
                    : "";

                player.inventory.slots[index] = slot;

                long cost = costumeItem.buyPrice + slot.item.data.buyPrice;

                player.gold -= cost;

                Debug.Log(costumeHash + " : " + player.inventory.slots[index].item.costumeHash + " : " + cost);

                Database.singleton.SaveCostumeInventory(player.inventory);
                Database.singleton.SaveCostumeEquipment((PlayerEquipment)player.equipment);

            }
        }
        // drag & drop /////////////////////////////////////////////////////////////
        void OnDragAndDrop_InventorySlot_NpcCostumeSlot(int[] slotIndices)
        {
            // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
            ItemSlot slot = inventory.slots[slotIndices[0]];

            if (slot.item.data is EquipmentItem eq)
            {
                UINpcCostume.singleton.inventoryIndex = slotIndices[0];
                UINpcCostume.singleton.costumeIndex = -1;

            }
        }

        void OnDragAndClear_NpcCostumeSlot(int slotIndex)
        {
            UINpcCostume.singleton.inventoryIndex = -1;
        }
    }
}
