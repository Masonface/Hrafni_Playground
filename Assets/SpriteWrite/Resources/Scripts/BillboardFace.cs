using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using DaggerfallWorkshop.Game.Utility.ModSupport;

//[ImportedComponent]
public class BillboardFace : MonoBehaviour
{

    public GameObject player;
    public Vector3 rotationVector;
    public Camera playerCamera;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("MainCamera");
        playerCamera = player.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {

        rotationVector = new Vector3(0, 0, 0);


        if (playerCamera.orthographic)
        {
            rotationVector.y = playerCamera.transform.eulerAngles.y;
        }

        else
        {
            this.transform.LookAt(player.transform);
            rotationVector.y = this.transform.eulerAngles.y + 180f;
        }

        this.transform.eulerAngles = rotationVector;

    }

    public void CenterBillboard(float xPosition, float zPosition)
    {
        this.transform.localPosition = new Vector3(xPosition, 0f, zPosition);
    }
}
