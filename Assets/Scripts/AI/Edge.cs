using System.Collections.Generic;
using UnityEngine;

namespace DoubleMMPrjc
{
    namespace AI
    {
        public class Edge
        {
            private float distance = 0f;
            private bool visited = false;
            private Node start;
            private Node end;

            public Edge(Node start, Node end, bool twoDirections = true)
            {
                this.start = start;
                this.end = end;
                distance = Vector2.Distance( start.transform.position, end.transform.position );
                if (twoDirections) {
                    end.AddEdge( this );
                }
            }

            public Node GetAnother(Node node)
            {
                if (node == start)
                    return end;
                else if (node == end)
                    return start;
                else
                    throw new System.Exception( node.ToString() + " doesn't belong to this edge" );
            }


            public void Refresh()
            {
                visited = false;
            }

            public override bool Equals(object obj)
            {
                return obj is Edge edge &&
                         distance == edge.distance &&
                        EqualityComparer<Node>.Default.Equals( start, edge.start ) &&
                        EqualityComparer<Node>.Default.Equals( end, edge.end );
            }

            public override int GetHashCode()
            {
                var hashCode = -318876701;
                hashCode = hashCode * -1521134295 + distance.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<Node>.Default.GetHashCode( start );
                hashCode = hashCode * -1521134295 + EqualityComparer<Node>.Default.GetHashCode( end );
                return hashCode;
            }

            public bool Visited { get => visited; set => visited = value; }
            public Node Start { get => start; }
            public Node End { get => end; }
            public float Distance { get => distance; }
        }
    }
}