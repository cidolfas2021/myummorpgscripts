// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
using UnityEngine;
using UnityEngine.UI;

    public partial class UIArenaNpcDialogue : MonoBehaviour
    {
        public static UIArenaNpcDialogue singleton;
        public GameObject panel;
        //public Transform offerPanel;
        //public GameObject offerButtonPrefab;
        //public Transform offerPanel;
       
        public UIArenaNpcDialogue()
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
        if (panel != null)
        {
            panel.SetActive(true);
            Debug.Log("Panel active state after Show(): " + panel.activeSelf);
        }
        else
        {
            Debug.LogError("Panel reference is null.");
        }
    }
    }


