using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DoubleMMPrjc
{
    namespace AI
    {
        public class AIManager : MonoBehaviour
        {
            public Node prefabNode;

            private static AIManager Instance;

            private LinkedList<Enemy> chasingEnemies = new LinkedList<Enemy>();
            private Dictionary<long, Node> staticNodes = new Dictionary<long, Node>();
            private LinkedList<ContactArea> contactAreas = new LinkedList<ContactArea>();

            public void Awake()
            {
                if (Instance != null) {
                    throw new System.Exception( "Cannot create more than one AIManager object!" );
                }
                Instance = this;
            }

            public void Start()
            {
                NodeConnection[] nodeNeighborhoods = NodeEditor.LoadNodeConnnections( SceneManager.GetActiveScene().name );
                foreach (NodeConnection nodeNeighborhood in nodeNeighborhoods) {

                    foreach (Edge edge in nodeNeighborhood.Edges) {
                        edge.Start = GameObject.Find( NodeEditor.NODE_DEFAULT_NAME + edge.StartId ).GetComponent<Node>();
                        edge.End = GameObject.Find( NodeEditor.NODE_DEFAULT_NAME + edge.EndId ).GetComponent<Node>();
                        edge.CalcDistance();
                    }

                    if (staticNodes.TryGetValue( nodeNeighborhood.NodeId, out Node node )) {
                        node.AddEdges( nodeNeighborhood.Edges );
                    }
                    Debug.Log( nodeNeighborhood.NodeId );
                }
            }

            public static Stack<Node> FindPath(Entity ai, Entity target)
            {
                // SPRAWDZAMY CZY ZNALEZLISMY JAKIEKOLWIEK NODE'Y
                List<Node> aiNodes = GetContactNodes( ai );
                if (aiNodes == null || aiNodes.Count == 0)
                    return null;
                List<Node> targetNodes = GetContactNodes( target );
                if (targetNodes == null || targetNodes.Count == 0)
                    return null;
                // W TAKIM RAZIE LECIMY DALEJ, TWORZYMY TERAZ TYMCZASOWE NODY NA POZYCJACH
                // ENTITÓW PRZEKAZANYCH JAKO ARGUMENTY, ABY UTWORZYĆ EDGE Z NODE'AMI
                // ZNALEZIONYMI W POBLIŻU

                // ZNAJDUJEMY NAJBLIŻSZEGO NODE'A KOŃCOWI
                Node end = GetNearestNode( targetNodes, target.transform.position );
                // TWORZYMY POZORNEGO NODE'A
                Node start = Instantiate( Instance.prefabNode, ai.transform.position, ai.transform.rotation );
                // OBLICZAMY HEURESTYKE DLA PIERWSZEGO NODE'A
                start.CalcHeurestic( end );
                // DODAJEMY KRAWĘDZIE MIĘDZY NOWYM NODE'EM A NAJBLIŻSZYMI
                foreach (Node node in aiNodes) {
                    node.AddTemplateEdge( start );
                }
                // LICZYMY HEURESTYKE DLA WSZYSTKICH STATYCZNYCH NODE'ÓW
                foreach (Node node in Instance.staticNodes.Values) {
                    node.CalcHeurestic( end );
                }
                // ROZPOCZYNAMY ALGORYTM, JEŻELI ISTNIEJE ŚCIEŻKA DO GRACZA
                Stack<Node> path = null;
                if (HasPath( start, end )) {
                    // TWORZYMY LISTĘ NODÓW DO SPRAWDZENIA
                    List<Node> nodesToCheck = new List<Node>();
                    // USTAWIAMY KOSZTY DLA PIERWSZEGO NODE'A
                    start.CombinedCost = start.Heuristic;
                    start.PathCost = 0;
                    // PRZYPISUJEMY PIERWSZEGO NODE'A
                    Node next = start;
                    // ROBIMY ALGORYTM ODWIEDZAJĄCY SĄSIADÓW WYBRANYCH PRZEZ
                    // ALGORYTM NODÓW DO MOMENTU, AŻ NASTĘPNYM NODE'EM NIE
                    // BĘDZIE KOŃCOWY NODE
                    while (next != end) {
                        next.VisitNeighboors( nodesToCheck );
                        // WYBIERAMY NODE'A O NAJKRÓTSZEJ TRASIE I
                        // POWATARZAMY PRZESZUKIWANIE SĄSIADÓW
                        next = GetMinNode( nodesToCheck );
                    }
                    // TWORZYMY ZNALEZIONĄ ŚCIEŻKĘ
                    path = new Stack<Node>();
                    while (next != start) {
                        path.Push( next );
                        next = next.Parent;
                    }
                }
                // USUWAMY WSZYSTKIE POŁĄCZENIA WCZEŚNIEJ UTWORZONE
                // DLA POCZĄTKOWEGO NODE'A
                start.RemoveAllConnections();
                // RESETUJEMY WARTOŚCI STATYCZNYCH NODE'ÓW
                foreach (Node node in Instance.staticNodes.Values) {
                    node.Refresh();
                }
                // WYMARZONY WYNIK
                foreach (Node node in path) {
                    Debug.Log( node + " --->> " );
                }
                Destroy( start.gameObject );
                return path;
            }

            public static bool HasPath(Node start, Node end)
            {
                start.Visited = true;
                foreach (Edge edge in start.Edges) {
                    Node next = edge.GetAnother( start );
                    if (next.Visited)
                        continue;
                    if (next == end || HasPath( next, end ))
                        return true;
                }
                return false;
            }

            public static List<Node> GetContactNodes(Entity entity)
            {
                return GetContactNodes( entity.ContactArea );
            }

            public static List<Node> GetContactNodes(ContactArea area)
            {
                if (area == null)
                    return null;
                return area.Nodes;
            }

            public static void AddNode(Node node)
            {
                Instance.staticNodes.Add( node.Id, node );
            }

            public static void AddContactArea(ContactArea contactArea)
            {
                Instance.contactAreas.AddLast( contactArea );
            }

            public static Node GetNearestNode(ICollection<Node> nodes, Vector2 position)
            {
                Node nearestNode = null;
                float distance = float.MaxValue;
                foreach (Node node in nodes) {
                    float nodeDistance = Vector2.Distance( node.transform.position, position );
                    if (nodeDistance < distance) {
                        nearestNode = node;
                        distance = nodeDistance;
                    }
                }
                return nearestNode;
            }

            public override string ToString()
            {
                return "Node " + name;
            }

            private static Node GetMinNode(List<Node> nodes)
            {
                if (nodes.Count == 0)
                    return null;
                Node minNode = null;
                float minCombinedCost = float.MaxValue;
                int index = 0;
                int minIndex = 0;
                foreach (Node node in nodes) {
                    if (node.CombinedCost < minCombinedCost) {
                        minNode = node;
                        minCombinedCost = node.CombinedCost;
                        minIndex = index;
                    }
                    index++;
                }
                nodes.RemoveAt( minIndex );
                return minNode;
            }

            public static Enemy[] ChasingEnemies { get => Utilities.Utility.ToArray( Instance.chasingEnemies ); }

        }
    }
}


