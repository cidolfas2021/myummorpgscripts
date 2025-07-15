using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UMA;
using UMA.CharacterSystem;
using Mirror;
using GameZero;
using System.Collections.Generic;
using System.Security.Cryptography;
using System;

namespace uMMORPG
{
    public class UI_BarberShop : MonoBehaviour
    {
        public static UI_BarberShop Instance { get; private set; }

        [Header("UMA Races")]
        public string dcaRaceMale = "Glad Male"; // Add this line


        [SerializeField] public DynamicCharacterAvatar DCA;
        [Header("Tattoo Styles")]
        
        public KeyCode hotKey = KeyCode.U;
        public List<UMATextRecipe> maleHairStyles;
        public List<UMATextRecipe> maleBeardStyles;
        public List<UMATextRecipe> maleEyebrowStyles;
        public List<UMATextRecipe> maleTattooStyles;
        [Header("UI References")]
        public GameObject panel;
        public Button closeButton;
        public Button saveButton;
        public Button resetButton;
        public Button nextHairButton;
        public Button nextBeardButton;
        public Button nextEyebrowButton;
        public Button nextTattooButton;
        public TMP_Text hairIndexText;
        public TMP_Text beardIndexText;
        public TMP_Text eyebrowIndexText;
        public TMP_Text tattooIndexText;
        public battlescripts bs;
        public Player player;
        public RenderTexture renderTexture;
        public RawImage image;
        //blic int hairIndex, beardIndex, eyebrowIndex, tattooIndex;
        // Flag to gate rendering until UMA build is done
        private bool canRenderAvatar = false;

        void Start()
        {

            panel.SetActive(false);

            closeButton.onClick.AddListener(() => panel.SetActive(false));
            saveButton.onClick.AddListener(SaveAppearance);
            resetButton.onClick.AddListener(Resetbarber);
            nextHairButton.onClick.AddListener(() => ChangeStyle("Hair"));
            nextBeardButton.onClick.AddListener(() => ChangeStyle("Beard"));
            nextEyebrowButton.onClick.AddListener(() => ChangeStyle("Eyebrow"));
            nextTattooButton.onClick.AddListener(() => ChangeStyle("Tattoo"));
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple instances of UI_BarberShop found. Destroying duplicate.");
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
        }

        void Update()
        {
            // Update player reference if needed
            if (player == null || player != Player.localPlayer)
            {
                player = Player.localPlayer;
                if (player != null)
                {

                    DCA = player.GetComponentInChildren<DynamicCharacterAvatar>();
                    bs = player.GetComponentInChildren<battlescripts>();
                    
                }
            }

            if (player == null)
                return;

            // Toggle panel with hotkey if no input field is active
            if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
                panel.SetActive(!panel.activeSelf);

            if (!panel.activeSelf)
                return;

            

            // Render avatar camera only if UMA build is complete
            if (canRenderAvatar)
            {
                if (player.equipment is PlayerEquipment pe && pe.avatarCamera != null)
                {
                    if (!pe.avatarCamera.gameObject.activeSelf)
                        pe.avatarCamera.gameObject.SetActive(true);
                    if (!pe.avatarCamera.enabled)
                        pe.avatarCamera.enabled = true;

                    pe.avatarCamera.Render();
                }
            }
        }

      
        /*
        void OnCharacterUpdated(UMAData umaData)
        {
            // UMA rebuild complete
            // Disable rendering until next frame to avoid SkinnedMeshRenderer error
            canRenderAvatar = false;
            StartCoroutine(EnableRenderNextFrame());
        }

        IEnumerator EnableRenderNextFrame()
        {
            yield return null; // Wait one frame
            canRenderAvatar = true;
            Debug.Log("UMA character fully updated, avatar rendering enabled.");
        }*/

        public void ChangeColor(UmaColorTypes type, Color newColor)
        {
            if (DCA == null)
                return;

            string colorHex = ColorUtility.ToHtmlStringRGBA(newColor);

            switch (type)
            {
                case UmaColorTypes.Skin:
                    bs.skinColor = colorHex;
                    break;
                case UmaColorTypes.Hair:
                    bs.hairColor = colorHex;
                    break;
                case UmaColorTypes.Eyes:
                    bs.eyeColor = colorHex;
                    break;
            }

            UpdateUMAColors();  // <-- Add this line to update the avatar visually
        }

        

        void ChangeStyle(string type)
        {
            if (player == null || bs == null)
                return;

            switch (type)
            {
                case "Hair":
                    bs.hairIndex = (bs.hairIndex + 1) % maleHairStyles.Count;
                    break;
                case "Beard":
                    bs.beardIndex = (bs.beardIndex + 1) % maleBeardStyles.Count;
                    break;
                case "Eyebrow":
                    bs.eyebrowIndex = (bs.eyebrowIndex + 1) % maleEyebrowStyles.Count;
                    break;
                case "Tattoo":
                    bs.tattooIndex = (bs.tattooIndex + 1) % maleTattooStyles.Count;
                    break;
            }

            hairIndexText.text = (bs.hairIndex + 1).ToString();
            beardIndexText.text = (bs.beardIndex + 1).ToString();
            eyebrowIndexText.text = (bs.eyebrowIndex + 1).ToString();
            tattooIndexText.text = (bs.tattooIndex + 1).ToString();

            UpdateUMAStyles();
        }

        public void UpdateUMAStyles()
        {

            string[] equipmentSlots = new string[]
{
    "Hair", "Beard", "Eyebrow", "Tattoo"
};

            foreach (string slotName in equipmentSlots)
            {
                DCA.ClearSlot(slotName);
            }

            if (bs.hairIndex >= 0 && bs.hairIndex < maleHairStyles.Count)
                DCA.SetSlot(maleHairStyles[bs.hairIndex]);

            if (bs.beardIndex >= 0 && bs.beardIndex < maleBeardStyles.Count)
                DCA.SetSlot(maleBeardStyles[bs.beardIndex]);

            if (bs.eyebrowIndex >= 0 && bs.eyebrowIndex < maleEyebrowStyles.Count)
                DCA.SetSlot(maleEyebrowStyles[bs.eyebrowIndex]);

            if (bs.tattooIndex >= 0 && bs.tattooIndex < maleTattooStyles.Count)
                DCA.SetSlot(maleTattooStyles[bs.tattooIndex]);

            DCA.BuildCharacter();
        }

        public void UpdateUMAColors()
        {
            if (DCA == null || bs == null) return;




            if (!string.IsNullOrEmpty(bs.hairColor) && ColorUtility.TryParseHtmlString(bs.hairColor.StartsWith("#") ? bs.hairColor : "#" + bs.hairColor, out var hairColor))
                DCA.SetColor("Hair", hairColor);

            if (!string.IsNullOrEmpty(bs.eyeColor) && ColorUtility.TryParseHtmlString(bs.eyeColor.StartsWith("#") ? bs.eyeColor : "#" + bs.eyeColor, out var eyeColor))
                DCA.SetColor("Eyes", eyeColor);

            if (!string.IsNullOrEmpty(bs.skinColor) && ColorUtility.TryParseHtmlString(bs.skinColor.StartsWith("#") ? bs.skinColor : "#" + bs.skinColor, out var skinColor))
                DCA.SetColor("Skin", skinColor);

            DCA.UpdateColors(true);
            //DCA.BuildCharacter(true);
        }
        public void Resetbarber()
        {
            string[] equipmentSlots = new string[]
{
    "Hair", "Beard", "Eyebrow", "Tattoo"
};

            foreach (string slotName in equipmentSlots)
            {
                DCA.ClearSlot(slotName);
                DCA.BuildCharacter();
            }

        }
        public void SaveAppearance()
        {
            Debug.Log("SaveAppearance() called");
            StartCoroutine(SaveAppearanceCoroutine());
        }

        public IEnumerator SaveAppearanceCoroutine()
        {
            if (DCA == null) yield break;

            //DCA.ClearSlots(); // ✅ Clear first

            

            //DCA.UpdateColors(true);
             // ✅ Build last

            yield return null;

            string compressedAvatarDef;
            try
            {
                var avatarDef = DCA.GetAvatarDefinition(false, false);
                string avatarJson = JsonUtility.ToJson(avatarDef);
                compressedAvatarDef = CompressionUtils.CompressToBase64(avatarJson);
            }
            catch (Exception e)
            {
                Debug.LogError("Error compressing AvatarDefinition: " + e);
                yield break;
            }

            if (bs.isLocalPlayer)
            {
                bs.CmdSaveUMAAppearanceToServer(
                    compressedAvatarDef,
                    bs.hairIndex,
                    bs.beardIndex,
                    bs.eyebrowIndex,
                    bs.tattooIndex,
                    bs.hairColor,
                    bs.eyeColor,
                    bs.skinColor
                );
            }

            //bs.RefreshUMAEquipmentVisuals();
            DCA.BuildCharacter(true);
        }
    }
}
