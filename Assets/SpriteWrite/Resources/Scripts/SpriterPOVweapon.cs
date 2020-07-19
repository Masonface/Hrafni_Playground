using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriterPOVweapon : MonoBehaviour
{

    [SerializeField] public bool spriterEnabled = true;
    [Header("Quality Settings")]
    [Tooltip("Frames per second for the sprite animation. Higher: smoother animations. Lower: more discrete (jerky). Lowering this number will increase performance.")]
    [Range(2f, 12f)] [SerializeField] public int FPS = 6;

    private bool prevSpriterEnabledState;

    private int rendTexX = 200;
    private int rendTexY = 250;
    public GameObject Weapon;
    private Renderer weaponRenderer;

    public float period;
    public Slider UpdateResSlider;
    public Toggle SpriterToggle;
    public Toggle FilterToggle;
    public Toggle SelfShadowingToggle;
    public Dropdown ColorDepthDrop;
    public Dropdown PresetDrop;
    public Slider FPSslider;

    public GameObject weaponCanvas;
    private Renderer _renderer;

    private RenderTexture rT;
    private MaterialPropertyBlock _propBlock;
    //private MaterialPropertyBlock _propBlockTemp;
    private RenderTextureDescriptor rendTexDesc;
    public float frameClock;
    public Vector3 objSize;
    public float objHeight;
    public float cameraNearClip;
    private Camera spriteCamera;
    private Camera playerCamera;
    public float pixelWidth;
    public float pixelHeight;

    public float pixelsPerUnit;
    private RenderTextureFormat rTformat;
    private FilterMode sFilterMode;

    public List<Renderer> meshRenderers;
    Transform[] childrenTrans;
    public List<GameObject> childrenGO;

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

    [System.Serializable]
    public struct LOD
    {
        public float distanceAway;
        public float percentRes;
        public float percentFPS;
        //public bool filterOn;
    }

    [Tooltip("Sets the resolution of the sprite. VeryLow = Pixel Art, Low = Doom, Medium = Daggerfall")]
    public QualitySettings spriteQuality;
    [Tooltip("Low = 15bit, High = 32bit. Low is better for video memory usage and will better emulate older style games.")]
    public ColorDepth colorDepth;
    [Tooltip("Enables bilinear filtering. Makes subject look less jaggy, but more blurry. Similar look to Nintendo 64 sprites.")]
    [SerializeField] private bool filter;

    [Header("Lights and Shadow Settings")]
    [Tooltip("Renders shadows cast by the subject onto itself.")]
    [SerializeField] private bool selfShadowing;

    //public List<Renderer> meshRenderers;
    //Transform[] childrenTrans;
    //public List<GameObject> childrenGO;

    //public GameObject mesh;
    public Material spriteMaterial;

    public int enumValue;

    // Use this for initialization
    void Start()
    {


    }

    private void Awake()
    {

        spriteCamera = this.GetComponent<Camera>();
        playerCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        //Weapon = this.transform.Find("Weapon").gameObject;
        weaponRenderer = Weapon.GetComponent<Renderer>();

        rT = new RenderTexture(32, 32, 0);
        rT.useMipMap = false;
        rT.autoGenerateMips = false;

        _renderer = weaponCanvas.GetComponent<Renderer>();
        _propBlock = new MaterialPropertyBlock();
        _renderer.GetPropertyBlock(_propBlock); 
        _propBlock.SetTexture("_MainTex", rT);
        spriteCamera.targetTexture = rT;
        spriteMaterial.mainTexture = rT;

        frameClock = 0.0f;

        GetRenderers();
      
        UpdateQualitySettings();
        

    }

    // Update is called once per frame
    void Update()
    {

        //if (spriterEnabled)
        //{

            //Set the spriteCamera properties so its last frame's contents remain until the next update.
            spriteCamera.clearFlags = CameraClearFlags.Nothing;

            //Send the mesh to an invisible layer so it cannot be rendered when not needed.
            foreach (GameObject childGO in childrenGO)
            {
                childGO.gameObject.layer = LayerMask.NameToLayer("Invisible");
            }
        //Weapon.layer = LayerMask.NameToLayer("Invisible");

        //Update the view if the sprite's FPS is due to be updated.
        if (frameClock >= period)
            {
                UpdateView();
            }

            //Iterate the frameClock by the time the previous frame took.
            frameClock += Time.deltaTime;

        //}

        if (Input.GetKeyUp("g"))
        {
            updateWeaponCanvas();
        }

        //if (spriterEnabled != prevSpriterEnabledState)
        //{
        //    ToggleSpriterEnable();
        //}

        //prevSpriterEnabledState = spriterEnabled;

    }

    void UpdateView()
    {

        //Send the mesh (or meshes) to the 3D Model layer so it can be seen by the render camera.
        foreach (GameObject childGO in childrenGO)
        {
            childGO.gameObject.layer = LayerMask.NameToLayer("3D Weapon");
        }

        //Weapon.layer = LayerMask.NameToLayer("3D Weapon");

        //Change the spriteCamera's settings so that it will clear the buffer before writing the new frame.
        spriteCamera.clearFlags = CameraClearFlags.SolidColor;

        //Reset the frameClock.
        frameClock = 0.0f;

    }

    public void updateWeaponCanvas()
    {
        cameraNearClip = playerCamera.nearClipPlane + 0.01f;
        float playerFOV = playerCamera.fieldOfView * Mathf.Deg2Rad;
        objHeight = 2.25f * cameraNearClip * Mathf.Sin(playerFOV * 0.5f);
        objSize = new Vector3(playerCamera.aspect, 1f, 1f);
        weaponCanvas.transform.localPosition = new Vector3(0f, 0f, cameraNearClip);
        weaponCanvas.transform.localScale = objSize * objHeight;
        pixelWidth = playerCamera.pixelWidth;
        pixelHeight = playerCamera.pixelHeight;
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

    public void GetRenderers()
    {
        //Find at all the children (all children will have a transform, so that's what we'll look for)
        childrenTrans = this.GetComponentsInChildren<Transform>();

        //Now, for each child, check that it has a renderer, and it's something that we want the sprite camera to see.
        //If so, add it to the meshRenderers list. We will use that list to calculate the bounds of all the meshes together
        // by using the GetBounds method.
        foreach (Transform childTrans in childrenTrans)
        {
            if ((childTrans.GetComponent<Renderer>() != null) && (childTrans.gameObject.layer == LayerMask.NameToLayer("3D Weapon")))
            {
                meshRenderers.Add(childTrans.GetComponent<Renderer>());
                childrenGO.Add(childTrans.gameObject);
            }
        }

    }

    public void ToggleSpriterEnable()
    {

        spriterEnabled = SpriterToggle.isOn;

        if (spriterEnabled)
        {
            //Weapon.SetActive(true);
        }

        else
        {
            //foreach (GameObject childGO in childrenGO)
            //{
            //    childGO.layer = LayerMask.NameToLayer("Default");
            //}
            //Weapon.SetActive(false);
        }
    }

    public void UpdateQualitySettings()
    {

        //Release the RenderTexture so we can reconfigure its descriptor
        // then re-initialize it.
        rT.Release();


        //Resolution 
        if (spriteQuality == QualitySettings.VeryLow)
        {
            pixelsPerUnit = 25f;
        }
        else if (spriteQuality == QualitySettings.Low)
        {
            pixelsPerUnit = 45f;
        }
        else if (spriteQuality == QualitySettings.Medium)
        {
            pixelsPerUnit = 70f;
        }
        else if (spriteQuality == QualitySettings.High)
        {
            pixelsPerUnit = 100f;
        }
        else if (spriteQuality == QualitySettings.VeryHigh)
        {
            pixelsPerUnit = 240;
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

        if (selfShadowing)
        {
            Weapon.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            Weapon.GetComponent<Renderer>().receiveShadows = true;
        }

        else
        {
            Weapon.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Weapon.GetComponent<Renderer>().receiveShadows = false;
        }

        //foreach (Renderer mR in meshRenderers)
        //{
        //    if (selfShadowing)
        //    {
        //        mR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        //        mR.receiveShadows = true;
        //    }
        //    else
        //    {
        //        mR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        //        mR.receiveShadows = false;
        //    }
       // }
       
        period = 1 / (float)FPS;

        rendTexX = Mathf.FloorToInt(objSize.x * pixelsPerUnit);
        rendTexY = Mathf.FloorToInt(objSize.y * pixelsPerUnit);
        rendTexDesc = new RenderTextureDescriptor(rendTexX, rendTexY, rTformat, 16);

        rT.filterMode = sFilterMode;
        rT.descriptor = rendTexDesc;
        rT.Create();

        UpdateView();
        
    }

    public void UpdateResolution()
    {
        rT.Release();
        pixelsPerUnit = ((UpdateResSlider.value / UpdateResSlider.maxValue));

        cameraNearClip = playerCamera.nearClipPlane + 0.01f;
        float playerFOV = playerCamera.fieldOfView * Mathf.Deg2Rad;
        objHeight = 2.25f * cameraNearClip * Mathf.Sin(playerFOV * 0.5f);
        objSize = new Vector3(playerCamera.aspect, 1f, 1f);
        weaponCanvas.transform.localPosition = new Vector3(0f, 0f, cameraNearClip);
        weaponCanvas.transform.localScale = objSize * objHeight;
        pixelWidth = playerCamera.pixelWidth;
        pixelHeight = playerCamera.pixelHeight;

        rendTexX = Mathf.FloorToInt(pixelWidth * pixelsPerUnit);
        rendTexY = Mathf.FloorToInt(pixelHeight * pixelsPerUnit);
        rendTexDesc = new RenderTextureDescriptor(rendTexX, rendTexY, rTformat, 16);
        rT.descriptor = rendTexDesc;
        rT.Create();
    }

    public void UpdateFPS()
    {
        FPS = Mathf.RoundToInt(FPSslider.value);
        period = 1 / (float)FPS;
    }

    public void UpdateColorDepth()
    {
        rT.Release();

        int colorDepthEnum = ColorDepthDrop.value;

        if (colorDepthEnum == 0)
        {
            rTformat = RenderTextureFormat.ARGB1555;
        }

        else if (colorDepthEnum == 1)
        {
            rTformat = RenderTextureFormat.ARGB32;
        }

        rendTexDesc = new RenderTextureDescriptor(rendTexX, rendTexY, rTformat, 16);
        rT.descriptor = rendTexDesc;
        rT.Create();

    }

    public void UpdateFilter()
    {
        rT.Release();

        if (FilterToggle.isOn)
        {
            sFilterMode = FilterMode.Bilinear;
        }
        else
        {
            sFilterMode = FilterMode.Point;
        }

        rT.filterMode = sFilterMode;
        rT.descriptor = rendTexDesc;
        rT.Create();

    }

    public void UpdateSelfShadows()
    {
        if (selfShadowing)
        {
            weaponRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            weaponRenderer.receiveShadows = true;
        }

        else
        {
            weaponRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            weaponRenderer.receiveShadows = false;
        }

        //foreach (Renderer mR in meshRenderers)
        //{
        //    if (SelfShadowingToggle.isOn)
        //    {
        //        mR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        //        mR.receiveShadows = true;
        //    }
        //    else
        //    {
        //        mR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        //        mR.receiveShadows = false;
        //    }
        //}

    }

    public void UpdatePreset()
    {

        if (PresetDrop.value == 1)
        {
            UpdateResSlider.value = 30;
            FPSslider.value = 8;
            ColorDepthDrop.value = 0;
            FilterToggle.isOn = false;
            SelfShadowingToggle.isOn = false;
        }

        if (PresetDrop.value == 2)
        {
            UpdateResSlider.value = 42;
            FPSslider.value = 4;
            ColorDepthDrop.value = 0;
            FilterToggle.isOn = false;
            SelfShadowingToggle.isOn = false;
        }

        if (PresetDrop.value == 3)
        {
            UpdateResSlider.value = 65;
            FPSslider.value = 4;
            ColorDepthDrop.value = 0;
            FilterToggle.isOn = false;
            SelfShadowingToggle.isOn = false;
        }

        if (PresetDrop.value == 4)
        {
            UpdateResSlider.value = 80;
            FPSslider.value = 5;
            ColorDepthDrop.value = 0;
            FilterToggle.isOn = true;
            SelfShadowingToggle.isOn = false;
        }

        if (PresetDrop.value == 5)
        {
            UpdateResSlider.value = 150;
            FPSslider.value = 12;
            ColorDepthDrop.value = 1;
            FilterToggle.isOn = true;
            SelfShadowingToggle.isOn = true;
        }

        if (PresetDrop.value == 6)
        {
            UpdateResSlider.value = 240;
            FPSslider.value = 15;
            ColorDepthDrop.value = 1;
            FilterToggle.isOn = true;
            SelfShadowingToggle.isOn = true;
        }

    }

}


