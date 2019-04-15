using System.Collections.Generic;
using UnityEngine;

namespace DoubleMMPrjc
{
    namespace AI
    {
        public class Node : MonoBehaviour
        {
            public Neighboors[] staticNeighboors;

            private float heurestic = 0f;
            private float pathCost = float.MaxValue;
            private float combinedCost = float.MaxValue;
            private bool visited = false;
            private Node parent;

            private HashSet<Edge> edges = new HashSet<Edge>();

            public void Start()
            {
                foreach (Neighboors neighboor in staticNeighboors) {
                    AddEdge( neighboor.node, neighboor.twoDirectioned );
                }
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

            public void AddEdge(Node other, bool twoDirections)
            {
                /*foreach (Edge _edge in edges) {
                    if (( _edge.Start == this && _edge.End == other ) ||
                            ( _edge.Start == other && _edge.End == this )) {
                        throw new Exception( ToString() + " has already connection with " + other.ToString() );
                    }
                }*/
                Edge edge = new Edge( this, other, twoDirections );
                edges.Add( edge );
            }

            public void AddEdge(Edge edge)
            {/*
                if (!( edge.Start == this || edge.End == this ))
                    throw new Exception( "Edge already contains " + ToString() );
                if (edges.Contains( edge ))
                    throw new Exception( ToString() + " already contains similar edge" );*/
                edges.Add( edge );
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

            #region Getters And Setters
            public float Heuristic { get => heurestic; set => heurestic = value; }
            public float PathCost { get => pathCost; set => pathCost = value; }
            public float CombinedCost { get => combinedCost; set => combinedCost = value; }
            public bool Visited { get => visited; set => visited = value; }
            public Node Parent { get => parent; set => parent = value; }
            public IEnumerable<Edge> Edges { get => edges; }
            #endregion
        }
    }


}

