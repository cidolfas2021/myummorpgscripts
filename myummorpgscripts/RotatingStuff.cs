using UnityEngine;
using GameZero;
//Attach this script to a GameObject to rotate around the target position.
public class RotatingStuff : MonoBehaviour
{
    //Assign a GameObject in the Inspector to rotate around
    public GameObject target;
    public bool rotatingbuffs;
    public GameObject redOrb;
    public GameObject blueOrb;
    public GameObject purpleOrb;
    public GameObject greyOrb;
    void Update()
    {

        transform.Rotate(0, 180 * Time.deltaTime, 0);

    }
}