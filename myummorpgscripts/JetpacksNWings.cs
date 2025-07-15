using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;
using uMMORPG;


public class JetpacksNWings : MonoBehaviour
{
    //public bool WearingWings = false;
    public GameObject equipmentObject; // Single GameObject for Jetpack, Angel Wings, or Demon Wings
    public Animator animator;
    public enum EquipmentType { None, Jetpack, AngelWings, DemonWings }
    public EquipmentType currentEquipment = EquipmentType.None;

    
   

}