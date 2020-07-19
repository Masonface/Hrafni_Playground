using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpriteWriteManager))]
public class SpriteWriteInfo : Editor
{
    public Texture2D testImage;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Label(testImage);
        GUILayout.Space(15);
        GUILayout.Label("This script is needed only on a single   ");
        GUILayout.Label("gameobject in your scene in order to     ");
        GUILayout.Label("prevent all of your real-time sprite     ");
        GUILayout.Label("imposters from updating at the same time.");

    }

}
