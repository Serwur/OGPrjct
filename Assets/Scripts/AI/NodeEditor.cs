using DoubleMMPrjc.Utility;
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
            public static readonly string CONNECTIONS_FOLDER = "NodeConnections";
            public static readonly string CONNECTION_DEFAULT_NAME = "Connection_";
            public static readonly string NODE_DEFAULT_NAME = "Node_";
            public static readonly string NODE_PREFAB_PATH = "Assets/Prefabs/AI/Node.prefab";

            private List<NodeConnection> connections = new List<NodeConnection>();
            private int selectedIndex = 0;
            private Vector2 nodeListScroll;
            private Vector2 connectionsListScroll;

            private NodeConnection selectedNodeConnection;
            private NodeConnection editedNodeConnection;

            private SceneNodeData data;

            [MenuItem( "Window/Node Connections Editor" )]
            public static void Init()
            {
                EditorWindow editor = GetWindow( typeof( NodeEditor ) );
                editor.titleContent = new GUIContent( "Node Connections Editor" );
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
                GUILayout.BeginHorizontal( GUILayout.ExpandWidth( true ) );    // 1 - START (H)
                GUILayout.BeginVertical( "Box", GUILayout.MaxWidth( 200 ), GUILayout.MaxHeight( 300 ), GUILayout.MinHeight( 300 ), GUILayout.ExpandWidth( false ) );    // 2 - START (V)
                GUILayout.BeginHorizontal();    // 3 - START (H)

                if (GUILayout.Button( "New", GUILayout.Width( 50 ) )) {
                    connections.Add( Create( SceneManager.GetActiveScene().name, data ) );
                }
                if (GUILayout.Button( "Edit", GUILayout.Width( 50 ) )) {
                    EditConnection( selectedNodeConnection );
                }
                if (GUILayout.Button( "Focus", GUILayout.Width( 50 ) )) {
                    FocusNodeOnScene( selectedNodeConnection );
                }
                if (GUILayout.Button( "Del", GUILayout.Width( 50 ) )) {
                    DeleteConnection( SceneManager.GetActiveScene().name, selectedNodeConnection );
                }
                GUILayout.EndHorizontal();  // 3 - END (H)

                ShowConnectionList();

                GUILayout.EndVertical();    // 2 - END (V)

                GUILayout.Space( 25 );

                if (editedNodeConnection != null) {
                    ShowEditLayout();
                } else {
                    EditorGUILayout.HelpBox( "Select node and click edit button to start editing node", MessageType.Info );
                }

                GUILayout.Space( 25 );

                if (GUI.changed) {
                    EditorUtility.SetDirty( data );
                }
                GUILayout.EndHorizontal();  // 1 - END (H)
            }

            private void ShowConnectionList()
            {
                if (connections.Count > 0) {
                    nodeListScroll = GUILayout.BeginScrollView( nodeListScroll, false, false, GUILayout.ExpandWidth( true ) );  // 1 - START (SV)
                    selectedIndex = GUILayout.SelectionGrid( selectedIndex, Collections.ToExtendedStringArray( connections.ToArray() ), 1, GUILayout.Width( 200 ), GUILayout.ExpandWidth( true ) );
                    SelectConnection( selectedIndex );
                    GUILayout.EndScrollView();  // 1 - END (SV)
                } else {
                    EditorGUILayout.HelpBox( "Create node and connection clicling on New button", MessageType.Warning );
                }
            }

            private void ShowEditLayout()
            {
                GUILayout.BeginVertical();  // 1 - START

                GUILayout.Label( "Edited node: " + editedNodeConnection.ToExtendedString(), EditorStyles.boldLabel, GUILayout.ExpandWidth( false ) );

                GUILayout.BeginHorizontal(); // 2 - START

                if (GUILayout.Button( "Focus", GUILayout.ExpandWidth( false ) )) {
                    FocusNodeOnScene( editedNodeConnection );
                }

                if (GUILayout.Button( "Delete", GUILayout.ExpandWidth( false ) )) {
                    DeleteConnection( SceneManager.GetActiveScene().name, editedNodeConnection, true );
                }

                if (GUILayout.Button( "Clear connections", GUILayout.ExpandWidth( false ) )) {
                    if (EditorUtility.DisplayDialog( "Are you sure to delete all connections?", "Ok", "Cancel" )) {
                        RemoveConnections( editedNodeConnection );
                    }
                }

                GUILayout.EndHorizontal(); // 2 - END

                if (editedNodeConnection != null) {
                    connectionsListScroll = GUILayout.BeginScrollView( connectionsListScroll, false, false,
                                                                    GUILayout.ExpandWidth( false ) ); // 2 - START
                    Edge toRemove = null;

                    // CREATING LIST OF EDGES CONNECTING WITH THIS NODE
                    foreach (Edge edge in editedNodeConnection.Edges) {
                        GUILayout.BeginHorizontal(); // 3 - START

                        GUILayout.Label( edge.ToString(), GUILayout.ExpandWidth( true ), GUILayout.Width( 140 ) );

                        GUILayout.Space( 5 );
                        edge.Direction = (Direction) EditorGUILayout.EnumPopup( edge.Direction, GUILayout.Width( 100 ) );

                        GUILayout.Space( 5 );
                        edge.Active = GUILayout.Toggle( edge.Active, "Active" );

                        GUILayout.Space( 5 );
                        if (GUILayout.Button( "Del", GUILayout.ExpandWidth( false ) )) {
                            toRemove = edge;
                        }

                        GUILayout.EndHorizontal(); // 3 - END
                        GUILayout.Space( 5 );
                    }

                    GUILayout.EndScrollView();  // 2 - END

                    bool disableButton = ( selectedNodeConnection == null ) || ( selectedNodeConnection == editedNodeConnection );
                    if (disableButton) {
                        EditorGUILayout.HelpBox( "Select another node to add", MessageType.Info );
                    } else if (editedNodeConnection.Contains( selectedNodeConnection.NodeId, editedNodeConnection.NodeId )) {
                        EditorGUILayout.HelpBox( "Edited node has already selected node, select another to add", MessageType.Info );
                        disableButton = true;
                    }

                    EditorGUI.BeginDisabledGroup( disableButton );

                    if (GUILayout.Button( "Add selected node" )) {
                        if (selectedNodeConnection != editedNodeConnection) {
                            try {
                                Edge _edge = new Edge( editedNodeConnection.NodeId, selectedNodeConnection.NodeId, Direction.BOTH );
                                editedNodeConnection.AddEdge( _edge );
                                selectedNodeConnection.AddEdge( _edge );
                                if (GUI.changed) {
                                    EditorUtility.SetDirty( selectedNodeConnection );
                                }
                            } catch (System.Exception e) {
                                Debug.LogError( e.Message );
                            }
                        }
                        editedNodeConnection.RemoveEdge( toRemove );
                    }

                    EditorGUI.EndDisabledGroup();

                    GUILayout.Space( 15 );
                    if (GUI.changed) {
                        EditorUtility.SetDirty( editedNodeConnection );
                    }
                }
                GUILayout.EndVertical();    // 1 - END
            }

            /// <summary>
            /// Selects asset from list
            /// </summary>
            /// <param name="index"></param>
            private void SelectConnection(int index)
            {
                if (index >= connections.Count)
                    selectedNodeConnection = null;
                else
                    selectedNodeConnection = connections[index];
            }

            public void ReloadNodeConnections(string sceneName)
            {
                this.connections.Clear();
                NodeConnection[] nodeConnections = LoadNodeConnnections( sceneName );
                if (nodeConnections != null) {
                    this.connections.AddRange( nodeConnections );
                }
            }

            public void EditConnection(NodeConnection connectionToEdit)
            {
                if (connectionToEdit != null) {
                    editedNodeConnection = connectionToEdit;
                    if (editedNodeConnection.Edges == null) {
                        editedNodeConnection.Edges = new List<Edge>();
                    }
                }
            }

            /// <summary>
            /// Focuses node on scene that belongs to asset object from list
            /// </summary>
            /// <param name="selectedConnection"></param>
            public void FocusNodeOnScene(NodeConnection selectedConnection)
            {
                if (selectedNodeConnection != null) {
                    Node node = GetNodeFromScene( selectedNodeConnection.NodeId );
                    if (node != null) {
                        Selection.activeGameObject = node.gameObject;
                    } else {
                        Debug.LogError( "Node with id: " + selectedConnection.NodeId + " dosn't exists on scene" );
                    }
                }
            }

            public void DeleteConnection(string sceneName, NodeConnection connection, bool confirmation = false)
            {
                bool delete = true;
                if (confirmation) {
                    delete = EditorUtility.DisplayDialog( "Node delete", "Are u sure to delete " +
                                                        connection.ToString() + "? It will remove also node " +
                                                        GetNodeFromScene( connection.NodeId ).ToExtendedString() + "?",
                                                        "Ok", "Cancel" );
                }
                if (delete) {
                    Node node = GetNodeFromScene( connection.NodeId );
                    if (node != null) {
                        DestroyImmediate( node.gameObject );
                    } else {
                        Debug.LogError( "Node with id: " + connection.NodeId + " dosn't exists on scene" );
                    }
                    connections.Remove( connection );
                    RemoveConnections( connection );
                    AssetDatabase.DeleteAsset( "Assets/Resources/" + CONNECTIONS_FOLDER + "/" + sceneName + "/" + connection.ToString() + ".asset" );
                    editedNodeConnection = null;
                }
            }

            public void RemoveConnections(NodeConnection connection)
            {
                foreach (Edge edge in connection.Edges) {
                    NodeConnection other = GetConnection( edge.GetAnotherId( connection.NodeId ) );
                    if (other != null) {
                        other.RemoveEdge( edge );
                    }
                }
                connection.Edges.Clear();
            }

            public NodeConnection GetConnection(long id)
            {
                foreach (NodeConnection connection in connections) {
                    if (connection.NodeId == id)
                        return connection;
                }
                return null;
            }

            public static NodeConnection[] LoadNodeConnnections(string sceneName)
            {
                string path = CONNECTIONS_FOLDER + "/" + sceneName;
                return Resources.LoadAll<NodeConnection>( path );
            }

            public static string GenerateName(string startName, long id)
            {
                return startName + id;
            }

            public static NodeConnection Create(string sceneName, SceneNodeData data)
            {
                NodeConnection asset = CreateInstance<NodeConnection>();

                // CREATE DATA AND FOLDER IF DOESN'T EXISTS
                if (data == null) {
                    data = CreateData( sceneName );
                }

                // GENERATES NEXT ID
                long id = data.GetNextId();

                // SETS NAME FOR NODE CONNECTION ASSET
                string assetName = GenerateName( CONNECTION_DEFAULT_NAME, id );
                AssetDatabase.CreateAsset( asset, "Assets/Resources/" + CONNECTIONS_FOLDER + "/" + sceneName + "/" + assetName + ".asset" );
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
                if (!AssetDatabase.IsValidFolder( "Assets/Resources/" + CONNECTIONS_FOLDER + "/" + sceneName )) {
                    AssetDatabase.CreateFolder( "Assets/Resources/" + CONNECTIONS_FOLDER, sceneName );
                }
                SceneNodeData data = CreateInstance<SceneNodeData>();
                AssetDatabase.CreateAsset( data, GetDataPath( sceneName ) );
                AssetDatabase.SaveAssets();
                return data;
            }

            public static string GetDataPath(string sceneName)
            {
                return "Assets/Resources/" + CONNECTIONS_FOLDER + "/" + sceneName + "/" + sceneName + "Data.asset";
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
