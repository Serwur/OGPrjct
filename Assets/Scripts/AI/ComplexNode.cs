namespace DoubleMMPrjc
{
    namespace AI
    {
        public class ComplexNode
        {
            private Node node;
            private AIAction action;

            public ComplexNode(Node node, AIAction action)
            {
                this.node = node;
                this.action = action;
            }

            public Node Node { get => node; }
            public AIAction Action { get => action; }
        }

    }
}