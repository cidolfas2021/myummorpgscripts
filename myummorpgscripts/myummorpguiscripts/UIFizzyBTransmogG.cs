// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
using UnityEngine;
using UnityEngine.UI;
namespace GameZero
{
    public partial class UIFizzyBTransmogG : MonoBehaviour
    {
        public static UIFizzyBTransmogG singleton;
        public GameObject panel;
        //public Transform offerPanel;
        public GameObject offerButtonPrefab;
        //public Transform offerPanel;
        public bool isPanelOpened = false;
        public UIFizzyBTransmogG()
        {
            // assign singleton only once (to work with DontDestroyOnLoad when
            // using Zones / switching scenes)
            if (singleton == null) singleton = this;
        }

        void Update()
        {

        }
        public void Start()
        {
            Debug.Log("UIArenaNpcDialogue script initialized");
        }

        public void Show()
        {
            panel.SetActive(true);
            isPanelOpened = true;

            Debug.Log("Panel is opened: " + isPanelOpened);
        }
    }
}
