using UnityEngine;
using UnityEditor;

public class BulkRenameEditor : EditorWindow
{
    private string renamePrefix = "NewName";
    private int startNumber = 1;

    // Add a menu item to open the Bulk Rename window.
    [MenuItem("Tools/Bulk Rename")]
    public static void ShowWindow()
    {
        GetWindow<BulkRenameEditor>("Bulk Rename");
    }

    void OnGUI()
    {
        GUILayout.Label("Bulk Rename Selected GameObjects", EditorStyles.boldLabel);
        renamePrefix = EditorGUILayout.TextField("Name Prefix", renamePrefix);
        startNumber = EditorGUILayout.IntField("Starting Number", startNumber);

        if (GUILayout.Button("Rename Selected GameObjects"))
        {
            RenameSelected();
        }
    }

    void RenameSelected()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("Bulk Rename", "No GameObjects selected in the Hierarchy!", "OK");
            return;
        }

        // Record an undo step for all selected objects.
        Undo.RecordObjects(selectedObjects, "Bulk Rename GameObjects");

        for (int i = 0; i < selectedObjects.Length; i++)
        {
            selectedObjects[i].name = renamePrefix + (startNumber + i);
            // Mark the object as dirty so that changes are saved.
            EditorUtility.SetDirty(selectedObjects[i]);
        }
    }
}
