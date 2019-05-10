using DoubleMMPrjc.Utilities;
using System.Collections.Generic;

namespace DoubleMMPrjc
{
    namespace AI
    {

        public class AIPathList
        {

            private LinkedList<ComplexNode> cnList = new LinkedList<ComplexNode>();
            private LinkedList<ComplexNode> cnHistoryList = new LinkedList<ComplexNode>();
            private ComplexNode currentCn = null;

            /// <summary>
            /// Pushes given <see cref="ComplexNode"/> to begin of the list
            /// </summary>
            /// <param name="cn"><see cref="ComplexNode"/> to push to the list</param>
            /// <returns>Pushed <see cref="ComplexNode"/></returns>
            public ComplexNode Push(ComplexNode cn)
            {
                return cnList.AddFirst( cn ).Value;
            }

            /// <summary>
            /// Creates new <see cref="ComplexNode"/> from given <see cref="Node"/> and <see cref="AIAction"/> and pushes it to begin of the list
            /// </summary>
            /// <param name="node"><see cref="Node"/> to create and push new <see cref="ComplexNode"/></param>
            /// <param name="action"><see cref="AIAction"/> to create and push new <see cref="ComplexNode"/></param>
            /// <returns>Created and pushed <see cref="ComplexNode"/></returns>
            public ComplexNode Push(Node node, AIAction action)
            {
                return cnList.AddFirst( new ComplexNode( node, action ) ).Value;
            }

            /// <summary>
            /// Returns last ComplexNode to history and returns it
            /// </summary>
            /// <returns>Previous complex node or <b>null</b> if there is no history</returns>
            public ComplexNode Previous()
            {
                ComplexNode cn = null;
                if (cnHistoryList.Count > 0) {
                    cn = Utility.RemoveFirst( cnHistoryList );
                    cnList.AddFirst( cn );
                }
                currentCn = cn;
                return cn;
            }

            /// <summary>
            /// Pops the first <see cref="ComplexNode"/> from list and puts it to history
            /// </summary>
            /// <returns>Poped <see cref="ComplexNode"/>, returns <b>null</b> if list is empty</returns>
            public ComplexNode Next()
            {
                ComplexNode cn = null;
                if (cnList.Count > 0) {
                    cn = Utility.RemoveFirst( cnList );
                    cnHistoryList.AddFirst( cn );
                }
                currentCn = cn;
                return cn;
            }

            /// <summary>
            /// Checks if list contains given <see cref="ComplexNode"/> as 
            /// </summary>
            /// <param name="cn">ComplexNode to check</param>
            /// <returns><code>TRUE</code> if list contains given <see cref="ComplexNode"/>, otherwise <code>FALSE</code></returns>
            public bool Contains(ComplexNode cn)
            {
                return cnList.Contains( cn );
            }

            /// <summary>
            /// Checks if any of <see cref="ComplexNode"/> from history that contains given <see cref="Node"/>
            /// </summary>
            /// <param name="node"><see cref="Node"/> to check list</param>
            /// <returns><code>TRUE</code> if any <see cref="ComplexNode"/> from list contains given <see cref="Node"/>, otherwise <code>FALSE</code></returns>
            public bool Contains(Node node)
            {
                foreach (ComplexNode cn in cnList) {
                    if (cn.Node == node)
                        return true;
                }
                return false;
            }

            /// <summary>
            /// Checks if history contains given <see cref="ComplexNode"/>
            /// </summary>
            /// <param name="cn">ComplexNode to check</param>
            /// <returns><code>TRUE</code> if history contains given <see cref="ComplexNode"/>, otherwise <code>FALSE</code></returns>
            public bool ContainsHistory(ComplexNode cn)
            {
                return cnHistoryList.Contains( cn );
            }

            /// <summary>
            /// Checks if any of <see cref="ComplexNode"/> from history that contains given <see cref="Node"/>
            /// </summary>
            /// <param name="node"><see cref="Node"/> to check list</param>
            /// <returns><code>TRUE</code> if any <see cref="ComplexNode"/> from history contains given <see cref="Node"/>, otherwise <code>FALSE</code></returns>
            public bool ContainsHistory(Node node)
            {
                foreach (ComplexNode cn in cnHistoryList) {
                    if (cn.Node == node)
                        return true;
                }
                return false;
            }

            /// <summary>
            /// Checks if list or history contains given <see cref="ComplexNode"/>
            /// </summary>
            /// <param name="cn"><see cref="ComplexNode"/> to check</param>
            /// <returns><code>TRUE</code> if list or history contains given <see cref="ComplexNode"/>, otherwise <code>FALSE</code></returns>
            public bool ContainsAll(ComplexNode cn)
            {
                if (Contains( cn ) || ContainsHistory( cn ))
                    return true;
                return false;
            }

            /// <summary>
            /// Checks if any of <see cref="ComplexNode"/> from list or history contains given  <see cref="Node"/>
            /// </summary>
            /// <param name="node"><see cref="Node"/> to check</param>
            /// <returns><code>TRUE</code> if any <see cref="ComplexNode"/> from list or history contains given <see cref="Node"/>, otherwise <code>FALSE</code></returns>
            public bool ContainsAll(Node node)
            {
                if (Contains( node ) || ContainsHistory( node ))
                    return true;
                return false;
            }

            /// <summary>
            /// Clear list and history
            /// </summary>
            public void Clear()
            {
                cnList.Clear();
                cnHistoryList.Clear();
                currentCn = null;
            }

            /// <summary>
            /// Current path going througth all nodes
            /// </summary>
            /// <returns>Path</returns>
            public override string ToString()
            {
                string path = "";

                foreach (ComplexNode cn in cnList) {
                    path += cn.Node.ToString();
                    if (cn != cnList.Last.Value) {
                        path += " --> ";
                    }
                }

                return path;
            }

            /// <summary>
            /// <code>TRUE</code> if list is empty, otherwise <code>FALSE</code>
            /// </summary>
            public bool IsEmpty { get => cnList.Count == 0; }
            /// <summary>
            /// <code>TRUE</code> if history is empty, otherwise <code>FALSE</code>
            /// </summary>
            public bool IsHistoryEmpty { get => cnHistoryList.Count == 0; }
            /// <summary>
            /// <code>TRUE</code> if list and history are empty, otherwise <code>FALSE</code>
            /// </summary>
            public bool IsFullyEmpty { get => IsEmpty && IsHistoryEmpty; }
            /// <summary>
            /// Gives current ComplexNode
            /// </summary>
            public ComplexNode Current { get => currentCn; }
            /// <summary>
            /// Amount of nodes in list that left
            /// </summary>
            public int LeftNodes { get => cnList.Count; }
            /// <summary>
            /// Amount of nodes in history
            /// </summary>
            public int NodesInHistory { get => cnHistoryList.Count; }
        }

    }
}