using DoubleMMPrjc.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DoubleMMPrjc
{
    namespace AI
    {
        [Serializable]
        public class NodeConnection : ScriptableObject, IExtendedString
        {
            [SerializeField] private long nodeId;
            [SerializeField] private List<Edge> edges;

            public void AddEdge(Edge edge)
            {
                if (Contains( edge )) {
                    throw new Exception( "Cannot add edge with same nodes twice" );
                }
                edges.Add( edge );
            }

            public bool RemoveEdge(Edge edge)
            {
                return edges.Remove( edge );
            }

            public bool Contains(Edge edge)
            {
                return edges.Contains( edge );
            }

            public bool Contains(long id1, long id2)
            {
                foreach (Edge edge in edges) {
                    if (edge.EqualsId( id1, id2 )) {
                        return true;
                    }
                }
                return false;
            }

            public override string ToString()
            {
                string basic = base.ToString();
                return basic.Substring( 0, basic.LastIndexOf( '(' ) - 1 );
            }

            public string ToExtendedString()
            {
                return "id: " + nodeId + ", edges: " + edges.Count;
            }

            public long NodeId { get => nodeId; set => nodeId = value; }
            public List<Edge> Edges { get => edges; set => edges = value; }


        }
    }
}


