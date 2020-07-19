using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class CreateTextureAtlas : MonoBehaviour
{

    public string directory = "Assets/Textures/Test";
    public string filename;

    public int width;
    public int height;
    private TextureFormat texArrayFormat;
    public Texture2D textureAtlas;
    public Texture2D[] texturesToLoad;

    // Start is called before the first frame update
    void Start()
    {
        texArrayFormat = TextureFormat.ARGB32;
        width = 0;
        height = 0;

        createTextureAtlas();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void createTextureAtlas()
    {
        

        for (int i = 0; i < texturesToLoad.Length; i++)
        {

            width += texturesToLoad[i].width;

            if(texturesToLoad[i].height >= height)
            {
                height = texturesToLoad[i].height;
            }

        }

        textureAtlas = new Texture2D(width, height);
        AssetDatabase.CreateAsset(textureAtlas, directory + "/" + filename + "_Atlas" + ".asset");
        
        

        for (int j = 0; j < texturesToLoad.Length; j++)
        {

            //texturesToLoad[j].GetPixels();
            textureAtlas.SetPixels(texturesToLoad[j].width * j, 0, texturesToLoad[j].width, texturesToLoad[j].height, texturesToLoad[j].GetPixels());

        }

        textureAtlas.Apply();

        
    }
}

