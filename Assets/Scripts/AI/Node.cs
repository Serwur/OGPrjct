using DoubleMMPrjc.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace DoubleMMPrjc
{
    namespace AI
    {
        public class Node : MonoBehaviour, IExtendedString
        {
            private float heurestic = 0f;
            private float pathCost = float.MaxValue;
            private float combinedCost = float.MaxValue;
            private bool visited = false;
            private Node parent;
            [SerializeField] private long id;
            [SerializeField] private List<Edge> edges;

            #region Unity API
            public void Start()
            {
                AIManager.AddNode( this );
            }

            public void OnTriggerEnter(Collider other)
            {
                NPC npc = other.GetComponent<NPC>();
                if (npc != null) {
                    if (npc.CurrentNode == this) {
                        npc.NextNodeInPath();
                    }
                }
            }
            #endregion

            #region Public Methods
            public void VisitNeighboors(List<Node> currentStack)
            {
                foreach (Edge edge in edges) {
                    if (edge.Visited)
                        continue;
                    edge.Visited = true;
                    Node visitedNode = edge.GetAnother( this );
                    float pathCost = this.pathCost + edge.Distance;
                    float combinedCost = pathCost + visitedNode.heurestic;
                    if (combinedCost < visitedNode.combinedCost) {
                        visitedNode.combinedCost = combinedCost;
                        visitedNode.pathCost = pathCost;
                        visitedNode.parent = this;
                    }
                    if (!currentStack.Contains( visitedNode )) {
                        currentStack.Add( visitedNode );
                    }
                }
            }

            public float CalcHeurestic(Node node)
            {
                return CalcHeurestic( node.transform.position );
            }

            public float CalcHeurestic(Vector2 position)
            {
                heurestic = Vector2.Distance( transform.position, position );
                return heurestic;
            }

            public void CalcEdgeDistances()
            {
                foreach (Edge edge in edges) {
                    edge.CalcDistance();
                }
            }

            public void AddTemplateEdge(Node other)
            {
                Edge edge = new Edge( this, other, Direction.BOTH );
                edge.CalcDistance();
                edges.Add( edge );
                other.edges.Add( edge );
            }

            public void AddEdge(Edge edge)
            {
                switch (edge.Direction) {
                    case Direction.BOTH:
                        edges.Add( edge );
                        break;
                    case Direction.TO_START:
                        if (edge.EndId == id) {
                            edges.Add( edge );
                        }
                        break;
                    case Direction.TO_END:
                        if (edge.StartId == id) {
                            edges.Add( edge );
                        }
                        break;
                }
            }

            public void AddEdges(List<Edge> edges)
            {
                foreach (Edge edge in edges) {
                    AddEdge( edge );
                }
            }

            public bool RemoveEdge(Edge edge)
            {
                return edges.Remove( edge );
            }

            public void RemoveAllConnections()
            {
                foreach (Edge edge in edges) {
                    edge.GetAnother( this ).RemoveEdge( edge );
                }
                edges.Clear();
            }

            public void Refresh()
            {
                heurestic = 0f;
                pathCost = float.MaxValue;
                combinedCost = float.MaxValue;
                visited = false;
                parent = null;
                foreach (Edge edge in edges) {
                    edge.Refresh();
                }
            }

            public string ToExtendedString()
            {
                float x = Utility.Round( transform.position.x, 2 );
                float y = Utility.Round( transform.position.y, 2 );
                return gameObject.name + " Pos(" + x + " , " + y + ")";
            }

            public override string ToString()
            {
                string basic = base.ToString();
                return basic.Substring( 0, basic.LastIndexOf( '(' ) - 1 );
            }

            public override bool Equals(object obj)
            {
                var node = obj as Node;
                return node != null &&
                        base.Equals( obj );
            }

            public override int GetHashCode()
            {
                return 624022166 + base.GetHashCode();
            }
            #endregion

            #region Getters And Setters
            public float Heuristic { get => heurestic; set => heurestic = value; }
            public float PathCost { get => pathCost; set => pathCost = value; }
            public float CombinedCost { get => combinedCost; set => combinedCost = value; }
            public bool Visited { get => visited; set => visited = value; }
            public Node Parent { get => parent; set => parent = value; }
            public List<Edge> Edges { get => edges; set => edges = value; }
            public long Id { get => id; set => id = value; }
            #endregion
        }
    }


}

