using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseMove : MonoBehaviour
{
    public Animator animator3D;
    public Animator animatorSW;
    public bool Attacking = false;
    public AudioSource aS;
    public AudioClip attackSound;
    public float attackTime;

    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    public RotationAxes axes = RotationAxes.MouseXAndY;
    public float sensitivityX = 15F;
    public float sensitivityY = 15F;
    public float minimumX = -360F;
    public float maximumX = 360F;
    public float minimumY = -60F;
    public float maximumY = 60F;
    float rotationX = 0F;
    float rotationY = 0F;
    Quaternion originalRotation;
    void Update()
    {
        if (axes == RotationAxes.MouseXAndY)
        {
            // Read the mouse input axis
            rotationX += Input.GetAxis("Mouse X") * sensitivityX;
            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
    
            Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
            Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, -Vector3.right);
            transform.localRotation = originalRotation * xQuaternion * yQuaternion;
        }
        else if (axes == RotationAxes.MouseX)
        {
            rotationX += Input.GetAxis("Mouse X") * sensitivityX;
           
            Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
            transform.localRotation = originalRotation * xQuaternion;
        }
        else
        {
            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
            
            Quaternion yQuaternion = Quaternion.AngleAxis(-rotationY, Vector3.right);
            transform.localRotation = originalRotation * yQuaternion;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (!Attacking)
            {
                StartCoroutine(Attack());
            }
        }
    }

    public IEnumerator Attack()
    {
        
        Debug.Log("Attack!");
        Attacking = true;
        animator3D.SetBool("Attack", true);
        animatorSW.SetBool("Attack", true);
        aS.PlayOneShot(attackSound);

        yield return new WaitForSeconds(attackTime);

        animator3D.SetBool("Attack", false);
        animatorSW.SetBool("Attack", false);
        Attacking = false;

    }

    void Start()
    {
        originalRotation = transform.localRotation;
    }
   
}
