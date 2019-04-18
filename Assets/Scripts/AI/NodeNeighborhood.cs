using DoubleMMPrjc.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DoubleMMPrjc
{
    namespace AI
    {
        [Serializable]
        public class NodeNeighborhood : ScriptableObject, IExtendedString
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

            // TODO Zrobić to kurwa!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            public bool Contains(long id1, long id2)
            {
                foreach (Edge edge in edges) {

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


