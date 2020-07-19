using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AddSpriteWriteManager : Editor
{

    [MenuItem("GameObject/Effects/Add Sprite Write Manager")]

    static void AddSpriteWriteComponent(MenuCommand menuCommand)
    {

        if(GameObject.FindObjectOfType<SpriteWriteManager>() == null) {

            string[] layers = new string[] { "3D Model", "2D Model", "Invisible" };

            foreach(string layerName in layers)
            {
                CreateLayers.CreateLayer(layerName);
            }
        
            var spriteWriteManagerPrefab    = Resources.Load("Prefabs/SpriteWriteManager") as GameObject;
            GameObject spriteWriteManager   = Instantiate(spriteWriteManagerPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            spriteWriteManager.name         = "SpriteWriteManager";

        }

        else
        {
            EditorUtility.DisplayDialog("SpriteWrite", "Error: You already have a Sprite Write Manager in your scene. You only need one! :D", "Okay");
        }
    }
}
