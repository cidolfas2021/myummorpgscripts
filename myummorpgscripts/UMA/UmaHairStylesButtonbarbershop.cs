using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UMA;
using System.Collections;
using UnityEngine.Events;
using UMA.CharacterSystem;
namespace uMMORPG
{
    public class UmaStyleButtonBarbershop : MonoBehaviour
    {
        public enum StyleType { Hair, Beard, Eyebrow, Tattoo }

        [Header("Configuration")]
        public StyleType styleType = StyleType.Hair;
        public bool increase = true;
        public TextMeshProUGUI indexText;

        private Button changeButton;
        private DynamicCharacterAvatar dca;

        
        private void OnEnable()
        {
            Player localPlayer = Player.localPlayer;
            if (localPlayer != null)
            {
                dca = localPlayer.GetComponentInChildren<DynamicCharacterAvatar>();
                UpdateIndexText();
            }
            else
            {
                Debug.LogWarning("UmaStyleButtonBarbershop: No local player found on OnEnable.");
            }

        }

        private void Start()
        {
            changeButton = GetComponent<Button>();
            if (changeButton != null)
            {
                changeButton.onClick.RemoveAllListeners();
                changeButton.onClick.AddListener(ChangeStyle);
            }
            else
            {
                Debug.LogError("UmaStyleButtonBarbershop: No Button component found.");
            }
        }

        public void ChangeStyle()
        {
            Player localPlayer = Player.localPlayer;
            if (localPlayer == null)
            {
                Debug.LogWarning("No local player found");
                return;
            }

            battlescripts bs = localPlayer.GetComponentInChildren<battlescripts>();
            DynamicCharacterAvatar dca = localPlayer.GetComponentInChildren<DynamicCharacterAvatar>();

            if (bs == null || dca == null)
            {
                Debug.LogWarning("battlescripts or DCA missing");
                return;
            }

            bool isMale = dca.activeRace.name == UI_BarberShop.Instance.dcaRaceMale;

            int currentIndex = 0;
            int count = 0;

            switch (styleType)
            {
                case StyleType.Hair:
                    currentIndex = bs.hairIndex;
                    count = UI_BarberShop.Instance.maleHairStyles.Count;
                    break;
                case StyleType.Beard:
                    currentIndex = bs.beardIndex;
                    count = UI_BarberShop.Instance.maleBeardStyles.Count;
                    break;
                case StyleType.Eyebrow:
                    currentIndex = bs.eyebrowIndex;
                    count = UI_BarberShop.Instance.maleEyebrowStyles.Count;
                    break;
                case StyleType.Tattoo:
                    currentIndex = bs.tattooIndex;
                    count = UI_BarberShop.Instance.maleTattooStyles.Count;
                    break;
            }

            int newIndex = ChangeIndex(currentIndex, count);

            // Update the player's battlescripts indexes here:
            switch (styleType)
            {
                case StyleType.Hair:
                    bs.hairIndex = newIndex;
                    break;
                case StyleType.Beard:
                    bs.beardIndex = newIndex;
                    break;
                case StyleType.Eyebrow:
                    bs.eyebrowIndex = newIndex;
                    break;
                case StyleType.Tattoo:
                    bs.tattooIndex = newIndex;
                    break;
            }

            // Update visuals and UI text
            ApplyStyle(newIndex, isMale);
            UpdateIndexText();

            // IMPORTANT: Notify the server about the changes using your command!
            if (bs.isLocalPlayer)
            {
                bs.CmdSetStyleIndexes(bs.hairIndex, bs.beardIndex, bs.eyebrowIndex, bs.tattooIndex);
            }
        }

        private int ChangeIndex(int currentIdx, int count)
        {
            if (count <= 0) return 0;
            return increase ? (currentIdx + 1) % count : (currentIdx - 1 + count) % count;
        }

        private void UpdateIndexText()
{
    if (indexText == null) return;

    Player localPlayer = Player.localPlayer;
    battlescripts bs = localPlayer?.GetComponentInChildren<battlescripts>();
    if (bs == null)
    {
        indexText.text = "0";
        return;
    }

    int index = styleType switch
    {
        StyleType.Hair => bs.hairIndex,
        StyleType.Beard => bs.beardIndex,
        StyleType.Eyebrow => bs.eyebrowIndex,
        StyleType.Tattoo => bs.tattooIndex,
        _ => -1
    };

    indexText.text = index >= 0 ? (index + 1).ToString() : "0";
}

        private void ApplyStyle(int selectedIndex, bool isMale)
        {
            UMATextRecipe recipe = GetRecipe(selectedIndex, isMale, out string slotName);

            if (recipe == null || string.IsNullOrEmpty(slotName))
            {
                Debug.LogWarning($"ApplyStyle: Invalid recipe or slot for {styleType} index {selectedIndex}");
                return;
            }

            Debug.Log($"Applying {recipe.name} to {slotName} for {styleType}");

            // Use this instead of SetSlot with string parameters:
            dca.SetSlot(recipe);

            StartCoroutine(ApplyAndBuildUMA(recipe));
        }

        private UMATextRecipe GetRecipe(int index, bool isMale, out string slotName)
        {
            slotName = styleType switch
            {
                StyleType.Hair => "Hair",
                StyleType.Beard => "Beard",
                StyleType.Eyebrow => "Eyebrow",
                StyleType.Tattoo => "Tattoos", // Adjust based on overlay location
                _ => ""
            };

            return styleType switch
            {
                StyleType.Hair =>SafeGet(UI_BarberShop.Instance.maleHairStyles, index),         
                StyleType.Beard => SafeGet(UI_BarberShop.Instance.maleBeardStyles, index),
                StyleType.Eyebrow => SafeGet(UI_BarberShop.Instance.maleEyebrowStyles, index),
                StyleType.Tattoo => SafeGet(UI_BarberShop.Instance.maleTattooStyles, index),
                _ => null
            };
        }

        private UMATextRecipe SafeGet(System.Collections.Generic.List<UMATextRecipe> list, int index)
        {
            return (list != null && index >= 0 && index < list.Count) ? list[index] : null;
        }

        private IEnumerator ApplyAndBuildUMA(UMATextRecipe recipe)
        {
            bool buildComplete = false;

            UnityAction<UMA.UMAData> onUpdated = null;
            onUpdated = (data) =>
            {
                buildComplete = true;
                dca.CharacterUpdated.RemoveListener(onUpdated);
            };

            dca.CharacterUpdated.AddListener(onUpdated);
            dca.BuildCharacter();

            yield return new WaitUntil(() => buildComplete);

            RenderAvatarCamera();

            Debug.Log("UMA character built and camera rendered.");
        }

        private void RenderAvatarCamera()
        {
            var player = Player.localPlayer;
            if (player == null) return;

            var cam = ((PlayerEquipment)player.equipment)?.avatarCamera;
            if (cam != null)
            {
                cam.enabled = true;
                cam.targetTexture = UI_BarberShop.Instance.renderTexture;
                UI_BarberShop.Instance.image.texture = UI_BarberShop.Instance.renderTexture;
                cam.Render();
            }
        }
    }
}
