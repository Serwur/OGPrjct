using DoubleMMPrjc.Utility;
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
            private AIAction actionToParent;
            /// <summary>
            /// Represents id of node, it mustn't be changed!
            /// </summary>
            [SerializeField] private long id;
            /// <summary>
            /// List of edges connecting other nodes
            /// </summary>
            [SerializeField] private List<Edge> edges;

            #region Unity API
            public void OnTriggerEnter(Collider other)
            {
                NPC npc = other.GetComponent<NPC>();
                if (npc) {
                    npc.OnNodeEnter( this );
                }
            }

            public void OnTriggerExit(Collider other)
            {
                NPC npc = other.GetComponent<NPC>();
                if (npc) {
                    npc.OnNodeExit( this );
                }
            }
            #endregion

            #region Public Methods
            /// <summary>
            /// Visits all nodes connected via <see cref="edges"/> and counting the current path costs for all of them. <br>
            /// It's used for A* algorithm.
            /// </summary>
            /// <param name="nodesList">Current stack of nodes to visit in A* algorithm</param>
            public void VisitNeighboors(List<Node> nodesList)
            {
                foreach (Edge edge in edges) {
                    if (!edge.Active || edge.Visited)
                        continue;
                    edge.Visited = true;
                    Node visitedNode = edge.GetAnother( this );
                    float pathCost = this.pathCost + edge.Distance;
                    float combinedCost = pathCost + visitedNode.heurestic;
                    if (combinedCost < visitedNode.combinedCost) {
                        visitedNode.combinedCost = combinedCost;
                        visitedNode.pathCost = pathCost;
                        visitedNode.parent = this;
                        if (visitedNode == edge.Start) {
                            visitedNode.actionToParent = edge.OnEndAction;
                        } else {
                            visitedNode.actionToParent = edge.OnStartAction;
                        }
                    }
                    if (!nodesList.Contains( visitedNode )) {
                        nodesList.Add( visitedNode );
                    }
                }
            }

            /// <summary>
            /// Calculate heurestic based of given <paramref name="node"/> position
            /// </summary>
            /// <param name="node">Other node to calculate heurestic</param>
            /// <returns>Heurestic value</returns>
            public float CalcHeurestic(Node node) => CalcHeurestic( node.transform.position );

            /// <summary>
            /// Calculate heurestic based of given <paramref name="position"/>
            /// </summary>
            /// <param name="position">Position to calculate heurestic</param>
            /// <returns>Heurestic value</returns>
            public float CalcHeurestic(Vector2 position)
            {
                heurestic = Vector2.Distance( transform.position, position );
                return heurestic;
            }

            /// <summary>
            /// Calculate distance for all edges contained in this node
            /// </summary>
            public void CalcEdgeDistances()
            {
                foreach (Edge edge in edges) {
                    edge.CalcDistance();
                }
            }

            /// <summary>
            /// Creates template edge that should be deleted after all action that must be done
            /// </summary>
            /// <param name="other">Node to create connection with</param>
            public void AddTemplateEdge(Node other)
            {
                Edge edge = new Edge( this, other, Direction.BOTH );
                edge.CalcDistance();
                edges.Add( edge );
                other.edges.Add( edge );
            }

            /// <summary>
            /// Adds an edge to node that connects another. It should be used for permament connections
            /// </summary>
            /// <param name="edge">Edge to add</param>
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

            /// <summary>
            /// Adds all edges to node from given list
            /// </summary>
            /// <param name="edges">List of edges</param>
            public void AddEdges(List<Edge> edges)
            {
                foreach (Edge edge in edges) {
                    AddEdge( edge );
                }
            }

            /// <summary>
            /// Removes given edge from node
            /// </summary>
            /// <param name="edge">Edge to remove</param>
            /// <returns><code>TRUE</code> if edge was removed, otherwise <code>FALSE</code></returns>
            public bool RemoveEdge(Edge edge)
            {
                return edges.Remove( edge );
            }

            /// <summary>
            /// Removes all connection with other nodes
            /// </summary>
            public void RemoveAllConnections()
            {
                foreach (Edge edge in edges) {
                    edge.GetAnother( this ).RemoveEdge( edge );
                }
                edges.Clear();
            }

            /// <summary>
            /// Resets node to init form
            /// </summary>
            public void Refresh()
            {
                heurestic = 0f;
                pathCost = float.MaxValue;
                combinedCost = float.MaxValue;
                visited = false;
                parent = null;
                //actionToParent = null;
                foreach (Edge edge in edges) {
                    edge.Refresh();
                }
            }

            /// <summary>
            /// Node in extended format
            /// </summary>
            /// <returns>name Pos( x position of node, y position of node)</returns>
            public string ToExtendedString()
            {
                float x = Math.Round( transform.position.x, 2 );
                float y = Math.Round( transform.position.y, 2 );
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

            public void OnDrawGizmos()
            {
                if (GameManager.DrawNodeConnections) {
                    foreach (Edge edge in edges) {
                        Gizmos.color = Color.green;
                        Gizmos.DrawLine( transform.position, edge.GetAnother( this ).transform.position );
                    }
                }
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
            public AIAction ActionToParent { get => actionToParent; set => actionToParent = value; }
            #endregion
        }
    }


}

