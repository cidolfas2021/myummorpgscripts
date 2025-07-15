// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
//using System.Drawing.Drawing2D;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace GameZero
{
    public partial class UIArenaExitDialogue : MonoBehaviour
    {

        public static UIArenaExitDialogue singleton;
        public GameObject panel;
        //public Transform offerPanel;
        public GameObject offerButtonPrefab;
        //public Transform offerPanel;
        public bool isPanelOpened = false;
        public bool gameEnded = false;
        public bool warpall = false;
        public KeyCode hotKey = KeyCode.L;
        public void Update()
        {
            if (Input.GetKeyDown(hotKey) && !uMMORPG.UIUtils.AnyInputActive())
                panel.SetActive(!panel.activeSelf);
        }
        public UIArenaExitDialogue()
        {
            // assign singleton only once (to work with DontDestroyOnLoad when
            // using Zones / switching scenes)
            if (singleton == null) singleton = this;
        }
        public void OnButtonClick()
        {
            gameEnded = true;
        }
        public void warpallbuttonclick()
        {
            warpall = true;
        }


        public void Show()
        {
            panel.SetActive(true);
            isPanelOpened = true;

            Debug.Log("Panel is opened: " + isPanelOpened);
        }
    }
}