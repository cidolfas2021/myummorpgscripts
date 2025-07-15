using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GameZero;
namespace uMMORPG { 
public partial class UIEquipmentPreview : MonoBehaviour
{
    public GameObject panel;

    [SerializeField] Button buttonClose, buttonReset;
    
    [SerializeField] MyEquipmentPreviewer previewer;
    [SerializeField] Button rotateLeftButton;
    [SerializeField] Button rotateRightButton;

    void Start()
    {
        // Initialize button listeners
        if (rotateLeftButton != null)
            rotateLeftButton.onClick.AddListener(previewer.RotateLeft);
        if (rotateRightButton != null)
            rotateRightButton.onClick.AddListener(previewer.RotateRight);
    }
    private void Awake()
    {
        buttonClose.onClick.AddListener(Close);
        buttonReset.onClick.AddListener(Reset);
        

        // Remove the dragging functionality
        EventTrigger trigger = GetComponentInChildren<EventTrigger>(true);
        if (trigger != null)
        {
            foreach (var entry in trigger.triggers)
            {
                if (entry.eventID == EventTriggerType.Drag)
                {
                    trigger.triggers.Remove(entry);
                    break;
                }
            }
        }
    }

    public void Open()
    {
        
        panel.SetActive(true);
    }

    void Close()
    {
       
        panel.SetActive(false);
    }
    public void Reset()
    {
        Debug.Log("Reset button clicked"); // Confirm this logs
        previewer.ResetToCurrent();
    }
}
}