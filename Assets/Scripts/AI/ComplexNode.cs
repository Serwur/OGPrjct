namespace ColdCry.AI
{
    /// <summary>
    /// Class is used for AI's path finding with information about <see cref="AIAction"/>
    /// that must be done when AI reaches specifed <see cref="Node"/>
    /// </summary>
    public class ComplexNode
    {
        public ComplexNode(Node node, AIAction action)
        {
            Node = node;
            Action = action;
        }

        public Node Node { get; }
        public AIAction Action { get; }
    }

}