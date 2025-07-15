using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Mirror;
using uMMORPG;
namespace GameZero
{
    public class UIWorldmap : NetworkBehaviour
    {
        private List<Image> markers = new List<Image>();
        [SerializeField] Button buttonClose;
        public GameObject panel;
        public RawImage worldmap;
        public float zoomMin = 0.1f;
        public float zoomMax = 10f;
        public float zoomStepSize = 0.1f;
        public float panningSpeed = 0.5f;

        public Button plusButton;
        public Button minusButton;
        public KeyCode hotKey = KeyCode.M;
        private float initialZoom = 1f;

        private Vector2 originalSizeDelta;
        private Vector2 originalPivot;
        private Vector2 originalPosition;
        private bool isDragging = false;
        private Vector2 dragStartPosition;
        public Text locationText;

        [SerializeField] private Image playerMarkerPrefab;
        // [SerializeField] private Image localplayerMarker;
        [SerializeField] private Image npcMarkerPrefab;
        [SerializeField] private Image enemyMarkerPrefab;
        public Text hoverCoordinatesText;

        // New fields for visRange management
        //private int defaultVisRange = 50;
        //private int mapVisRange = 50000;
        private bool isMapActive = false;

        //public SpatialHashingInterestManagement shim;
        void Start()
        {
            originalSizeDelta = worldmap.rectTransform.sizeDelta;
            originalPivot = worldmap.rectTransform.pivot;
            originalPosition = worldmap.rectTransform.anchoredPosition;

            worldmap.rectTransform.localScale = Vector3.one * initialZoom;

            panel.SetActive(false);

            /*if (localplayerMarker == null && playerMarkerPrefab != null)
            {
                localplayerMarker = Instantiate(playerMarkerPrefab, worldmap.transform);
                localplayerMarker.gameObject.SetActive(false);
            }*/
        }

        private void Awake()
        {
            buttonClose.onClick.AddListener(Close);
        }





        void Update()
        {
            try
            {


                Player player = Player.localPlayer;
                if (!player)
                {
                    locationText.text = "";
                    /*if (localplayerMarker != null)
                    {
                        localplayerMarker.gameObject.SetActive(false);
                    }*/
                    return;
                }

                locationText.text = $"X: {player.transform.position.x:F0} / Z: {player.transform.position.z:F0}";

                Vector2 mapPosition = ConvertWorldToMap(player.transform.position);

                if (panel.activeSelf)
                {
                    CmdClearMarkers();
                    //CmdPlaceMarker(mapPosition);
                    CmdUpdateOtherMarkers();

                }

                if (Input.GetKeyDown(hotKey))
                {
                    ToggleMapView();
                }

                if (panel.activeSelf)
                {
                    HandleHoverCoordinates();
                    //HandleClickCoordinates();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error in UIWorldmap Update: " + ex.Message);
            }
        }

        void ResetMap()
        {
            worldmap.rectTransform.sizeDelta = originalSizeDelta;
            worldmap.rectTransform.pivot = originalPivot;
            worldmap.rectTransform.localScale = Vector3.one * initialZoom;
            worldmap.rectTransform.anchoredPosition = originalPosition;
        }

        void HandleHoverCoordinates()
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                worldmap.rectTransform, Input.mousePosition, null, out Vector2 localPoint))
            {
                // Shift hover area DOWN by 300 pixels
                localPoint.y -= -275f;

                float width = worldmap.rectTransform.rect.width;
                float height = worldmap.rectTransform.rect.height;

                float xNorm = (localPoint.x + width * 0.5f) / width;
                float yNorm = (localPoint.y + height * 0.5f) / height;

                // Only allow coordinates inside panel bounds (0..1 normalized space)
                if (xNorm >= 0 && xNorm <= 1 && yNorm >= 0 && yNorm <= 1)
                {
                    Vector2 worldHoverPosition = new Vector2(
                        xNorm * 30000 - 15000,
                        (1.0f - yNorm) * 30000 - 15000);

                    hoverCoordinatesText.text = $"X: {worldHoverPosition.x:F0} / Z: {worldHoverPosition.y:F0}";
                }
                else
                {
                    hoverCoordinatesText.text = "";
                }
            }
            else
            {
                hoverCoordinatesText.text = "";
            }
        }

        /*
        void HandleClickCoordinates()
        {
            if (Input.GetMouseButtonDown(1))
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    worldmap.rectTransform, Input.mousePosition, null, out Vector2 localPoint);

                Vector2 normalizedPoint = new Vector2(
                    (localPoint.x + worldmap.rectTransform.rect.width * 0.5f) / worldmap.rectTransform.rect.width,
                    (localPoint.y + worldmap.rectTransform.rect.height * 0.5f) / worldmap.rectTransform.rect.height);

                Vector2 worldClickPosition = new Vector2(
                    normalizedPoint.x * 30000 - 15000,
                    (1.0f - normalizedPoint.y) * 30000 - 15000);

                Debug.Log($"Clicked world coordinates: ({worldClickPosition.x}, {worldClickPosition.y})");
            }
        }

        void ClampToPanel()
        {
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            Vector3[] worldCorners = new Vector3[4];
            worldmap.rectTransform.GetWorldCorners(worldCorners);

            Vector3[] panelCorners = new Vector3[4];
            panelRect.GetWorldCorners(panelCorners);

            Vector3 min = panelCorners[0];
            Vector3 max = panelCorners[2];

            Vector3 pos = worldmap.rectTransform.anchoredPosition;

            if (worldCorners[0].x > min.x)
                pos.x -= worldCorners[0].x - min.x;
            if (worldCorners[2].x < max.x)
                pos.x -= worldCorners[2].x - max.x;

            if (worldCorners[0].y > min.y)
                pos.y -= worldCorners[0].y - min.y;
            if (worldCorners[2].y < max.y)
                pos.y -= worldCorners[2].y - max.y;

            worldmap.rectTransform.anchoredPosition = pos;
        }*/

        Vector2 ConvertWorldToMap(Vector3 worldPosition)
        {
            //float mapX = (worldPosition.x + 15000) / 30000 * 675f;
            //float mapY = (worldPosition.z + 15000) / 30000 * 575f;
            float mapX = (worldPosition.x + 15000) / 30000 * 800f;
            float mapY = (worldPosition.z + 15000) / 30000 * 550f;
            float mapOffsetX = worldmap.rectTransform.rect.width * 0.5f;
            float mapOffsetY = worldmap.rectTransform.rect.height * 0.5f;

            return new Vector2(mapOffsetX - mapX, mapOffsetY - mapY);
        }
        /*[Command(requiresAuthority = false)]
        void CmdPlaceMarker(Vector2 mapPosition)
        {
            if (localplayerMarker != null)
            {
                localplayerMarker.rectTransform.anchoredPosition = mapPosition;
                localplayerMarker.gameObject.SetActive(true);
            }
        }*/



        [Command(requiresAuthority = false)]
        void CmdUpdateOtherMarkers()
        {
            // Clear existing markers before adding new ones


            NetworkIdentity[] networkIdentities = FindObjectsOfType<NetworkIdentity>();

            foreach (NetworkIdentity identity in networkIdentities)
            {
                GameObject networkObject = identity.gameObject;

                Vector3 worldPosition = networkObject.transform.position;
                string tag = networkObject.tag;

                RpcUpdateMarker(worldPosition, tag);
            }
        }

        [ClientRpc]
        void RpcUpdateMarker(Vector3 worldPosition, string tag)
        {
            Vector2 mapPosition = ConvertWorldToMap(worldPosition);
            Image marker = null;

            // Instantiate the appropriate marker based on the tag
            switch (tag)
            {
                case "Player":
                    if (playerMarkerPrefab != null)
                        marker = Instantiate(playerMarkerPrefab, worldmap.transform);
                    break;
                case "Npc":
                    if (npcMarkerPrefab != null)
                        marker = Instantiate(npcMarkerPrefab, worldmap.transform);
                    break;
                case "Monster":
                    if (enemyMarkerPrefab != null)
                        marker = Instantiate(enemyMarkerPrefab, worldmap.transform);
                    break;
            }

            if (marker != null)
            {
                marker.rectTransform.anchoredPosition = mapPosition;
                marker.gameObject.SetActive(true);
                markers.Add(marker);
            }
        }

        [Command(requiresAuthority = false)]
        void CmdClearMarkers()
        {
            // Destroy markers on the server and tell the clients to update
            foreach (Image marker in markers)
            {
                Destroy(marker.gameObject);
            }
            markers.Clear();

            // Notify all clients to clear their markers
            RpcClearMarkers();
        }

        // This RPC will be used to clear markers on all clients
        [ClientRpc]
        void RpcClearMarkers()
        {
            // Clear markers on all clients
            foreach (Image marker in markers)
            {
                if (marker != null)
                {
                    Destroy(marker.gameObject);
                }
            }
            markers.Clear();
        }



        void ToggleMapView()
        {
            panel.SetActive(!panel.activeSelf);
            isMapActive = panel.activeSelf;


        }

        public void Open()
        {

            panel.SetActive(true);
        }

        void Close()
        {

            panel.SetActive(false);
        }
        

    }
}