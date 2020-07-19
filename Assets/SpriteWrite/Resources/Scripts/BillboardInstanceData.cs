using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardInstanceData : MonoBehaviour
{

    public int index;
    public Renderer Rend;
    public Material Mat;


    // Start is called before the first frame update
    void Start()
    {
        Rend = this.gameObject.GetComponent<Renderer>();
        Mat = Rend.material;
        Mat.SetFloat("_Index", index);
    }

    // Update is called once per frame
    void Update()
    {
     
    }

}
