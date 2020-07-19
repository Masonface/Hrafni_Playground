using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteBake : MonoBehaviour
{

    //Subject
    public bool normals;
    public GameObject spriteObj;
    private Bounds subjectBounds;
    public List<Renderer> meshRenderers;
    private Transform[] childrenTrans;
    public List<GameObject> childrenGO;
    public Vector3 objSize;
    private Vector3 objCenter;
    private Bounds encapBounds;
    private Vector3 subjectCenter;
    public Renderer testObj;
 

    //Render Texture
    private RenderTexture rT;
    private Renderer _renderer;
    private RenderTextureFormat rTformat;
    private FilterMode sFilterMode;
    public float pixelsPerUnit;
    public int rendTexX = 200;
    public int rendTexY = 250;
    private RenderTextureDescriptor rendTexDesc;

    //Camera
    public ViewAngles viewAngles;
    public BakeType bakeType;
    private float angleSpan;
    private float cameraWidth;
    public Camera spriteCamera;
    public bool grab = false;
    public Transform SpriteCameraPivot;
    //Material
    private Material spriteMaterial;
    private MaterialPropertyBlock _propBlock;

    //Texture2D
    public Texture2D outputTexture;


    public string directory;
    public string filename;
    public int frame;
    public int views;

    public int width;
    public int height;
    public int depth;

    public float minDepth;
    public float maxDepth;
    public float deltaDepth;
    public int depthSlices;

    private string normalString;

    // Start is called before the first frame update
    void Start()
    {

        //int views = 8;

        if (viewAngles == ViewAngles.Four)
        {
            views = 4;
        }
        else if (viewAngles == ViewAngles.Eight)
        {
            views = 8;
        }
        else if (viewAngles == ViewAngles.Sixteen)
        {
            views = 16;
        }
        else if (viewAngles == ViewAngles.ThirtyTwo)
        {
            views = 32;
        }


        _renderer = spriteObj.GetComponent<Renderer>();
        _propBlock = new MaterialPropertyBlock();

        
        rTformat = RenderTextureFormat.ARGB32;
        GetRenderers();
        GetBounds();
        rendTexX = Mathf.FloorToInt(objSize.x * pixelsPerUnit);
        rendTexY = Mathf.FloorToInt(objSize.y * pixelsPerUnit);
        rendTexDesc = new RenderTextureDescriptor(rendTexX, rendTexY, rTformat, 16);
        rT = new RenderTexture(rendTexDesc);
        rT.useMipMap = true;
        rT.autoGenerateMips = true;
        rT.filterMode = FilterMode.Point;
        rT.descriptor = rendTexDesc;
        rT.Create();
        angleSpan = 360f / views;

        spriteCamera.targetTexture = rT;

        filename = spriteObj.gameObject.name;
        frame = 0;
        deltaDepth = objSize.z / depthSlices;
        minDepth = this.transform.localPosition.x - spriteObj.transform.position.x - objSize.x * 0.5f;

        if(bakeType == BakeType.Volume)
        {
            maxDepth = minDepth + deltaDepth;
        }
        else
        {
            maxDepth = minDepth + objSize.x;
        }
        spriteCamera.nearClipPlane = minDepth;
        spriteCamera.farClipPlane = maxDepth;

    }

    // Update is called once per frame
    void Update()
    {
        //Press space to start the screen grab
        if (Input.GetKeyDown(KeyCode.Space))
        {

            if (bakeType == BakeType.Rotational)
            {
                for (int i = 0; i < views; i++)
                {


                    //Need to cycle through each view and determine the biggest frame size
                    //if (texturesToLoad[i].height >= height)
                    //{
                    //    height = texturesToLoad[i].height;
                    //}

                }

                GetBounds();
                changeRotation();
            }

            else if(bakeType == BakeType.Volume)
            {
                GetBounds();
                changeDepth();
            }
        }

        //Press space to start the screen grab
        //if (Input.GetKeyDown(KeyCode.P))
        //{
        //    
        //}
    }

    public void changeRotation()
    {
        TakeScreenShot();
        float angle = angleSpan * frame;
        SpriteCameraPivot.transform.eulerAngles = new Vector3(0, angle, 0);
    }

    public void changeDepth()
    {

        //for (int i = 0; i < depthSlices; i++)
        //{
            TakeScreenShot();
            minDepth = maxDepth;
            maxDepth += deltaDepth;

        //}
        
    }

    public void GetRenderers()
    {
        //Find at all the children (all children will have a transform, so that's what we'll look for)
        childrenTrans = spriteObj.GetComponentsInChildren<Transform>();

        //Now, for each child, check that it has a renderer, and it's something that we want the sprite camera to see.
        //If so, add it to the meshRenderers list. We will use that list to calculate the bounds of all the meshes together
        // by using the GetBounds method.
        foreach (Transform childTrans in childrenTrans)
        {
            if ((childTrans.GetComponent<Renderer>() != null) && (childTrans.gameObject.layer == LayerMask.NameToLayer("3D Model")))
            {
                meshRenderers.Add(childTrans.GetComponent<Renderer>());
                childrenGO.Add(childTrans.gameObject);
            }
        }

    }

    public enum BakeType
    {
        Rotational,
        Volume
    }

    public enum ViewAngles
    {
        Four,
        Eight,
        Sixteen,
        ThirtyTwo
    }

    public void GetBounds()
    {

        subjectBounds = meshRenderers[meshRenderers.Count - 1].bounds;

        //If you have only one renderer, simply get the bounds.
        if (meshRenderers.Count == 1)
        {
            objSize = subjectBounds.size;
            objCenter = subjectBounds.center;

        }

        //If you have more than one, encapsulate them to get their total bounds.
        else
        {
            //Reset the encapsulated bounds.
            encapBounds.extents = Vector2.zero;

            //Grow the bounds to include all renderers in children.
            foreach (Renderer mR in meshRenderers)
            {
                encapBounds.Encapsulate(mR.bounds);
            }

            objSize = encapBounds.size;
            objCenter = encapBounds.center;
            subjectCenter = subjectBounds.center;

        }

    }

    public void TakeScreenShot()
    {

        if (normals)
        {
            normalString = "_Normal";
        }
        else
        {
            normalString = "";
        }

        spriteCamera.nearClipPlane = minDepth;
        spriteCamera.farClipPlane = maxDepth;
        
        rendTexX = Mathf.FloorToInt(objSize.x * pixelsPerUnit);
        rendTexY = Mathf.FloorToInt(objSize.y * pixelsPerUnit);
        outputTexture = new Texture2D(rendTexX, rendTexY, TextureFormat.ARGB32, false);
        
        RenderTexture.active = rT;

        outputTexture.ReadPixels(new Rect(0, 0, rendTexX, rendTexY), 0, 0, false);
        outputTexture.Apply();
        

        byte[] bytes = outputTexture.EncodeToPNG();
        System.IO.File.WriteAllBytes(directory + filename + "_" + frame + normalString + ".png", bytes);

        testObj.material.mainTexture = outputTexture;

        Debug.Log("Writing " + directory + filename + "_" + frame + normalString + ".png");
        RenderTexture.active = null;
        frame++;


    }



}
