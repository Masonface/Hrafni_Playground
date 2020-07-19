using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using DaggerfallWorkshop.Game.Utility.ModSupport;

//[ImportedComponent]
public class SpriteWrite : MonoBehaviour
{

    private float angleBetweenHor = 0f;
    private float angleBetweenVert = 0f;
    private SpriteWriteManager spriteWriteManager;

    [SerializeField] public bool spriterEnabled = true;

    [Header("Quality Settings")]
    [Tooltip("Frames per second for the sprite animation. Higher: smoother animations. Lower: more discrete (jerky). Lowering this number will increase performance.")]
    [Range(2f, 12f)] [SerializeField] public int FPS = 6;


    private bool prevSpriterEnabledState;
    private int rendTexX = 200;
    private int rendTexY = 250;

    private float period;
    private readonly float spriteCamAngle;
    private float angleSpan;
    private float angleSpanVert;
    private float rotateVectorY;
    private float prevRotVecY;
    private float rotateVectorX;
    private Vector3 playerRot;
    private RenderTexture rT;
    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;
    private RenderTextureDescriptor rendTexDesc;
    private float frameClock;
    private bool frameClockOn;
    [System.NonSerialized] public bool allowUpdate;
    private Vector3 objSize;
    private Vector3 objCenter;
    private Vector3 subjectCenter;
    private readonly float subjectCenterX;
    private readonly float spriteCenterX;
    private float farClipCameraPlane;
    private float nearClipCameraPlane;
    private Vector3 cameraPosition;
    private Vector3 spriteLocalScale;
    private Vector3 spriteScaleTemp;
    private Vector3 spriteCenterTemp;
    private float cameraWidth;
    private Vector3 subjectOrientation;
    private Vector3 V;
    private float magV;
    private Camera playerCamera;
    private Camera spriteCamera;
    private GameObject spriteCameraPivot;
    private GameObject spriteOrigin;
    private GameObject spriteObj;
    private Material spriteMaterial;
    private float pixelsPerUnit;
    private RenderTextureFormat rTformat;
    private FilterMode sFilterMode;
    private Bounds subjectBounds;
    private Bounds encapBounds;
    private Bounds tempBounds;
    private BillboardFace billBoardFace;

    private bool LODEnabled;
    private LOD[] LODLevel;
    private float distance;
    private int LODResX;
    private int LODResY;
    private Transform BasePlane;


    public enum ViewAngles
    {
        Four,
        Eight,
        Sixteen
    }

    public enum ViewAnglesVert
    {
        One,
        Three,
        Five
    }

    public enum QualitySettings
    {
        VeryLow,
        Low,
        Medium,
        High,
        VeryHigh
    }

    public enum ColorDepth
    {
        Low,
        High
    }

    public enum BoundsType
    {
        Renderers,
        Colliders,
        StaticBox
    }

    [System.Serializable]
    public struct LOD
    {
        public float distanceAway;
        public float percentRes;
        public float percentFPS;
    }


    [Tooltip("Number of viewing angles around the subject.")]
    public ViewAngles viewAngles;
    [Tooltip("Number of viewing angles above and below the subject.")]
    private ViewAnglesVert viewAnglesVert;
    [Tooltip("Sets the resolution of the sprite. VeryLow = Pixel Art, Low = Doom, Medium = Daggerfall")]
    public QualitySettings spriteQuality;
    [Tooltip("Low = 15bit, High = 32bit. Low is better for video memory usage and will better emulate older style games.")]
    public ColorDepth colorDepth;
    [Tooltip("Enables bilinear filtering. Makes subject look less jaggy, but more blurry. Similar look to Nintendo 64 sprites.")]
    [SerializeField] private bool filter;
    [Tooltip("Renders shadows cast by the subject onto itself.")]
    [SerializeField] private bool selfShadowing;
    public bool UMACharacter;
    public float UMADelay;
    public BoxCollider staticBoxBounds;
    public BoundsType boundsType;
    public SphereCollider sphereCollider;
    public bool collided;
    public GameObject collGO;
    [Space]

    [Header("Meshes")]
    public List<Renderer> meshRenderers;
    public List<BoxCollider> boxColliders;
    private Transform[] childrenTrans;
    public List<GameObject> childrenGO;
    public Rigidbody rB;

    private int enumValue;

    private void Start()
    {

        //this.name = this.gameObject.GetInstanceID().ToString();
        spriterEnabled = false;

        if (boundsType == BoundsType.StaticBox)
        {
            staticBoxBounds = this.GetComponentInChildren<BoxCollider>();
        }

        playerCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        spriteWriteManager = GameObject.FindObjectOfType<SpriteWriteManager>();
        spriteCamera = this.GetComponentInChildren<Camera>();
        spriteCameraPivot = this.transform.Find("SpriteCameraPivot").gameObject;
        spriteOrigin = this.transform.Find("SpriteOrigin").gameObject;
        spriteObj = this.transform.Find("SpriteOrigin/SpriteObject").gameObject;
        BasePlane = this.transform.Find("BasePlane").transform;
        spriteMaterial = spriteObj.gameObject.GetComponent<Renderer>().material;
        billBoardFace = spriteOrigin.GetComponent<BillboardFace>();
        _renderer = spriteObj.GetComponent<Renderer>();
        _propBlock = new MaterialPropertyBlock();

        //Baseplane need not be renderered in game since it is only for positional reference.
        BasePlane.GetComponent<Renderer>().enabled = false;

        rT = new RenderTexture(32, 32, 0);
        rT.useMipMap = true;
        rT.autoGenerateMips = true;

        if (UMACharacter)
        {
            
            StartCoroutine(DelayGetRenderers());
        }

        else
        {
            //GetRenderers();
            //Invoke("GetRenderers", 0);
            GetRenderers();
            GetBounds();
            subjectOrientation = this.transform.eulerAngles;
            spriteCamera.targetTexture = rT;
            spriteMaterial.mainTexture = rT;
            frameClock = 0.0f;
            frameClockOn = true;
            allowUpdate = true;
            farClipCameraPlane = spriteCamera.farClipPlane;
            spriterEnabled = true;

            UpdateQualitySettings();
        }

        rB.WakeUp();

    }

    private IEnumerator DelayGetRenderers()
    {
        yield return new WaitForSeconds(UMADelay);
        GetRenderers();
        GetBounds();
        subjectOrientation = this.transform.eulerAngles;
        //_renderer = spriteObj.GetComponent<Renderer>();
        //_propBlock = new MaterialPropertyBlock();
        spriteCamera.targetTexture = rT;
        spriteMaterial.mainTexture = rT;
        frameClock = 0.0f;
        frameClockOn = true;
        allowUpdate = false;
        farClipCameraPlane = spriteCamera.farClipPlane;
        spriterEnabled = true;

        UpdateQualitySettings();
    }


    // Update is called once per frame
    void Update()
    {

        if (spriterEnabled)
        {

            //Iterate the frameClock by the time the previous frame took.
            if (frameClockOn)
            {
                frameClock += Time.deltaTime;
            }

            //Convert the object's facing direction relative to the player into a discrete angle around the sprite.
            Vector3 angleVector;
            angleVector = this.transform.position - playerCamera.transform.position;
            angleVector.y = 0;
            angleBetweenHor = Vector3.SignedAngle(this.transform.forward, angleVector, Vector3.up);
            rotateVectorY = Mathf.FloorToInt((angleBetweenHor + angleSpan * 0.5f) / angleSpan) * angleSpan;

            if (viewAnglesVert == ViewAnglesVert.One)
            {
                rotateVectorX = 0f;
            }

            else
            {

                //Get the object's vertical facing direction
                angleVector = spriteObj.transform.position - playerCamera.transform.position;
                int inverter = 1;
                //Vector3 referenceVector = new Vector3(0,0,0);
                inverter = playerCamera.transform.position.y >= spriteObj.transform.position.y ? 1 : -1;
                angleBetweenVert = Vector3.Angle(spriteObj.transform.forward, angleVector) * inverter;


                if (viewAnglesVert == ViewAnglesVert.Three)
                {
                    if (angleBetweenVert >= 45)
                    {
                        rotateVectorX = 45;
                    }
                    else if (angleBetweenVert <= -45)
                    {
                        rotateVectorX = -45;
                    }

                    else
                    {
                        rotateVectorX = 0;
                    }
                }

            }


            //Update the view if the player's viewing angle changes or if the sprite's FPS is due to be updated.
            if ((rotateVectorY != prevRotVecY) || (frameClock >= period))
            {

                //Don't allow the render texture to be updated until the SpriteWriteManager says it is okay.
                if (!allowUpdate)
                {
                    //frameClock = 0.0f;
                    frameClockOn = false;
                    spriteWriteManager.AddToQueue(this.gameObject);
                    
                }

                else if (allowUpdate)
                {
                    UpdateView();
                    //allowUpdate = false;
                }
            }

            

            //Store the current view angle to compare next frame so we know when it changes.
            prevRotVecY = rotateVectorY;

        }


        if (Input.GetKeyUp("j"))
        {
            Debug.Log("Toggled");
            spriterEnabled = !spriterEnabled;
        }

        if (Input.GetKeyUp("u"))
        {
            Debug.Log("Update Sprites");
            UpdateQualitySettings();
            GetBounds();
        }

        if (spriterEnabled != prevSpriterEnabledState)
        {
            ToggleSpriterEnable();
        }

        if (Input.GetKeyUp("="))
        {
            changeRes("+");
        }

        if (Input.GetKeyUp("-"))
        {
            changeRes("-");
        }

        prevSpriterEnabledState = spriterEnabled;

    }

    public void UpdateView()
    {

        frameClock = 0.0f;
        frameClockOn = false;

        if (LODEnabled)
        {
            distance = Vector3.Magnitude(playerCamera.transform.position - this.transform.position);
            int LODstate;

            for (int L = LODLevel.Length - 1; L >= 0; L--)
            {
                if (distance >= LODLevel[L].distanceAway)
                {
                    LODstate = L;
                    changeLOD(L);
                    break;
                }
            }
        }

        //Send the mesh (or meshes) to the 3D Model layer so it can be seen by the render camera.
        foreach (GameObject childGO in childrenGO)
        {
            childGO.layer = LayerMask.NameToLayer("3D Model");
        }

        //Store the subject's original orientation (we will rotate it back later on).
        subjectOrientation = this.transform.eulerAngles;

        //Rotate the subject so that the camera's view is axis aligned for correct measurement.
        this.transform.localEulerAngles = new Vector3(-rotateVectorX, -rotateVectorY, 0);

        //Get the boundary size and center of the subject.
        GetBounds();

        //Scale the sprite object parent occording to the subject's boundary size. 
        //Divide by the parent Object's scale.
        spriteScaleTemp = spriteOrigin.transform.localScale;
        spriteScaleTemp.x = objSize.x / this.transform.localScale.x;
        spriteScaleTemp.y = objSize.y / this.transform.localScale.y;
        spriteScaleTemp.z = objSize.x / this.transform.localScale.z;
        spriteOrigin.transform.localScale = spriteScaleTemp;

        //Rotate the subject back to its correct orientation.
        this.transform.eulerAngles = subjectOrientation;

        //Rotate the camera to the correct view angle.
        spriteCameraPivot.transform.localEulerAngles = new Vector3(rotateVectorX, rotateVectorY, 0);

        //Get the view vector "V"
        V = spriteCameraPivot.transform.position - spriteCamera.transform.position;
        magV = Vector3.Magnitude(V);

        CenterSubject();

        //Set the size of the camera to capture the entire height of the subject.
        spriteCamera.orthographicSize = objSize.y * 0.5f;

        //Set the aspect of the camera to capture the entire width of the subject.
        cameraWidth = objSize.x;
        spriteCamera.aspect = cameraWidth / objSize.y;

        //Set the camera clipping planes to only capture the subject.
        nearClipCameraPlane = (magV - objSize.z / 2);
        farClipCameraPlane = (magV + objSize.z / 2);

        spriteCamera.farClipPlane = farClipCameraPlane;
        spriteCamera.nearClipPlane = nearClipCameraPlane;

        if (childrenGO.Count > 1)
        {
            ReCenterSprite();
        }

        //Set the texture of the sprite object material to this RenderTexture.
        _renderer.GetPropertyBlock(_propBlock);
        _propBlock.SetTexture("_EmissionMap", rT);
        _renderer.SetPropertyBlock(_propBlock);

        UpdateQualitySettings();

        //Send the subject back to "Invisible" layer and set the camera clipping plane to 0.
        StartCoroutine(ResetCameraView());

        //Turn the frame clock back on.
        frameClockOn = true;

    }

    public IEnumerator ResetCameraView()
    {

        yield return new WaitForEndOfFrame();

        //Send the mesh (or meshes) to an invisible layer so it cannot be rendered when not needed.
        foreach (GameObject childGO in childrenGO)
        {
            childGO.gameObject.layer = LayerMask.NameToLayer("Invisible");
        }

        //Set the camera clipping planes to 0 so that nothing can be rendered when not used.
        spriteCamera.farClipPlane = 1000f;
        spriteCamera.nearClipPlane = 1000f;

    }

    void ReCenterSprite()
    {
        //If the subject has multiple children, then the encapsulated render bounds may throw off the
        // alignment between the subject's origin and the sprite object.
        float spriteLocalXposition = spriteCameraPivot.transform.localPosition.x * Mathf.Abs(Vector3.Dot(spriteOrigin.transform.forward, gameObject.transform.forward));
        float spriteLocalZposition = spriteCameraPivot.transform.localPosition.z * Mathf.Abs(Vector3.Dot(spriteOrigin.transform.right, gameObject.transform.forward));
        billBoardFace.CenterBillboard(spriteLocalXposition, spriteLocalZposition);
    }

    public void changeRes(string diff)
    {

        enumValue = (int)spriteQuality;

        if (diff == "+")
        {

            if (enumValue < System.Enum.GetValues(typeof(QualitySettings)).Length - 1)
            {
                enumValue++;
            }
        }

        else
        {
            if (enumValue > 0)
            {
                enumValue--;
            }
        }

        if (enumValue == 0)
        {
            spriteQuality = QualitySettings.VeryLow;
        }

        else if (enumValue == 1)
        {
            spriteQuality = QualitySettings.Low;
        }

        else if (enumValue == 2)
        {
            spriteQuality = QualitySettings.Medium;
        }

        else if (enumValue == 3)
        {
            spriteQuality = QualitySettings.High;
        }

        else if (enumValue == 4)
        {
            spriteQuality = QualitySettings.VeryHigh;
        }

        UpdateQualitySettings();
    }

    public void changeLOD(int L)
    {
        rT.Release();
        LODResX = Mathf.FloorToInt(pixelsPerUnit * objSize.x * LODLevel[L].percentRes * 0.01f);
        LODResY = Mathf.FloorToInt(pixelsPerUnit * objSize.y * LODLevel[L].percentRes * 0.01f);

        rendTexDesc = new RenderTextureDescriptor(LODResX, LODResY, rTformat, 16);

        rT.descriptor = rendTexDesc;
        rT.Create();

        int LODfps = Mathf.FloorToInt(FPS * LODLevel[L].percentFPS * 0.01f);
        period = 1 / (float)LODfps;
    }

    public void GetRenderers()
    {
        //Find at all the children (all children will have a transform, so that's what we'll look for)
        childrenTrans = this.GetComponentsInChildren<Transform>();

        //Now, for each child, check that it has a renderer, and it's something that we want the sprite camera to see.
        //If so, add it to the meshRenderers list. We will use that list to calculate the bounds of all the meshes together
        // by using the GetBounds method. 
        foreach (Transform childTrans in childrenTrans)
        {
            if ((childTrans.GetComponent<Renderer>() != null) && (childTrans.gameObject.layer == LayerMask.NameToLayer("3D Model")))
            {
            meshRenderers.Add(childTrans.GetComponent<Renderer>());

            //boxColliders.Add(childTrans.GetComponent<BoxCollider>());
                childrenGO.Add(childTrans.gameObject);
            }

            

        }

        //GameObject GOtransform = this.transform.Find("UMARenderer").gameObject;

        //childrenGO.Add(GOtransform);

        //Renderer umaRend = this.transform.Find("UMARenderer").GetComponent<Renderer>();

        //meshRenderers.Add(umaRend);

    }

    public void GetBounds()
    {
        if (boundsType == BoundsType.Renderers)
        {
            GetBoundsRenderers();
        }
        else if (boundsType == BoundsType.StaticBox)
        {
            GetBoundsStaticBox();
        }
        else if (boundsType == BoundsType.Colliders)
        {
            GetBoundsColliders();
        }

    }

    public void GetBoundsRenderers()
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
            encapBounds = subjectBounds;

            //Grow the bounds to include all renderers in children.
            foreach (Renderer mR in meshRenderers)
            {
                encapBounds.Encapsulate(mR.bounds);
            }

            //Set the encapsulated bounds to be a bit larger than needed to ensure full capture.
            objSize = encapBounds.size * 1.25f;
            objCenter = encapBounds.center;
            subjectCenter = subjectBounds.center;

        }

    }

    public void GetBoundsStaticBox()
    {
        subjectBounds = staticBoxBounds.bounds;
        objSize = subjectBounds.size;
        objCenter = subjectBounds.center;
    }

    public void GetBoundsColliders()
    {

        subjectBounds = boxColliders[boxColliders.Count - 1].bounds;

        //If you have only one box collider, simply get the bounds.
        if (boxColliders.Count == 1)
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
            foreach (BoxCollider mR in boxColliders)
            {
                encapBounds.Encapsulate(mR.bounds);
            }

            objSize = encapBounds.size * 1.125f;
            objCenter = encapBounds.center;
            subjectCenter = subjectBounds.center;

        }

        //CenterSubject();

    }

    //This method keeps the camera centered on the subject.
    public void CenterSubject()
    {

        float verticalCameraOffset = (BasePlane.transform.position.y + (objSize.y * 0.5f));
        Vector3 tempSpriteCameraPivot = new Vector3(BasePlane.transform.position.x, verticalCameraOffset, BasePlane.transform.position.z);
        //Vector3 tempSpriteCameraPivot = new Vector3(spriteCameraPivot.transform.position.x, verticalCameraOffset, spriteCameraPivot.transform.position.z);
        spriteCameraPivot.transform.position = tempSpriteCameraPivot;
    }

    public void ToggleSpriterEnable()
    {

        if (spriterEnabled)
        {
            spriteCameraPivot.SetActive(true);
            spriteObj.SetActive(true);
        }

        else
        {
            foreach (GameObject childGO in childrenGO)
            {
                childGO.layer = LayerMask.NameToLayer("Default");
            }
            spriteCameraPivot.SetActive(false);
            spriteObj.SetActive(false);
        }
    }

    public void UpdateQualitySettings()
    {

        //Release the RenderTexture so we can reconfigure its descriptor
        // then re-initialize it.
        rT.Release();

        //View Angles

        int views = 8;

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

        int viewsVert = 1;

        if (viewAnglesVert == ViewAnglesVert.One)
        {
            viewsVert = 4;
        }
        else if (viewAnglesVert == ViewAnglesVert.Three)
        {
            viewsVert = 8;
        }
        else if (viewAnglesVert == ViewAnglesVert.Five)
        {
            viewsVert = 16;
        }

        //Resolution 
        if (spriteQuality == QualitySettings.VeryLow)
        {
            pixelsPerUnit = 25f;
        }
        else if (spriteQuality == QualitySettings.Low)
        {
            pixelsPerUnit = 50;
        }
        else if (spriteQuality == QualitySettings.Medium)
        {
            pixelsPerUnit = 100;
        }
        else if (spriteQuality == QualitySettings.High)
        {
            pixelsPerUnit = 200;
        }
        else if (spriteQuality == QualitySettings.VeryHigh)
        {
            pixelsPerUnit = 360;
        }

        //Color Depth (Low = 15bit, High = 32bit)
        if (colorDepth == ColorDepth.Low)
        {
            rTformat = RenderTextureFormat.ARGB1555;
        }
        else if (colorDepth == ColorDepth.High)
        {
            rTformat = RenderTextureFormat.ARGB32;
        }

        //Texture filter (Trilinear filtering would offer no benefit)
        if (filter)
        {
            sFilterMode = FilterMode.Bilinear;
        }
        else
        {
            sFilterMode = FilterMode.Point;
        }

        //Self Shadowing
        foreach (Renderer mR in meshRenderers)
        {
            if (selfShadowing)
            {
                mR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                mR.receiveShadows = true;
            }
            else
            {
                mR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mR.receiveShadows = false;
            }
        }


        angleSpan = 360f / views;
        angleSpanVert = 180 / viewsVert;
        period = 1 / (float)FPS;

        rendTexX = Mathf.FloorToInt(objSize.x * pixelsPerUnit);
        rendTexY = Mathf.FloorToInt(objSize.y * pixelsPerUnit);
        rendTexDesc = new RenderTextureDescriptor(rendTexX, rendTexY, rTformat, 16);

        rT.filterMode = sFilterMode;
        rT.descriptor = rendTexDesc;
        rT.Create();

        //UpdateView();
    }

    public void ToggleFrameClock()
    {
        frameClockOn = !frameClockOn;
    }

    private void OnTriggerStay(Collider other)
    {

        //int instanceID = this.GetInstanceID();
        //if(other.gameObject.name == "MobileNPC" && other.gameObject != this.transform.parent)

        //if (collided)
       // {
                if (other.gameObject.layer == this.gameObject.layer)
                {
                    Debug.Log("Collision from " + other.gameObject.name);
                    collGO = other.gameObject;

                    allowUpdate = false;
                }

       // }
    }


    public void OnDrawGizmos()
    {
        //Gizmos.DrawCube(encapBounds.center, new Vector3(subjectBounds.size.x, subjectBounds.size.y, subjectBounds.size.z));
        //Gizmos.DrawSphere(encapBounds.center, 0.125f);
    }

}


