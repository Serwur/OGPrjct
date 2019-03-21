using UnityEditor;
using UnityEngine;
using Inputs;

public class MakeScriptableObject
{
    [MenuItem( "Assets/Create/Alternative Keyboard Setup" )]
    public static void CreateMyAsset()
    {
        KeyboardSettings asset = ScriptableObject.CreateInstance<KeyboardSettings>();

        AssetDatabase.CreateAsset( asset, "Assets/Data/KeyboardSetups/NewKeyboardSetup.asset" );
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }
}
