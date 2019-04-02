using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DialogueEditor : EditorWindow
{
    public DialogueList dialogueList;
    public static readonly string DIALOGUES_FOLDER = "DialoguesLists";
    public static readonly string DEFAULT_NAME = "DialoguesList";

    private int viewIndex = -1;

    [MenuItem( "Window/Dialogue List" )]
    static void Init()
    {
        EditorWindow editor = GetWindow( typeof( DialogueEditor ) );
        editor.minSize = new Vector2( 700, 300 );
        editor.maxSize = new Vector2( 700, 300 );
        editor.titleContent = new GUIContent( "Dialogue List Editor" );
    }


    // DO SPRAWDZENIA, JESZCZE NEI WIEM CO TO DOKŁADNIE ROBI
    void OnEnable()
    {
        if (EditorPrefs.HasKey( "ObjectPath" )) {
            string objectPath = EditorPrefs.GetString( "ObjectPath" );
            dialogueList = AssetDatabase.LoadAssetAtPath( objectPath, typeof( DialogueList ) ) as DialogueList;
        }
    }

    void OnGUI()
    {
        // ROZPOCZYNAMY USTAWIANIE POZIOMO
        GUILayout.Space( 10 );
        GUILayout.BeginHorizontal();
        // TWORZYMY PRZYCISK
        if (GUILayout.Button( "New Dialogue List", GUILayout.ExpandWidth( false ) )) {
            NewDialogueList();
        }
        if (GUILayout.Button( "Open Dialogue List", GUILayout.ExpandWidth( false ) )) {
            OpenDialogueList();
        }
        if (dialogueList != null) {
            // TWORZYMY PRZYCISK
            if (GUILayout.Button( "Focus In Assets", GUILayout.ExpandWidth( false ) )) {
                // FOCUSUJEMY OKNO
                EditorUtility.FocusProjectWindow();
                // ZAZNACZAMY OBIEKT JAKO WYBRANA LISTA?
                Selection.activeObject = dialogueList;
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space( 5 );

        if (dialogueList != null) {

            GUILayout.Label( "Current List: " + dialogueList, EditorStyles.boldLabel, GUILayout.ExpandWidth( false ) );
            GUILayout.BeginHorizontal();
            GUILayout.Label( "Dialogue List Type", GUILayout.ExpandWidth( false ) );
            dialogueList.type = (DialogueType) EditorGUILayout.EnumPopup( dialogueList.type, GUILayout.ExpandWidth( false ) );
            GUILayout.EndHorizontal();

            GUILayout.Space( 10 );

            GUILayout.BeginHorizontal();

            GUILayout.Space( 10 );
            if (GUILayout.Button( "Prev", GUILayout.ExpandWidth( false ) )) {
                if (viewIndex > 1)
                    viewIndex--;
            }

            GUILayout.Space( 5 );
            if (GUILayout.Button( "Next", GUILayout.ExpandWidth( false ) )) {
                if (viewIndex < dialogueList.dialogues.Count) {
                    viewIndex++;
                }
            }

            GUILayout.Space( 55 );

            if (GUILayout.Button( "Add Dialogue", GUILayout.ExpandWidth( false ) )) {
                AddDialogue();
            }
            if (GUILayout.Button( "Delete Dialogue", GUILayout.ExpandWidth( false ) )) {
                DeleteDialogue( viewIndex - 1 );
            }

            GUILayout.EndHorizontal();

            if (dialogueList.dialogues.Count > 0) {
                GUILayout.BeginHorizontal();
                viewIndex = Mathf.Clamp( EditorGUILayout.IntField( "Current Dialogue", viewIndex, GUILayout.ExpandWidth( false ) ), 1, dialogueList.dialogues.Count );
                EditorGUILayout.LabelField( "of   " + dialogueList.dialogues.Count.ToString() + "  dialogues", "", GUILayout.ExpandWidth( false ) );
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                dialogueList.dialogues[viewIndex - 1].dialogueName = EditorGUILayout.TextField( "Dialogue Name", dialogueList.dialogues[viewIndex - 1].dialogueName, GUILayout.ExpandWidth( true ) );
                dialogueList.dialogues[viewIndex - 1].ownerName = EditorGUILayout.TextField( "Owner name", dialogueList.dialogues[viewIndex - 1].ownerName );
                dialogueList.dialogues[viewIndex - 1].objectName = EditorGUILayout.TextField( "Object Name", dialogueList.dialogues[viewIndex - 1].objectName );
                GUILayout.EndHorizontal();

                GUILayout.Space( 5 );

                EditorGUILayout.LabelField( "Dialogue text", EditorStyles.boldLabel, GUILayout.ExpandWidth( false ) );

                GUILayout.BeginHorizontal();
                dialogueList.dialogues[viewIndex - 1].dialogue = EditorGUILayout.TextArea( dialogueList.dialogues[viewIndex - 1].dialogue, GUILayout.Width( 250 ), GUILayout.Height( 80 ) );
                GUILayout.BeginVertical();
                dialogueList.dialogues[viewIndex - 1].speed = EditorGUILayout.FloatField( "Dialogue Speed", dialogueList.dialogues[viewIndex - 1].speed, GUILayout.ExpandWidth( false ) );
                dialogueList.dialogues[viewIndex - 1].audioClip = EditorGUILayout.ObjectField( "Dialogue Audio", dialogueList.dialogues[viewIndex - 1].audioClip, typeof( AudioClip ), false ) as AudioClip;
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            } else {
                GUILayout.Label( "This Dialogue List is Empty." );
            }
            if (GUI.changed) {
                EditorUtility.SetDirty( dialogueList );
            }
        }
    }

    public void NewDialogueList()
    {
        // There is no overwrite protection here!
        // There is No "Are you sure you want to overwrite your existing object?" if it exists.
        // This should probably get a string from the user to create a new name and pass it ...
        viewIndex = 1;
        dialogueList = CreateDialogueList.Create();
        if (dialogueList != null) {
            dialogueList.dialogues = new List<Dialogue>();
            Selection.activeObject = dialogueList;
            string relPath = AssetDatabase.GetAssetPath( dialogueList );
            EditorPrefs.SetString( "ObjectPath", relPath );
        }
    }

    public void OpenDialogueList()
    {
        if (Selection.activeObject != null &&
            Selection.activeObject.GetType() == typeof( DialogueList )) {
            dialogueList = (DialogueList) Selection.activeObject;
        } else {
            // Otwieramy panel z wyborem plików
            string absPath = EditorUtility.OpenFilePanel( "Select Dialogue List", "Assets/Resources/" + DIALOGUES_FOLDER, ".asset" );
            if (absPath.Equals( "" ))
                return;
            // Jeżeli ścieżka zaczyna się od tego czegoś??
            if (absPath.StartsWith( Application.dataPath )) {
                // Tworzymy substringa ścieżki względnej
                string relPath = absPath.Substring( Application.dataPath.Length - "Assets".Length );
                // Wczytujemy wybranego asseta
                dialogueList = AssetDatabase.LoadAssetAtPath( relPath, typeof( DialogueList ) ) as DialogueList;
                if (dialogueList != null) {
                    // Jeżeli jego lista jest nullem to tworzymy nową
                    if (dialogueList.dialogues == null)
                        dialogueList.dialogues = new List<Dialogue>();
                    // Jeżeli to jest nullem?? To ustawiamy preferowaną ścieżke tego pliku?
                    EditorPrefs.SetString( "ObjectPath", relPath );
                }
            }
        }
    }

    void AddDialogue()
    {
        Dialogue dialogue = new Dialogue {
            dialogueName = "New Dialogue"
        };
        dialogueList.dialogues.Add( dialogue );
        viewIndex = dialogueList.dialogues.Count;
    }

    void DeleteDialogue(int index)
    {
        dialogueList.dialogues.RemoveAt( index );
    }
}

public class CreateDialogueList
{
    [MenuItem( "Assets/Create/Dialogue List" )]
    public static DialogueList Create()
    {
        DialogueList asset = ScriptableObject.CreateInstance<DialogueList>();
        object[] lists = Resources.LoadAll( DialogueEditor.DIALOGUES_FOLDER, typeof( DialogueList ) );
        int number = 0;
        foreach (object list in lists) {
            if (list.ToString().Contains( DialogueEditor.DEFAULT_NAME ))
                number++;
        }
        AssetDatabase.CreateAsset( asset, "Assets/Resources/" + DialogueEditor.DIALOGUES_FOLDER +
            "/DialoguesList" + ( number == 0 ? "" : number.ToString() ) + ".asset" );
        AssetDatabase.SaveAssets();
        return asset;
    }
}
