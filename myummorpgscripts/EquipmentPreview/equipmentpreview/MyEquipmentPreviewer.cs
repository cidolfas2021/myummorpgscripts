using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UMA.CharacterSystem;
using uMMORPG;
using System;
using System.Security.Cryptography;

public class MyEquipmentPreviewer : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 15f;
    [SerializeField] public Player prefab;
    [SerializeField] public Player currentPreview;
    UIEquipmentPreview previewWindow;
    [SerializeField] public NetworkManagerMMO manager;
    [SerializeField] Transform cameraPivotX;
    [SerializeField] public DynamicCharacterAvatar DCA;

    Player player => Player.localPlayer;

    ItemSlot[] previewingSlots;

    private string baseUmaDna;

    public void Update()
    {
        Player player = Player.localPlayer;
        bool equipmentOpen = UIEquipment.Instance != null && UIEquipment.Instance.panel.activeSelf;
        bool barbershopOpen = UI_BarberShop.Instance != null && UI_BarberShop.Instance.panel.activeSelf;

        bool shouldEnableCamera = equipmentOpen || barbershopOpen;

        if (player != null && player.equipment is PlayerEquipment playerEquipment && playerEquipment.avatarCamera != null)
        {
            playerEquipment.avatarCamera.enabled = shouldEnableCamera;
        }
    }

    
    private void OnEnable()
    {
        
        if (player == null)
        {
            Debug.LogWarning("Local player is not ready yet.");
            return;
        }
        if (previewWindow == null)
            previewWindow = FindFirstObjectByType<UIEquipmentPreview>();

        if (player == null)
        {
            Debug.LogWarning("Local player not found.");
            return;
        }

        // Initialize preview slots from player's current equipment
        previewingSlots = player.equipment.slots.ToArray();


        // Find prefab for player class
        prefab = manager.playerClasses.Find(p => p.name == "Glad Male");
        if (prefab == null)
        {
            Debug.LogWarning("No prefab found for class: " + "Glad Male");
            return;
        }

        // Destroy old preview if any
        if (currentPreview != null)
            Destroy(currentPreview.gameObject);

        // Instantiate preview player
        GameObject previewObj = Instantiate(prefab.gameObject, transform.position, transform.rotation, transform);
        currentPreview = previewObj.GetComponent<Player>();
        if (currentPreview == null)
        {
            Debug.LogError("Preview prefab missing Player component.");
            return;
        }

        currentPreview.name = "Previewer";
        currentPreview.equipment.slots.Clear();
        currentPreview.equipment.slots.AddRange(previewingSlots);

        DCA = currentPreview.GetComponentInChildren<DynamicCharacterAvatar>();
        if (DCA == null)
        {
            Debug.LogError("DynamicCharacterAvatar missing on preview prefab.");
            return;
        }

        // Load base DNA once from player
        var originalDCA = player.GetComponentInChildren<DynamicCharacterAvatar>();
        if (originalDCA == null)
        {
            Debug.LogError("Original player DynamicCharacterAvatar missing.");
            return;
        }
        if (originalDCA.umaData == null)
        {
            Debug.LogError("Original player UMAData is null.");
            return;
        }
        if (originalDCA.umaData.umaRecipe == null || originalDCA.umaData.umaRecipe.GetRace() == null)
        {
            Debug.LogError("Original player UMA recipe or race missing.");
            return;
        }

        try
        {
            baseUmaDna = originalDCA.GetCurrentRecipe();
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to get UMA recipe: " + ex);
            baseUmaDna = null;
        }

        if (!string.IsNullOrEmpty(baseUmaDna))
        {
            DCA.LoadFromRecipeString(baseUmaDna);
        }
        else
        {
            Debug.LogWarning("Base UMA recipe missing from player. Using empty avatar.");
        }

        // Step 2: Apply slots for preview equipment (this modifies the loaded recipe)
        RefreshPreviewEquipmentVisuals();

        // Step 3: Apply appearance from battlescripts (hair, beard, tattoo, and colors)
        CleanUpPreview();

        // Step 4: Build preview character fully
        DCA.BuildCharacter();

        // Set preview to correct layer
        SetLayerRecursively(currentPreview.transform, 8);

        // Setup equipment visuals initially
       
    }
   

    public void PreviewEquipmentItem(EquipmentItem itemData)
    {
        previewWindow.Open();
        Debug.Log("previewequipmentitem");
        if (itemData == null)
        {
            Debug.LogWarning("ItemData is null.");
            return;
        }

        if (previewingSlots == null || previewingSlots.Length != player.equipment.slots.Count)
        {
            previewingSlots = player.equipment.slots.ToArray();
        }

        int targetSlot = -1;


        if (currentPreview.equipment.slots[22].amount != 0)
        {
            foreach (Animator animator in GetComponentsInChildren<Animator>())
            {
                animator.SetBool("closeRight", true);
            }

        }
        else
        {
            foreach (Animator animator in GetComponentsInChildren<Animator>())
            {
                animator.SetBool("closeRight", false);
            }
        }
        if (currentPreview.equipment.slots[23].amount != 0)
        {
            foreach (Animator animator in GetComponentsInChildren<Animator>())
            {
                animator.SetBool("closeLeft", true);
            }
        }
        else
        {
            foreach (Animator animator in GetComponentsInChildren<Animator>())
            {
                animator.SetBool("closeLeft", false);
            }
        }

        if (UIAppearances.singleton.currentTab == AppearanceTab.Mainhand)
            targetSlot = 22; // Right hand
        else if (UIAppearances.singleton.currentTab == AppearanceTab.Offhand)
            targetSlot = 23; // Left hand
        else
        {
            for (int i = 0; i < previewingSlots.Length; i++)
            {
                EquipmentInfo info = ((PlayerEquipment)player.equipment).slotInfo[i];
                if (info.requiredCategory == itemData.category)
                {
                    targetSlot = i;
                    break;
                }
            }
        }

        if (targetSlot >= 0 && targetSlot < previewingSlots.Length)
        {
            // Replace slot with new item
            previewingSlots[targetSlot] = new ItemSlot { amount = 1, item = new Item(itemData) };
            Debug.Log($"[Preview] Assigned {itemData.name} to slot {targetSlot}");//this put on a helmet

            // Update preview player equipment slots
            currentPreview.equipment.slots.Clear();
            currentPreview.equipment.slots.AddRange(previewingSlots);

            // Refresh visuals on UMA preview
            RefreshPreviewEquipmentVisuals();


            ApplyColors();
            CleanUpPreview();
        }
        else
        {
            Debug.LogWarning($"[Preview] No valid slot found for item: {itemData.name}");
        }
        //ApplyColors();
    }

    void RefreshPreviewEquipmentVisuals()
    {
        Debug.Log("refreshpreviewequipmentvisuals");

        // Clear slots first to prevent duplicates
        //DCA.ClearSlots();

        // Set UMA slots from previewing equipment
        for (int i = 0; i < previewingSlots.Length; i++)
        {
            ItemSlot slot = previewingSlots[i];
            if (slot.amount > 0)
            {
                ScriptableItem baseItem = slot.item.dataCostume ?? slot.item.data;

                if (baseItem is EquipmentItem equipmentItem)
                {
                    if (equipmentItem.maleUmaRecipe != null)
                        DCA.SetSlot(equipmentItem.maleUmaRecipe);
                    else if (equipmentItem.femaleUmaRecipe != null)
                        DCA.SetSlot(equipmentItem.femaleUmaRecipe);
                }
                else if (baseItem is WeaponItem weaponItem)
                {
                    if (weaponItem.maleUmaRecipe != null)
                        DCA.SetSlot(weaponItem.maleUmaRecipe);
                    else if (weaponItem.femaleUmaRecipe != null)
                        DCA.SetSlot(weaponItem.femaleUmaRecipe);
                }
            }
        }



        // Refresh equipment models (e.g., weapons)
        
        if (currentPreview.equipment is PlayerEquipment pe)
        {
            for (int i = 0; i < previewingSlots.Length; i++)
            {
                pe.RefreshLocation(i);
            }
        }

        DCA.BuildCharacter();
        //ApplyColors();
    }

    void ApplyColors()
    {
        battlescripts realBS = player.GetComponent<battlescripts>();

        if (realBS == null) return;


        // Force update of UMA styles and colors
        string[] equipmentSlots = new string[]
{
    "Hair", "Beard", "Eyebrow", "Tattoo"
};

        foreach (string slotName in equipmentSlots)
        {
            DCA.ClearSlot(slotName);
        }

        if (realBS.hairIndex >= 0 && realBS.hairIndex < UI_BarberShop.Instance.maleHairStyles.Count)
            DCA.SetSlot(UI_BarberShop.Instance.maleHairStyles[realBS.hairIndex]);

        if (realBS.beardIndex >= 0 && realBS.beardIndex < UI_BarberShop.Instance.maleBeardStyles.Count)
            DCA.SetSlot(UI_BarberShop.Instance.maleBeardStyles[realBS.beardIndex]);

        if (realBS.eyebrowIndex >= 0 && realBS.eyebrowIndex < UI_BarberShop.Instance.maleEyebrowStyles.Count)
            DCA.SetSlot(UI_BarberShop.Instance.maleEyebrowStyles[realBS.eyebrowIndex]);

        if (realBS.tattooIndex >= 0 && realBS.tattooIndex < UI_BarberShop.Instance.maleTattooStyles.Count)
            DCA.SetSlot(UI_BarberShop.Instance.maleTattooStyles[realBS.tattooIndex]);
        if (!string.IsNullOrEmpty(realBS.hairColor) && ColorUtility.TryParseHtmlString(realBS.hairColor.StartsWith("#") ? realBS.hairColor : "#" + realBS.hairColor, out var hairColor))
            DCA.SetColor("Hair", hairColor);

        if (!string.IsNullOrEmpty(realBS.eyeColor) && ColorUtility.TryParseHtmlString(realBS.eyeColor.StartsWith("#") ? realBS.eyeColor : "#" + realBS.eyeColor, out var eyeColor))
            DCA.SetColor("Eyes", eyeColor);

        if (!string.IsNullOrEmpty(realBS.skinColor) && ColorUtility.TryParseHtmlString(realBS.skinColor.StartsWith("#") ? realBS.skinColor : "#" + realBS.skinColor, out var skinColor))
            DCA.SetColor("Skin", skinColor);
        DCA.BuildCharacter();
    }
    void CleanUpPreview()
    {
        Debug.Log("cleanuppreview");
        if (currentPreview == null) return;

        SetLayerRecursively(currentPreview.transform, 6);

        //realBS.LoadFromDBselect(player.name);
        //ApplyColors();


        // Rebuild hairstyles AFTER setting all slots from equipment
       
    }

    

    

    void SetLayerRecursively(Transform obj, int newLayer)
    {
        if (obj == null) return;

        int currentLayer = obj.gameObject.layer;
        if (currentLayer == 0 || currentLayer == LayerMask.NameToLayer("Player"))
        {
            obj.gameObject.layer = newLayer;
        }

        foreach (Transform child in obj)
        {
            SetLayerRecursively(child, newLayer);
        }

        Camera[] allCameras = currentPreview.GetComponentsInChildren<Camera>(true);

        foreach (Camera cam in allCameras)
        {
            if (
                cam.gameObject.layer == LayerMask.NameToLayer("EquipmentPreview"))
            {
                cam.enabled = false; // ✅ disables the camera
                //Debug.Log("Disabled Camera component on EquipmentPreviewer layer.");
            }
        }
    }

    public void RotateLeft() => RotateCharacter(-rotationSpeed);
    public void RotateRight() => RotateCharacter(rotationSpeed);

    void RotateCharacter(float angle)
    {
        if (cameraPivotX != null)
            cameraPivotX.Rotate(Vector3.up, angle);
    }

    public void ResetToCurrent()
    {
        string[] targetWardrobeSlots = new string[]
        {
        "Chest", "Legs", "Shoulders", "Gloves", "Feet", "Helmet", "Mask", "Tabard", "Shirt", "Tie", "Waist"
        };

        // Step 1: Clear only specified wardrobe slots
        foreach (string slotName in targetWardrobeSlots)
        {
            DCA.ClearSlot(slotName);
        }

        // Step 2: Load current equipment from the actual player
        if (!(player.equipment is PlayerEquipment pe)) return;

        // Copy over CURRENT equipment slots from the real player (not preview copy)
        ItemSlot[] currentSlots = player.equipment.slots.ToArray();

        for (int i = 0; i < currentSlots.Length; i++)
        {
            ItemSlot slot = currentSlots[i];
            EquipmentInfo info = pe.slotInfo[i];

            if (slot.amount > 0 && targetWardrobeSlots.Contains(info.requiredCategory))
            {
                ScriptableItem baseItem = slot.item.dataCostume ?? slot.item.data;

                if (baseItem is EquipmentItem equipmentItem)
                {
                    if (equipmentItem.maleUmaRecipe != null)
                        DCA.SetSlot(equipmentItem.maleUmaRecipe);
                    else if (equipmentItem.femaleUmaRecipe != null)
                        DCA.SetSlot(equipmentItem.femaleUmaRecipe);
                }
                else if (baseItem is WeaponItem weaponItem)
                {
                    if (weaponItem.maleUmaRecipe != null)
                        DCA.SetSlot(weaponItem.maleUmaRecipe);
                    else if (weaponItem.femaleUmaRecipe != null)
                        DCA.SetSlot(weaponItem.femaleUmaRecipe);
                }
            }
        }

        if (currentPreview.equipment is PlayerEquipment previewEquipment && player.equipment is PlayerEquipment playerEquipment)
        {
            for (int i = 0; i < previewingSlots.Length; i++)
            {
                ItemSlot playerSlot = playerEquipment.slots[i];
                ItemSlot previewSlot = previewingSlots[i];

                // If player doesn't have an item equipped here, clear the preview slot
                if (playerSlot.amount == 0)
                {
                    previewingSlots[i] = new ItemSlot(); // clear preview slot
                    previewEquipment.RefreshLocation(i); // update visuals
                }
                else
                {
                    // If player has an item equipped but preview slot differs, sync it
                    if (previewSlot.amount == 0 || !previewSlot.item.Equals(playerSlot.item))
                    {
                        previewingSlots[i] = playerSlot; // copy player's equipped item to preview
                        previewEquipment.RefreshLocation(i);
                    }
                }
            }
        }

        // Update the previewingSlots copy if needed
        previewingSlots = pe.slots.ToArray();
        


        // Step 3: Build updated character
        DCA.BuildCharacter();
        previewingSlots = player.equipment.slots.ToArray();
    }


}
