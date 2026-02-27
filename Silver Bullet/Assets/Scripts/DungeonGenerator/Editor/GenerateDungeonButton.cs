using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CreateDungeon))]
public class GenerateDungeonButton : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CreateDungeon button = (CreateDungeon)target;
        if (GUILayout.Button("Create Dungeon"))
        {
            button.editorCreateDungeon();
        }
    }
}