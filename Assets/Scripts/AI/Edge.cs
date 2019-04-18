using System.Collections.Generic;
using UnityEngine;

namespace DoubleMMPrjc
{
    namespace AI
    {
        [System.Serializable]
        public class Edge
        {
            private float distance = 0f;
            private bool visited = false;
            [SerializeField] private Node start;
            [SerializeField] private Node end;
            [SerializeField] private long startId;
            [SerializeField] private long endId;
            [SerializeField] private Direction directionType;

            public Edge(long startId, long endId, Direction directionType)
            {
                this.startId = startId;
                this.endId = endId;
                this.directionType = directionType;
            }

            public Edge(Node start, Node end, Direction directionType)
            {
                this.start = start;
                this.end = end;
                this.directionType = directionType;
            }

            public float CalcDistance()
            {
                distance = Vector2.Distance( start.transform.position, end.transform.position );
                return distance;
            }

            public Node GetAnother(Node node)
            {
                if (node == start)
                    return end;
                if (node == end)
                    return start;
                throw new System.Exception( node.ToString() + " doesn't belong to this edge" );
            }

            public Node GetAnother(long id)
            {
                if (start.Id == id)
                    return end;
                if (end.Id == id)
                    return start;
                throw new System.Exception( "Node with id" + id + " doesn't belong to this edge" );
            }

            public void Refresh()
            {
                visited = false;
            }

            public override bool Equals(object obj)
            {
                var edge = obj as Edge;
                return startId == edge.startId && endId == edge.endId;
            }

            public override string ToString()
            {
                string direction = "";
                switch (Direction) {
                    case Direction.BOTH:
                        direction = "<-->";
                        break;
                    case Direction.TO_START:
                        direction = "<---";
                        break;
                    case Direction.TO_END:
                        direction = "--->";
                        break;
                }
                return "Connection: " + startId + direction + endId;
            }

            public override int GetHashCode()
            {
                var hashCode = 1665513263;
                hashCode = hashCode * -1521134295 + startId.GetHashCode();
                hashCode = hashCode * -1521134295 + endId.GetHashCode();
                hashCode = hashCode * -1521134295 + directionType.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<Node>.Default.GetHashCode( End );
                hashCode = hashCode * -1521134295 + Direction.GetHashCode();
                return hashCode;
            }

            public bool Visited { get => visited; set => visited = value; }
            public float Distance { get => distance; }
            public Node Start { get => start; set => start = value; }
            public Node End { get => end; set => end = value; }
            public Direction Direction{ get => directionType; set => directionType = value; }
            public long StartId { get => startId;  }
            public long EndId { get => endId; }
        }

        public enum Direction
        {
            BOTH, TO_START, TO_END
        }
    }
}