using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using uMMORPG;
using UMA;

namespace GameZero
{
    public enum UmaColorTypes
    {
        Skin,
        Hair,
        Eyes,
        Beard,
        Tattoo
    }

    public class UmaEditColorsbarbershop : MonoBehaviour, IPointerClickHandler
    {
        public Color selectedColor;
        public UmaColorTypes Category;
        private UI_BarberShop barberShop;
        public Image imageColor;

        private void OnEnable()
        {
            // Find the UI_BarberShop instance in the scene
            barberShop = FindFirstObjectByType<UI_BarberShop>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (barberShop == null) return;

            Vector2 uv = GetPointerUVPosition();
            selectedColor = GetColor(uv);

            //if (imageColor != null)
                //imageColor.color = selectedColor;

            barberShop.ChangeColor(Category, selectedColor); // <-- THIS IS WHAT LINKS THEM
        }

        private Color GetColor(Vector2 pos)
        {
            Image image = GetComponent<Image>();
            Texture2D originalTexture = image.sprite.texture;

            // Safety check
            if (originalTexture == null)
            {
                Debug.LogWarning("No texture found on image.");
                return Color.white;
            }

            // Read pixel color without altering the original image
            if (!originalTexture.isReadable)
            {
                Debug.LogError("Texture is not readable. Enable Read/Write in import settings.");
                return Color.white;
            }

            // Sample color
            Color color = originalTexture.GetPixelBilinear(pos.x, pos.y);
            color.a = 1f; // Force full alpha
            return color;
        }

        private Vector2 GetPointerUVPosition()
        {
            RectTransform rt = GetComponent<RectTransform>();
            Vector2 localCursor;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, Input.mousePosition, null, out localCursor);

            Rect rect = rt.rect;
            float uvX = Mathf.InverseLerp(rect.xMin, rect.xMax, localCursor.x);
            float uvY = Mathf.InverseLerp(rect.yMin, rect.yMax, localCursor.y);
            return new Vector2(uvX, uvY);
        }
    }
}