using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
class bl_WelcomeEditor
{

    static readonly string key = "mfp.welcome.setup";

    static bl_WelcomeEditor()
    {
        if (PlayerPrefs.GetInt(key, 0) == 0)
        {
            EditorApplication.update += Update;
        }
    }

    static void Update()
    {

        EditorApplication.update -= Update;
        // Open tag manager
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        // Adding a Tag
        string s = "NetworkPlayer";

        // First check if it is not already present
        bool found = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(s)) { found = true; break; }
        }

        // if not found, add it
        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty n = tagsProp.GetArrayElementAtIndex(0);
            n.stringValue = s;
        }
        tagManager.ApplyModifiedProperties();
        PlayerPrefs.SetInt(key, 1);
        Debug.Log("Initial MFP setup ready!");
    }
}