using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DoubleMMPrjc
{
    public class DialogueEditor : EditorWindow
    {
        public DialogueList dialogueList;
        public static readonly string DIALOGUES_FOLDER = "DialoguesLists";
        public static readonly string DEFAULT_NAME = "DialoguesList";

        private int viewIndex = -1;

        [MenuItem( "Window/Dialogue List" )]
        public static void Init()
        {
            EditorWindow editor = GetWindow( typeof( DialogueEditor ) );
            editor.minSize = new Vector2( 700, 300 );
            editor.maxSize = new Vector2( 700, 300 );
            editor.titleContent = new GUIContent( "Dialogue List Editor" );
            EditorStyles.textField.wordWrap = true;
        }


        // DO SPRAWDZENIA, JESZCZE NEI WIEM CO TO DOKŁADNIE ROBI
        public void OnEnable()
        {
            if (EditorPrefs.HasKey( "ObjectPath" )) {
                string objectPath = EditorPrefs.GetString( "ObjectPath" );
                dialogueList = AssetDatabase.LoadAssetAtPath( objectPath, typeof( DialogueList ) ) as DialogueList;
            }
        }

        public void OnGUI()
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
                    FocusList( this.dialogueList );
                }

                // TWORZYMY PRZYCISK
                if (GUILayout.Button( "Copy Dialogue List", GUILayout.ExpandWidth( false ) )) {
                    CopyDialogueList( dialogueList );
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space( 5 );

            if (dialogueList != null) {

                GUILayout.Label( "Current List: " + dialogueList, EditorStyles.boldLabel, GUILayout.ExpandWidth( false ) );
                GUILayout.BeginHorizontal();
                GUILayout.Label( "Dialogue List Type", GUILayout.ExpandWidth( false ) );
                dialogueList.type = (DialogueType) EditorGUILayout.EnumPopup( dialogueList.type, GUILayout.ExpandWidth( false ) );
                dialogueList.continueIfFails = EditorGUILayout.Toggle( "Continue If Dialogue Fails", dialogueList.continueIfFails );
                GUILayout.EndHorizontal();

                GUILayout.Space( 10 );

                GUILayout.BeginHorizontal();

                GUILayout.Space( 10 );
                if (GUILayout.Button( "Prev", GUILayout.ExpandWidth( false ) )) {
                    if (viewIndex > 1) {
                        EditorUtility.FocusProjectWindow();
                        viewIndex--;
                    }
                }

                GUILayout.Space( 5 );
                if (GUILayout.Button( "Next", GUILayout.ExpandWidth( false ) )) {
                    if (viewIndex < dialogueList.dialogues.Count) {
                        viewIndex++;
                        EditorUtility.FocusProjectWindow();
                    }
                }

                GUILayout.Space( 55 );

                if (GUILayout.Button( "Add Dialogue", GUILayout.ExpandWidth( false ) )) {
                    AddDialogue();
                }
                if (GUILayout.Button( "Delete Dialogue", GUILayout.ExpandWidth( false ) )) {
                    DeleteDialogue( viewIndex - 1 );
                }
                if (GUILayout.Button( "Copy Dialogue", GUILayout.ExpandWidth( false ) )) {
                    CopyDialogue( viewIndex - 1 );
                }

                GUILayout.EndHorizontal();

                if (dialogueList.dialogues.Count > 0) {
                    GUILayout.BeginHorizontal();
                    viewIndex = Mathf.Clamp( EditorGUILayout.IntField( "Current Dialogue", viewIndex, GUILayout.ExpandWidth( false ) ), 1, dialogueList.dialogues.Count );
                    EditorGUILayout.LabelField( "of   " + dialogueList.dialogues.Count.ToString() + "  dialogues", "", GUILayout.ExpandWidth( false ) );
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    dialogueList.dialogues[viewIndex - 1].dialogueName = EditorGUILayout.TextField( "Dialogue Name", dialogueList.dialogues[viewIndex - 1].dialogueName, GUILayout.ExpandWidth( true ) );
                    dialogueList.dialogues[viewIndex - 1].ownerName = EditorGUILayout.TextField( "Owner name", dialogueList.dialogues[viewIndex - 1].ownerName, GUILayout.ExpandWidth( true ) );
                    dialogueList.dialogues[viewIndex - 1].objectName = EditorGUILayout.TextField( "Object Name", dialogueList.dialogues[viewIndex - 1].objectName, GUILayout.ExpandWidth( true ) );
                    GUILayout.EndHorizontal();

                    GUILayout.Space( 5 );

                    EditorGUILayout.LabelField( "Dialogue text", EditorStyles.boldLabel, GUILayout.ExpandWidth( false ) );

                    GUILayout.BeginHorizontal();
                    dialogueList.dialogues[viewIndex - 1].dialogue = EditorGUILayout.TextArea( dialogueList.dialogues[viewIndex - 1].dialogue, GUILayout.Width( 250 ), GUILayout.Height( 80 ) );
                    GUILayout.BeginVertical();
                    dialogueList.dialogues[viewIndex - 1].speed = EditorGUILayout.Slider( "Dialogue Speed", dialogueList.dialogues[viewIndex - 1].speed, 0f, 1f );
                    dialogueList.dialogues[viewIndex - 1].audioClip = EditorGUILayout.ObjectField( "Dialogue Audio", dialogueList.dialogues[viewIndex - 1].audioClip, typeof( AudioClip ), false ) as AudioClip;
                    dialogueList.dialogues[viewIndex - 1].followObject = EditorGUILayout.Toggle( "Follow Target", dialogueList.dialogues[viewIndex - 1].followObject );
                    dialogueList.dialogues[viewIndex - 1].followSpeed = EditorGUILayout.Slider( "Follow Speed", dialogueList.dialogues[viewIndex - 1].followSpeed, 0.01f, 1f );
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

        /// <summary>
        /// Creates new dialogue list
        /// </summary>
        public void NewDialogueList()
        {
            viewIndex = 1;
            dialogueList = DialogueListCreator.Create();
            dialogueList.dialogues = new List<Dialogue>();
            string relPath = AssetDatabase.GetAssetPath( dialogueList );
            EditorPrefs.SetString( "ObjectPath", relPath );
            FocusList( dialogueList );
        }

        /// <summary>
        /// Copies dialogue list by given parameter
        /// </summary>
        /// <param name="listToCopy">Dialogue list that you want to copy</param>
        public void CopyDialogueList(DialogueList listToCopy)
        {
            viewIndex = 1;
            dialogueList = DialogueListCreator.Copy( listToCopy );
            string relPath = AssetDatabase.GetAssetPath( dialogueList );
            EditorPrefs.SetString( "ObjectPath", relPath );
            FocusList( dialogueList );
        }

        /// <summary>
        /// Opens existing dialogue list. It opens selected object in Project window or creates file manager window to select the object.
        /// </summary>
        public void OpenDialogueList()
        {
            if (Selection.activeObject != null &&
                Selection.activeObject.GetType() == typeof( DialogueList )) {
                dialogueList = (DialogueList) Selection.activeObject;
            } else {
                // Otwieramy panel z wyborem plików
                string absPath = EditorUtility.OpenFilePanel( "Select Dialogue List", "Assets/Resources/" + DIALOGUES_FOLDER, "asset" );
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

        public void FocusList(DialogueList list)
        {
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = list;
        }

        public void AddDialogue()
        {
            Dialogue dialogue = new Dialogue {
                dialogueName = "New Dialogue"
            };
            dialogueList.dialogues.Add( dialogue );
            viewIndex = dialogueList.dialogues.Count;
        }

        public void DeleteDialogue(int index)
        {
            dialogueList.dialogues.RemoveAt( index );
        }

        public void CopyDialogue(int index)
        {
            dialogueList.dialogues.Add( dialogueList.dialogues[index].Clone() );
            viewIndex = dialogueList.dialogues.Count;
        }
    }

    public class DialogueListCreator
    {
        [MenuItem( "Assets/Create/Dialogue List" )]
        public static DialogueList Create()
        {
            DialogueList asset = ScriptableObject.CreateInstance<DialogueList>();
            string name = GenerateName( ToStringList( Resources.LoadAll( DialogueEditor.DIALOGUES_FOLDER, typeof( DialogueList ) ) ), DialogueEditor.DEFAULT_NAME );
            AssetDatabase.CreateAsset( asset, "Assets/Resources/" + DialogueEditor.DIALOGUES_FOLDER + "/" + name + ".asset" );
            AssetDatabase.SaveAssets();
            return asset;
        }

        public static DialogueList Copy(DialogueList dialogueList)
        {
            DialogueList asset = dialogueList.Clone();
            string originName = dialogueList.ToString();
            string name = GenerateName( ToStringList( Resources.LoadAll( DialogueEditor.DIALOGUES_FOLDER, typeof( DialogueList ) ) ), originName + "_copy" );
            AssetDatabase.CreateAsset( asset, "Assets/Resources/" + DialogueEditor.DIALOGUES_FOLDER + "/" + name + ".asset" );
            AssetDatabase.SaveAssets();
            return asset;
        }

        public static List<string> ToStringList(object[] list)
        {
            List<string> stringList = new List<string>();
            foreach (object o in list) {
                stringList.Add( o.ToString() );
            }
            return stringList;
        }

        public static string GenerateName(List<string> list, string startName, int startNumber = 0)
        {
            list.Sort( (s1, s2) => { return s1.Length - s2.Length; } );
            string name = startName;
            for (int i = 0; i < list.Count; i++) {
                if (list[i].Equals( name )) {
                    startNumber++;
                    list.RemoveAt( i );
                    i--;
                    name = startName + "(" + startNumber + ")";
                }
            }
            return name;
        }
    }
}