using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using uMMORPG;

public enum AppearanceTab
{
    Helmets,
    Masks,
    Shoulders,
    Chests,
    Capes,
    Legs,
    Boots,
    Gloves,
    Belts,
    Mainhand,
    Offhand
}

public partial class UIAppearances : MonoBehaviour
{
    [Header("Tab System")]
    public AppearanceTab currentTab = AppearanceTab.Helmets;
    public static string LastClickedItemName = string.Empty;
    public static UIAppearances singleton;

    public Button helmetTabButton;
    public Button maskTabButton;
    public Button shoulderTabButton;
    public Button chestTabButton;
    public Button capeTabButton;
    public Button legTabButton;
    public Button bootTabButton;
    public Button gloveTabButton;
    public Button beltTabButton;
    public Dropdown mainhandDropdown;
    public Dropdown offhandDropdown;

    private string selectedMainhandType = "All";
    private string selectedOffhandType = "All";

    public KeyCode hotKey = KeyCode.I;
    public GameObject panel;


    public UIInventorySlot slotPrefab;


    public Transform content;
    public Text goldText;
    public UIDragAndDropable trash;
    public Image trashImage;
    public GameObject trashOverlay;
    public Text trashAmountText;

    [Header("Durability Colors")]
    public Color brokenDurabilityColor = Color.red;
    public Color lowDurabilityColor = Color.magenta;
    [Range(0.01f, 0.99f)] public float lowDurabilityThreshold = 0.1f;

    void Awake()
    {
        if (singleton == null) singleton = this;
        else Destroy(gameObject);
    }

   
    
    void Start()
    {
        helmetTabButton.onClick.AddListener(() => SwitchTab(AppearanceTab.Helmets));
        maskTabButton.onClick.AddListener(() => SwitchTab(AppearanceTab.Masks));
        shoulderTabButton.onClick.AddListener(() => SwitchTab(AppearanceTab.Shoulders));
        chestTabButton.onClick.AddListener(() => SwitchTab(AppearanceTab.Chests));
        capeTabButton.onClick.AddListener(() => SwitchTab(AppearanceTab.Capes));
        legTabButton.onClick.AddListener(() => SwitchTab(AppearanceTab.Legs));
        bootTabButton.onClick.AddListener(() => SwitchTab(AppearanceTab.Boots));
        gloveTabButton.onClick.AddListener(() => SwitchTab(AppearanceTab.Gloves));
        beltTabButton.onClick.AddListener(() => SwitchTab(AppearanceTab.Belts));

        if (mainhandDropdown != null)
        {
            mainhandDropdown.ClearOptions();
            var mainhandOptions = new List<Dropdown.OptionData>
            {
                new Dropdown.OptionData("All"),
                new Dropdown.OptionData("1hSword"),
                new Dropdown.OptionData("1hAxe"),
                new Dropdown.OptionData("1hMace"),
                new Dropdown.OptionData("2hSword"),
                new Dropdown.OptionData("2hAxe"),
                new Dropdown.OptionData("2hMace"),
                new Dropdown.OptionData("Staff"),
                new Dropdown.OptionData("Wand"),
                new Dropdown.OptionData("Ranged"),
                new Dropdown.OptionData("Dagger"),
                new Dropdown.OptionData("Fist")
                
            };
            mainhandDropdown.AddOptions(mainhandOptions);
            mainhandDropdown.onValueChanged.AddListener(index =>
            {
                selectedMainhandType = mainhandDropdown.options[index].text;
                currentTab = AppearanceTab.Mainhand;
                RefreshCurrentTab();
            });
            mainhandDropdown.value = 0;
            selectedMainhandType = "All";
        }

        if (offhandDropdown != null)
        {
            offhandDropdown.ClearOptions();
            var offhandOptions = new List<Dropdown.OptionData>
            {
                new Dropdown.OptionData("All"),
                new Dropdown.OptionData("1hSword"),
                new Dropdown.OptionData("1hAxe"),
                new Dropdown.OptionData("1hMace"),
                new Dropdown.OptionData("2hSword"),
                new Dropdown.OptionData("2hAxe"),
                new Dropdown.OptionData("2hMace"),
                new Dropdown.OptionData("Wand"),
                new Dropdown.OptionData("Ranged"),
                new Dropdown.OptionData("Dagger"),
                new Dropdown.OptionData("Fist"),
                new Dropdown.OptionData("Shield"),
                 new Dropdown.OptionData("Tome")

            };
            offhandDropdown.AddOptions(offhandOptions);
            offhandDropdown.onValueChanged.AddListener(index =>
            {
                selectedOffhandType = offhandDropdown.options[index].text;
                currentTab = AppearanceTab.Offhand;
                RefreshCurrentTab();
            });
            offhandDropdown.value = 0;
            selectedOffhandType = "All";
        }

        // Always keep both dropdowns visible
        if (mainhandDropdown != null) mainhandDropdown.gameObject.SetActive(true);
        if (offhandDropdown != null) offhandDropdown.gameObject.SetActive(true);
    }

    void SwitchTab(AppearanceTab tab)
    {
        currentTab = tab;
        RefreshCurrentTab();
    }

    // Returns slots from source where the slot's item data matches any item in arr
    List<(ItemSlot slot, int realIndex)> GetSlotsFrom(ScriptableItemAndAmount[] arr, List<ItemSlot> source)
    {
        List<(ItemSlot, int)> result = new();
        if (arr == null) return result;

        // Create a HashSet of ScriptableItems for fast lookup
        HashSet<ScriptableItem> allowedItems = new(arr.Select(x => x.item));

        for (int i = 0; i < source.Count; ++i)
        {
            ItemSlot s = source[i];
            if (s.amount > 0 && s.item.data != null && allowedItems.Contains(s.item.data))
            {
                result.Add((s, i));
            }
        }
        return result;
    }

    List<(ItemSlot slot, int realIndex)> GetSlotsForCurrentTab(Player player)
    {
        if (Appearancedatabase.Instance == null)
            return new();

        var allSlots = Appearancedatabase.Instance.slots.ToList();

        List<(ItemSlot, int)> result = new();

        switch (currentTab)
        {
            case AppearanceTab.Helmets:
                return GetSlotsFrom(Appearancedatabase.Instance.helmetAppearances, allSlots);
            case AppearanceTab.Masks:
                return GetSlotsFrom(Appearancedatabase.Instance.maskAppearances, allSlots);
            case AppearanceTab.Shoulders:
                return GetSlotsFrom(Appearancedatabase.Instance.shoulderAppearances, allSlots);
            case AppearanceTab.Chests:
                return GetSlotsFrom(Appearancedatabase.Instance.chestAppearances, allSlots);
            case AppearanceTab.Capes:
                return GetSlotsFrom(Appearancedatabase.Instance.capesAppearances, allSlots);
            case AppearanceTab.Legs:
                return GetSlotsFrom(Appearancedatabase.Instance.legsAppearances, allSlots);
            case AppearanceTab.Boots:
                return GetSlotsFrom(Appearancedatabase.Instance.bootsAppearances, allSlots);
            case AppearanceTab.Gloves:
                return GetSlotsFrom(Appearancedatabase.Instance.glovesAppearances, allSlots);
            case AppearanceTab.Belts:
                return GetSlotsFrom(Appearancedatabase.Instance.beltsAppearances, allSlots);

            case AppearanceTab.Mainhand:
                {
                    HashSet<ScriptableItem> seen = new();
                    foreach (var group in Appearancedatabase.Instance.mainhandGroups ?? System.Array.Empty<WeaponAppearanceGroup>())
                    {
                        if (selectedMainhandType == "All" || group.type.Equals(selectedMainhandType, System.StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (var (slot, index) in GetSlotsFrom(group.appearances, allSlots))
                            {
                                if (seen.Add(slot.item.data))
                                    result.Add((slot, index));
                            }
                        }
                    }
                    return result;
                }
            case AppearanceTab.Offhand:
                {
                    HashSet<ScriptableItem> seen = new();
                    foreach (var group in Appearancedatabase.Instance.offhandGroups ?? System.Array.Empty<WeaponAppearanceGroup>())
                    {
                        if (selectedOffhandType == "All" || group.type.Equals(selectedOffhandType, System.StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (var (slot, index) in GetSlotsFrom(group.appearances, allSlots))
                            {
                                if (seen.Add(slot.item.data))
                                    result.Add((slot, index));
                            }
                        }
                    }
                    return result;
                }

            default:
                return new();
        }
    }
    void Update()
    {
        Player player = Player.localPlayer;
        if (player != null)
        {
            if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
            {
                panel.SetActive(!panel.activeSelf);
                if (panel.activeSelf) RefreshCurrentTab();
            }
        }
        else panel.SetActive(false);
    }

    public void RefreshCurrentTab()
    {
        Player player = Player.localPlayer;
        if (!panel.activeSelf || player == null) return;

        List<(ItemSlot slot, int realIndex)> currentSlots = GetSlotsForCurrentTab(player);
        UIUtils.BalancePrefabs(slotPrefab.gameObject, currentSlots.Count, content);

        for (int i = 0; i < currentSlots.Count; ++i)
        {

            UIInventorySlot slot = content.GetChild(i).GetComponent<UIInventorySlot>();

            var (itemSlot, realIndex) = currentSlots[i];
            slot.dragAndDropable.name = i.ToString();

            // Cache local variables for closure capture inside listeners
            ItemSlot capturedSlot = itemSlot;
            int capturedIndex = realIndex;

            if (capturedSlot.amount > 0)
            {
                slot.button.onClick.RemoveAllListeners(); // remove previous listeners first
                slot.button.onClick.AddListener(() =>
                {
                    LastClickedItemName = capturedSlot.item.name;

                    if (Input.GetKey(KeyCode.LeftControl))
                    {
                        if (capturedIndex >= 0 && capturedIndex < Appearancedatabase.Instance.slots.Count)
                        {
                            ItemSlot slotInner = Appearancedatabase.Instance.slots[capturedIndex];
                            if (slotInner.amount > 0 && slotInner.item.data is EquipmentItem equipmentItem)
                            {
                                Appearancedatabase.Instance.CtrlClickPreview(equipmentItem);
                            }
                            else
                            {
                                Debug.LogWarning("[UIAppearances] Slot is empty or not an EquipmentItem");
                            }
                        }
                        else
                        {
                            // Optionally log warning about out-of-bounds index here
                        }
                    }
                });

                slot.tooltip.enabled = true;
                slot.tooltip.GetComponent<UIShowToolTip>().text = capturedSlot.ToolTip();

                slot.dragAndDropable.dragable = true;


                if (capturedSlot.item.maxDurability > 0)
                {
                    if (capturedSlot.item.durability == 0)
                        slot.image.color = brokenDurabilityColor;
                    else if (capturedSlot.item.DurabilityPercent() < lowDurabilityThreshold)
                        slot.image.color = lowDurabilityColor;
                    else
                        slot.image.color = Color.white;
                }
                else

                    slot.image.color = Color.white;

                slot.image.sprite = capturedSlot.item.image;

                if (capturedSlot.item.data is UsableItem usable2)
                {
                    float cooldown = player.GetItemCooldown(usable2.cooldownCategory);
                    slot.cooldownCircle.fillAmount = usable2.cooldown > 0 ? cooldown / usable2.cooldown : 0;
                }
                else slot.cooldownCircle.fillAmount = 0;

                slot.amountOverlay.SetActive(capturedSlot.amount > 1);
                slot.amountText.text = capturedSlot.amount.ToString();


                

            }
            else
            {
                slot.button.onClick.RemoveAllListeners();
                slot.tooltip.enabled = false;
                slot.dragAndDropable.dragable = false;
                slot.image.color = Color.clear;
                slot.image.sprite = null;
                slot.cooldownCircle.fillAmount = 0;
                slot.amountOverlay.SetActive(false);

            }
        }

       
    }
}
