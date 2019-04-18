﻿using DoubleMMPrjc.Utilities;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DoubleMMPrjc
{
    namespace AI
    {
        public class NodeEditor : EditorWindow
        {
            public static readonly string NEIGHBOORS_FOLDER = "NodeNeighborhood";
            public static readonly string NEIGHBORHOOD_DEFAULT_NAME = "Neighborhood_";
            public static readonly string NODE_DEFAULT_NAME = "Node_";
            public static readonly string NODE_PREFAB_PATH = "Assets/Prefabs/AI/Node.prefab";

            private List<NodeNeighborhood> nodeNeighborhoods = new List<NodeNeighborhood>();
            private int selectedIndex = 0;
            private Vector2 nodeListScroll;
            private Vector2 neihgborsListScroll;

            private NodeNeighborhood selectedNodeNeighborhood;
            private NodeNeighborhood editedNodeNeighborhood;

            private SceneNodeData data;

            [MenuItem( "Window/Neighborhood Editor" )]
            public static void Init()
            {
                EditorWindow editor = GetWindow( typeof( NodeEditor ) );
                editor.titleContent = new GUIContent( "Neighborhood Editor" );
                EditorStyles.textField.wordWrap = true;

                NodeEditor nodeEditor = editor as NodeEditor;

                string sceneName = SceneManager.GetActiveScene().name;
                nodeEditor.data = AssetDatabase.LoadAssetAtPath<SceneNodeData>( GetDataPath( sceneName ) );
                if (nodeEditor.data == null) {
                    nodeEditor.data = CreateData( sceneName );
                }
                nodeEditor.ReloadNodeConnections( sceneName );
            }

            public void OnGUI()
            {
                GUILayout.BeginHorizontal( GUILayout.ExpandWidth( true ) );    // 1 - START
                GUILayout.BeginVertical( "Box", GUILayout.MaxWidth( 200 ), GUILayout.MaxHeight( 300 ), GUILayout.MinHeight( 300 ), GUILayout.ExpandWidth( false ) );    // 2.1 - START
                GUILayout.BeginHorizontal();    // 3.1 - START
                if (GUILayout.Button( "New", GUILayout.Width( 50 ) )) {
                    nodeNeighborhoods.Add( Create( SceneManager.GetActiveScene().name, data ) );
                }
                if (GUILayout.Button( "Edit", GUILayout.Width( 50 ) )) {
                    EditNeighborhood( selectedNodeNeighborhood );
                }
                if (GUILayout.Button( "Focus", GUILayout.Width( 50 ) )) {
                    FocusNodeOnScene( selectedNodeNeighborhood );
                }
                if (GUILayout.Button( "Del", GUILayout.Width( 50 ) )) {
                    DeleteNeighboor( SceneManager.GetActiveScene().name, selectedNodeNeighborhood );
                }
                GUILayout.EndHorizontal();  // 3.1 - END

                if (nodeNeighborhoods.Count > 0) {
                    nodeListScroll = GUILayout.BeginScrollView( nodeListScroll, false, false, GUILayout.ExpandWidth( true ) );  // 3.2 - START
                    selectedIndex = GUILayout.SelectionGrid( selectedIndex, Utility.ToExtendedStringArray( nodeNeighborhoods.ToArray() ), 1, GUILayout.Width( 200 ), GUILayout.ExpandWidth( true ) );
                    GUILayout.EndScrollView();  // 3.2 - END
                } else {
                    EditorGUILayout.HelpBox( "Create node and connection clicling on New button", MessageType.Warning );
                }

                GUILayout.EndVertical();    // 2.1 - END
                GUILayout.Space( 25 );

                if (editedNodeNeighborhood != null) {
                    GUILayout.BeginVertical();  // 2.2 - START

                    GUILayout.Label( "Edited node: " + editedNodeNeighborhood.ToExtendedString(), EditorStyles.boldLabel, GUILayout.ExpandWidth( false ) );

                    neihgborsListScroll = GUILayout.BeginScrollView( neihgborsListScroll, false, false,
                                                                    GUILayout.ExpandWidth( false ) ); // 3 - START
                    Edge toRemove = null;

                    // CREATING LIST OF EDGES CONNECTING WITH THIS NODE
                    foreach (Edge edge in editedNodeNeighborhood.Edges) {
                        GUILayout.BeginHorizontal(); // 4 - START

                        GUILayout.Label( edge.ToString(), GUILayout.ExpandWidth( true ), GUILayout.Width( 140 ) );

                        GUILayout.Space( 5 );
                        edge.Direction = (Direction) EditorGUILayout.EnumPopup( edge.Direction, GUILayout.Width( 100 ) );
                        GUILayout.Space( 5 );

                        if (GUILayout.Button( "Del", GUILayout.ExpandWidth( false ) )) {
                            toRemove = edge;
                        }

                        GUILayout.EndHorizontal(); // 4 - END
                        GUILayout.Space( 5 );
                    }
                    GUILayout.EndScrollView();  // 3 - END

                    if (GUILayout.Button( "Add selected node" )) {
                        if (selectedNodeNeighborhood != null) {
                            if (selectedNodeNeighborhood != editedNodeNeighborhood) {
                                try {
                                    Edge edge = new Edge( editedNodeNeighborhood.NodeId, selectedNodeNeighborhood.NodeId, Direction.BOTH );
                                    editedNodeNeighborhood.AddEdge( edge );
                                    selectedNodeNeighborhood.AddEdge( edge );
                                } catch (System.Exception e) {
                                    Debug.LogError( e.Message );
                                }
                            }
                        }
                        editedNodeNeighborhood.RemoveEdge( toRemove );

                        GUILayout.Space( 15 );
                        GUILayout.EndVertical();    // 2.2 - END
                    }
                    GUILayout.Space( 25 );
                    GUILayout.EndHorizontal();  // 1 - END               
                } else {
                    EditorGUILayout.HelpBox( "Select node and click edit button to start editing node", MessageType.Info );
                }
                SelectNeighborhood( selectedIndex );
            }

            public void ReloadNodeConnections(string sceneName)
            {
                this.nodeNeighborhoods.Clear();
                NodeNeighborhood[] nodeNeighborhoods = LoadNodeConnnections( sceneName );
                if (nodeNeighborhoods != null) {
                    this.nodeNeighborhoods.AddRange( nodeNeighborhoods );
                }
            }

            public void EditNeighborhood(NodeNeighborhood neighborhoodToEdit)
            {
                if (neighborhoodToEdit != null) {
                    editedNodeNeighborhood = neighborhoodToEdit;
                    if (editedNodeNeighborhood.Edges == null) {
                        editedNodeNeighborhood.Edges = new List<Edge>();
                    }
                }
            }

            /// <summary>
            /// Selects asset from list
            /// </summary>
            /// <param name="index"></param>
            public void SelectNeighborhood(int index)
            {
                if (index >= nodeNeighborhoods.Count)
                    selectedNodeNeighborhood = null;
                else
                    selectedNodeNeighborhood = nodeNeighborhoods[index];
            }

            /// <summary>
            /// Focuses node on scene that belongs to asset object from list
            /// </summary>
            /// <param name="selectedNeighborhood"></param>
            public void FocusNodeOnScene(NodeNeighborhood selectedNeighborhood)
            {
                if (selectedNodeNeighborhood != null) {
                    Node node = GetNodeFromScene( selectedNodeNeighborhood.NodeId );
                    if (node != null) {
                        Selection.activeGameObject = node.gameObject;
                    } else {
                        Debug.LogError( "Node with id: " + selectedNeighborhood.NodeId + " dosn't exists on scene" );
                    }
                }
            }

            public void DeleteNeighboor(string sceneName, NodeNeighborhood neighborhood)
            {
                Node node = GetNodeFromScene( neighborhood.NodeId );
                if (node != null) {
                    DestroyImmediate( node.gameObject );
                } else {
                    Debug.LogError( "Node with id: " + neighborhood.NodeId + " dosn't exists on scene" );
                }
                nodeNeighborhoods.Remove( neighborhood );
                AssetDatabase.DeleteAsset( "Assets/Resources/" + NEIGHBOORS_FOLDER + "/" + sceneName + "/" + neighborhood.ToString() + ".asset" );
            }

            public static NodeNeighborhood[] LoadNodeConnnections(string sceneName)
            {
                string path = NEIGHBOORS_FOLDER + "/" + sceneName;
                return Resources.LoadAll<NodeNeighborhood>( path );
            }

            public static string GenerateName(string startName, long id)
            {
                return startName  + id;
            }

            public static NodeNeighborhood Create(string sceneName, SceneNodeData data)
            {
                NodeNeighborhood asset = CreateInstance<NodeNeighborhood>();

                // CREATE DATA AND FOLDER IF DOESN'T EXISTS
                if (data == null) {
                    data = CreateData( sceneName );
                }

                // GENERATES NEXT ID
                long id = data.GetNextId();

                // SETS NAME FOR NODE NEIGHBORHOOD ASSET
                string assetName = GenerateName( NEIGHBORHOOD_DEFAULT_NAME, id );
                AssetDatabase.CreateAsset( asset, "Assets/Resources/" + NEIGHBOORS_FOLDER + "/" + sceneName + "/" + assetName + ".asset" );
                AssetDatabase.SaveAssets();
                asset.NodeId = id;
                asset.Edges = new List<Edge>();

                Node node = Instantiate( AssetDatabase.LoadAssetAtPath<Node>( NODE_PREFAB_PATH ), Vector3.zero, Quaternion.identity );
                node.gameObject.name = GenerateName( NODE_DEFAULT_NAME, id );
                node.Id = asset.NodeId;

                // CREATE EMPTY OBJECT NAMED AI PATHS IF DOESN'T EXISTS
                // ALL NODES ALL BELONGS TO HIM, BEACUSE OF CLEAR
                // HIERARCHY WINDOW
                GameObject aiPaths = GameObject.Find( "AIPaths" );
                if (aiPaths == null) {
                    aiPaths = Instantiate( new GameObject(), Vector3.zero, Quaternion.identity );
                    aiPaths.name = "AIPaths";
                }

                node.transform.parent = aiPaths.transform;
                return asset;
            }

            public static SceneNodeData CreateData(string sceneName)
            {
                if (!AssetDatabase.IsValidFolder( "Assets/Resources/" + NEIGHBOORS_FOLDER + "/" + sceneName )) {
                    AssetDatabase.CreateFolder( "Assets/Resources/" + NEIGHBOORS_FOLDER, sceneName );
                }
                SceneNodeData data = CreateInstance<SceneNodeData>();
                AssetDatabase.CreateAsset( data, GetDataPath( sceneName ) );
                AssetDatabase.SaveAssets();
                return data;
            }

            public static string GetDataPath(string sceneName)
            {
                return "Assets/Resources/" + NEIGHBOORS_FOLDER + "/" + sceneName + "/" + sceneName + "Data.asset";
            }

            public static Node GetNodeFromScene(long id)
            {
                Node[] nodes = FindObjectsOfType<Node>();
                foreach (Node node in nodes) {
                    if (node.Id == id)
                        return node;
                }
                return null;
            }

        }
    }
}
