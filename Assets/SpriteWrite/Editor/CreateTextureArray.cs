using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CreateTextureArray : MonoBehaviour
{

    public string directory = "/Textures/Test";
    public string filename;
    
    public int width;
    public int height;
    private TextureFormat texArrayFormat;
    public Texture2DArray textureArray;
    public Texture2D[] texturesToLoad;

    // Start is called before the first frame update
    void Start()
    {
        texArrayFormat = TextureFormat.ARGB32;
        createTextureArray();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void createTextureArray()
    {
        textureArray = new Texture2DArray(texturesToLoad[0].width, texturesToLoad[0].height, texturesToLoad.Length, texArrayFormat, true);
       
        for (int i = 0; i < texturesToLoad.Length; i++)
        {
            textureArray.SetPixels(texturesToLoad[i].GetPixels(0), i, 0);
           
        }

        textureArray.Apply();

        //Graphics.CopyTexture(texturesToLoad[0], textureArray);

        AssetDatabase.CreateAsset(textureArray, directory + "/" + filename + ".asset");
    }

}
