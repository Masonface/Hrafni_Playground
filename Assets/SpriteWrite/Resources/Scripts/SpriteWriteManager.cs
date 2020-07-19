using System.Collections.Generic;
using UnityEngine;
using System;
//using DaggerfallWorkshop.Game.Utility.ModSupport;


//[ImportedComponent]
public class SpriteWriteManager : MonoBehaviour
{

    public Camera mainCamera;
    public Queue<GameObject> updateQueue = new Queue<GameObject>();
    public List<int> objectPool;

    #region Culling Mask Stuff
    //For ease of use to the end user, the layer mask here, Awake, and toggleLayerMask()
    // will automatically disable the correct culling masks on the main camera for Sprite to
    // work without messing up their pre-selected camera culling masks.
    //In order to do this, we need to do arithmetic on a 32bit integer in binary format.
    private int layerMask;
    private string layerMaskString;
    private int layerMaskIndex;
    private int layerMaskValue;
    private float changeLayers;
    private int initialCullingMaskLength;
    private int initialCullingMask;
    private string initialCullingMaskString;
    #endregion

    //Singeton Stuff:
    private static SpriteWriteManager _instance;
    public static SpriteWriteManager Instance { get { return _instance; } }


    private void Awake()
    {
        #region Singleton Stuff
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            _instance.name = "SpriteWriteManager";
        }
        #endregion

        //To avoid null reference exceptions, if the user didn't specify a camera, use the MainCamera.
        if (mainCamera == null)
        {
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        }


        #region More Culling Mask Stuff
        //Store the original culling mask. We will modify this by adding or subtracting
        // values from it to enable/disable the three Sprite Write layers.
        initialCullingMask = mainCamera.cullingMask;
        //The culling mask int is much easier to work with as binary representation:
        initialCullingMaskString = Convert.ToString(initialCullingMask, 2);
        initialCullingMaskLength = initialCullingMaskString.Length;

        //Initiate our change to this culling mask to be zero.
        changeLayers = 0;

        //Pass the three Sprite Write culling masks into a function to get their values.
        //Add or subtract from the initial culling mask to toggle them on/off as needed.
        toggleLayerMask("3D Model");
        toggleLayerMask("2D Model");
        toggleLayerMask("Invisible");

        #endregion
    }

    void LateUpdate()
    {

        //Every GameObject with the Sprite Write script will ask this Singleton for permission to update its Render Texture.
        //The main idea is that no two GameObjects should be allowed to update at the exact same time.
        //This helps to smooth out performance and prevents two nearby Sprite Write objects from rendering into each other.
        //This method works well until the FPS gets too low, or the number of Sprite Write objects gets too high.
        //In which case, more updates per frame would need to be allowed.
        if (updateQueue.Count > 0)
        {
            int gameID = updateQueue.Peek().gameObject.GetInstanceID();
            Debug.Log("Updating gameobject ID# " + gameID);
            //updateQueue.
            //objectPool.Remove(updateQueue.Peek().gameObject.GetInstanceID());
            updateQueue.Peek().gameObject.GetComponent<SpriteWrite>().allowUpdate = true;
            updateQueue.Dequeue();
            
        }

    }

    public void AddToQueue(GameObject go)
    {
        //Each time a Sprite Write object needs to update its render texture, it will call this function
        // which will add it to a queue. Each frame, just one of these will be allowed to be updated.
        updateQueue.Enqueue(go);
        objectPool.Add(go.GetInstanceID());
    }

    public void toggleLayerMask(string layerName)
    {

        //Get an int representation of the culling mask in question.
        layerMask = LayerMask.GetMask(layerName);
        //Convert to a binary representation.
        layerMaskString = Convert.ToString(layerMask, 2);
        //Determine the location of that culling mask (index location) so we can then determine it's value (1 or 0).
        layerMaskIndex = (layerMaskString.Length - 1);

        //If the layer is the last one and its value is 0, then it will be culled by the string conversion. 
        //We can use an exception to catch that situation and set its value to zero. 
        //Otherwise check the actual value at that index position of the initiallCullingMaskString.
        try
        {
            layerMaskValue = System.Int32.Parse(initialCullingMaskString.Substring(initialCullingMaskLength - 1 - layerMaskIndex, 1));
        }
        catch (ArgumentOutOfRangeException outOfRange)
        {
            if (outOfRange == null)
            {
                layerMaskValue = System.Int32.Parse(initialCullingMaskString.Substring(initialCullingMaskLength - 1 - layerMaskIndex, 1));
            }

            else
            {
                layerMaskValue = 0;
            }
        }

        //Finally, we need for 2D Model layer to be on (1) and the other two to be off (0) regardless of what they were originally set to.
        //We will set the value of changeLayers to the int value necessary to enable/disable that culling mask once added to mainCamera.cullingMask.
        if (layerName == "2D Model")
        {
            if (layerMaskValue == 0)
            {
                changeLayers = Mathf.Pow(2, layerMaskIndex);
            }
            else
            {
                changeLayers = 0;
            }
        }
        else
        {
            if (layerMaskValue == 1)
            {
                changeLayers = Mathf.Pow(2, layerMaskIndex) * -1;
            }
            else
            {
                changeLayers = 0;
            }
        }

        int changeLayersInt = System.Convert.ToInt32(changeLayers);

        mainCamera.cullingMask += changeLayersInt;

    }

}
