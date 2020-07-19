using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AddSpriteWrite : Editor
{

    [MenuItem("GameObject/Effects/Add Sprite Write")]

    static void AddSpriteWriteComponent(MenuCommand menuCommand)
    {
        
        string cameraPivotName  = "SpriteCameraPivot";
        string spriteOriginName = "SpriteOrigin";
        string basePlaneName    = "BasePlane";
        

        //Store the selected GameObject to reference it.
        GameObject goSelected   = Selection.activeGameObject as GameObject;
        Selection.activeObject  = goSelected;

        //Check to make sure that at least one GameObject is selected, or throw an error.
        if(Selection.gameObjects.Length == 1) { 

            //Move the selected GameObject and each of its children with Renderers to the "3D Model" layer.
            goSelected.layer = LayerMask.NameToLayer("3D Model");

            //Check all the children under the selected GameObject.
            //Count the Renderers to make sure there is at least one!
            Transform[] childrenTrans;
            childrenTrans = goSelected.GetComponentsInChildren<Transform>();
            List<string> childrenNames = new List<string>();
            int numberOfRenderers = 0;

                foreach (Transform child in childrenTrans)
                {   
                    //Check for Renderers
                    if(child.gameObject.GetComponent<Renderer>() != null)
                    {
                        child.gameObject.layer = LayerMask.NameToLayer("3D Model");
                        numberOfRenderers++;
                    }

                childrenNames.Add(child.gameObject.name);

                }
            
            //If you don't have a renderer, then what are you wanting to look like a sprite?
            if (numberOfRenderers != 0)
            {
                EditorUtility.DisplayDialog("SpriteWrite", "Error: Make sure your selected GameObject has a renderer before adding SpriteWrite", "Okay");
            }

            Undo.SetCurrentGroupName("Undo Add SpriteWrite");

            //Reference the prefab resources that we will be pulling in.
            var spriteWriteCameraPrefab     = Resources.Load("Prefabs/SpriteCameraPivot") as GameObject;
            var spriteWritePrefab           = Resources.Load("Prefabs/SpriteOrigin") as GameObject;
            var spriteWriteBasePlanePrefab  = Resources.Load("Prefabs/BasePlane") as GameObject;

            //Check if these are already instanced. If so, then ignore.
            //Pull in these prefab instances from the Resources folder.
            //Rename the instances so we don't have to reference a "(Clone)"
            //Register undo functions.
            //Set these GameObjects as children of the selected GameObject.
            //Initiate their position, angles, and scales.
            if (!childrenNames.Contains(cameraPivotName))
            {
                GameObject cameraPivot = Instantiate(spriteWriteCameraPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                cameraPivot.name = cameraPivotName;
                Undo.SetTransformParent(cameraPivot.transform, goSelected.transform, "Set new parent");
                cameraPivot.transform.localPosition     = Vector3.zero;
                cameraPivot.transform.localEulerAngles  = Vector3.zero; 
                cameraPivot.transform.localScale        = Vector3.one;
                Undo.RegisterCreatedObjectUndo(cameraPivot, "Create " + spriteWriteCameraPrefab.name);
            }

            if (!childrenNames.Contains(spriteOriginName))
            {
                GameObject spriteOrigin = Instantiate(spriteWritePrefab, Vector3.zero, Quaternion.identity) as GameObject;
                spriteOrigin.name = spriteOriginName;
                Undo.SetTransformParent(spriteOrigin.transform, goSelected.transform, "Set new parent");
                spriteOrigin.transform.localPosition    = Vector3.zero;
                spriteOrigin.transform.localEulerAngles = Vector3.zero;
                spriteOrigin.transform.localScale       = Vector3.one;
                Undo.RegisterCreatedObjectUndo(spriteOrigin, "Create " + spriteWriteCameraPrefab.name);
            }

            if (!childrenNames.Contains(basePlaneName))
            {
                GameObject basePlane = Instantiate(spriteWriteBasePlanePrefab, Vector3.zero, Quaternion.identity) as GameObject;
                basePlane.name = basePlaneName;
                Undo.SetTransformParent(basePlane.transform, goSelected.transform, "Set new parent");
                basePlane.transform.localPosition = Vector3.zero;
                basePlane.transform.localEulerAngles = new Vector3(90, 0, 0);
                basePlane.transform.localScale = Vector3.one;
                Undo.RegisterCreatedObjectUndo(basePlane, "Create " + spriteWriteCameraPrefab.name);
            }

            //Add the SpriteWrite script to the selected GameObject.
            //But check first if it is already attached!
            if (goSelected.GetComponent<SpriteWrite>() == null)
            {
                Undo.AddComponent<SpriteWrite>(goSelected);
            }
            // Register the creation in the undo system
                
               
            
        }

        //Multi-Object editing is no good for this.
        else if(Selection.gameObjects.Length > 1)
        {
            EditorUtility.DisplayDialog("SpriteWrite", "Error: Make sure you select only a single GameObject before trying to add SpriteWrite.", "Okay");
        }

        //Need to have a GameObject selected before adding SpriteWrite.
        else
        {
            EditorUtility.DisplayDialog("SpriteWrite", "Error: Make sure you select a GameObject before trying to add SpriteWrite.", "Okay");
        }
    }
}
