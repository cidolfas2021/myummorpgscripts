using UnityEngine;
using UnityEngine.UI;
namespace uMMORPG
{
    public partial class UINpcCostume : MonoBehaviour
    {
        public static UINpcCostume singleton;
        public GameObject panel;
        public UINpcCostumeSlot slotPrefab;
        public Transform content;
        public UIDragAndDropable buySlot;
        public Text buyCostsText;

        public Button repairButton;
        [HideInInspector] public int inventoryIndex = -1;
        [HideInInspector] public int costumeIndex = -1;

        private void Awake()
        {
            // assign singleton only once (to work with DontDestroyOnLoad when
            // using Zones / switching scenes)
            if (singleton == null) singleton = this;
        }
        bool AreCategoriesCompatible(string playerCategory, string playerCategory2, string costumeCategory, string costumeCategory2)
        {
            // Consider if any category matches or cross-compatible
            if (playerCategory == costumeCategory)
                return true;
            if (!string.IsNullOrEmpty(playerCategory2) && playerCategory2 == costumeCategory)
                return true;
            if (!string.IsNullOrEmpty(playerCategory) && playerCategory == costumeCategory2)
                return true;
            if (!string.IsNullOrEmpty(playerCategory2) && !string.IsNullOrEmpty(costumeCategory2) && playerCategory2 == costumeCategory2)
                return true;

            // treat main/off hand as interchangeable
            if ((playerCategory == "OffHand" && costumeCategory == "MainHand") || (playerCategory == "MainHand" && costumeCategory == "OffHand"))
                return true;

            return false;
        }
        void Update()
        {
            Player player = Player.localPlayer;

            // use collider point(s) to also work with big entities
            if (player != null &&
                player.target != null &&
                player.target is Npc npc &&
                Utils.ClosestDistance(player, player.target) <= player.interactionRange)
            {

                // appearance list
                UIUtils.BalancePrefabs(slotPrefab.gameObject, player.npcCostume.sortedCostume.Count, content);
                for (int i = 0; i < player.npcCostume.sortedCostume.Count; ++i)
                {
                    UINpcCostumeSlot slot = content.GetChild(i).GetComponent<UINpcCostumeSlot>();
                    int costumeHash = player.npcCostume.sortedCostume[i];
                    ScriptableItem itemData = ScriptableItem.All[costumeHash];

                    slot.gameObject.SetActive(true);

                    if (inventoryIndex != -1)
                    {
                        // filtering by category
                        ItemSlot invSlot = player.inventory.slots[inventoryIndex];
                        if (invSlot.item.data is EquipmentItem eq && itemData is EquipmentItem costumeEq &&
    !AreCategoriesCompatible(eq.category, eq.category2, costumeEq.category, costumeEq.category2))
                        {
                            slot.gameObject.SetActive(false);
                            continue;
                        }
                    }

                    // show item in UI
                    int icopy = i;
                    slot.button.interactable = inventoryIndex != -1;
                    slot.button.onClick.SetListener(() =>
                    {
                        costumeIndex = icopy;
                    });
                    slot.background.color = Color.white;
                    if (inventoryIndex != -1 && player.inventory.slots[inventoryIndex].item.costumeHash == costumeHash)
                    {
                        slot.background.color = Color.green;
                        slot.button.interactable = false;
                    }
                    else if (costumeIndex == i)
                        slot.background.color = Color.yellow;

                    slot.image.sprite = itemData.image;
                    // only build tooltip while it's actually shown. this
                    // avoids MASSIVE amounts of StringBuilder allocations.
                    slot.tooltip.enabled = true;
                    if (slot.tooltip.IsVisible())
                        slot.tooltip.text = new ItemSlot(new Item(itemData)).ToolTip(); // with slot for {AMOUNT}
                }

                // buy
                if (inventoryIndex != -1 && inventoryIndex < player.inventory.slots.Count)
                {
                    Item playerItem = player.inventory.slots[inventoryIndex].item;
                    ScriptableItem itemData = playerItem.data;


                    // make valid amount, calculate price
                    long price = itemData.buyPrice;

                    // show buy panel with item in UI
                    buySlot.GetComponent<Image>().color = Color.white;
                    buySlot.GetComponent<Image>().sprite = itemData.image;
                    // only build tooltip while it's actually shown. this
                    // avoids MASSIVE amounts of StringBuilder allocations.
                    buySlot.GetComponent<UIShowToolTip>().enabled = true;
                    if (buySlot.GetComponent<UIShowToolTip>().IsVisible())
                        buySlot.GetComponent<UIShowToolTip>().text = new ItemSlot(new Item(itemData)).ToolTip(); // with slot for {AMOUNT}
                    buySlot.dragable = true;

                    repairButton.interactable = false;

                    repairButton.onClick.SetListener(() =>
                    {
                        player.npcCostume.CmdCustomizeEquip(inventoryIndex, costumeIndex);
                        costumeIndex = -1;
                    });

                    if (costumeIndex != -1)
                    {
                        int selectedCostumeHash = player.npcCostume.sortedCostume[costumeIndex];
                        price += ScriptableItem.All[selectedCostumeHash].buyPrice;

                        repairButton.interactable = playerItem.costumeHash != selectedCostumeHash && price <= player.gold;

                    }

                    buyCostsText.text = price.ToString();
                }
                else
                {
                    // show default buy panel in UI
                    buySlot.GetComponent<Image>().color = Color.clear;
                    buySlot.GetComponent<Image>().sprite = null;
                    buySlot.GetComponent<UIShowToolTip>().enabled = false;
                    buySlot.dragable = false;
                    buyCostsText.text = "0";
                    repairButton.interactable = false;
                }

            }
            else panel.SetActive(false);
        }
    }
}
