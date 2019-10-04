using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    /*  RampageCrumpet
        2/22/2019

        Attached to the camera to make it follow the target.
    */

    [Tooltip("The target to follow with the main camera.")]
    [SerializeField]private GameObject cameraTarget;


    [Tooltip("The size of the pixel perfect world we want to snap to.")]
    [SerializeField]private int pixelsPerWorldUnit;

    // Update is called once per frame
    void Update()
    {
        if(cameraTarget != null)
        {
            Vector3 newPosition = new Vector3();
            newPosition = cameraTarget.transform.position;
            newPosition.z = this.gameObject.transform.position.z;

            this.gameObject.transform.position = CameraSnap(newPosition);
        }
    }

    //Snaps the camera to a pixel perfect position.
    private Vector3 CameraSnap(Vector3 snapPosition)
    {
        if(pixelsPerWorldUnit != 0)
        {
            Vector3 finalPosition = new Vector3(); 
                
            finalPosition.x = (Mathf.RoundToInt(snapPosition.x/(1.0f/pixelsPerWorldUnit))*(1.0f/pixelsPerWorldUnit));
            finalPosition.y = (Mathf.RoundToInt(snapPosition.y/(1.0f/pixelsPerWorldUnit))*(1.0f/pixelsPerWorldUnit));
            finalPosition.z = this.transform.position.z;

            return finalPosition;
        }
        else
        {
            Debug.LogError("Your camera's pixelsPerWorldUnit is set to 0!");
            return snapPosition;
        }        
    }

}
