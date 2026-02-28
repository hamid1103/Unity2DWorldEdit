using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileSelector))]
public class TileSelectorEditor : Editor
{
    void OnEnable()
    {
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Space(10);
        
    }
}
