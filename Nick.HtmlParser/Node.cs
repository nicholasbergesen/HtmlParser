namespace HtmlParser
{
    public interface INode
    {
        public NodeType NodeType { get; }
        public string? Content { get; }
        public Dictionary<string, string> Attributes { get; }
        public int OpenTagPosition { get; }
        public int ClosedTagPosition { get; }
        public int Depth { get; }
    }

    public class Node : INode
    {
        internal Node(string node, int depth, int openTagPosition)
        {
            _nodeLength = node.Length;
            OpenTagPosition = openTagPosition;
            Depth = depth;
            ClosedTagPosition = -1;
            Attributes = new Dictionary<string, string>();

            var firstSpacePos = node.IndexOf(' ');
            if (firstSpacePos == -1)
            {
                NodeType = Enum.Parse<NodeType>(node, ignoreCase: true);
                return;
            }

            NodeType = Enum.Parse<NodeType>(node[0..firstSpacePos], ignoreCase: true);
            var attributes = node[(firstSpacePos + 1)..^0];
            var attPos = 0;
            while (attPos < attributes.Length)
            {
                var firstSingleQuote = attributes.IndexOf('\'', attPos);
                var firstDoubleQuote = attributes.IndexOf('\"', attPos);

                //return if no quote is found.
                if (firstSingleQuote == -1 && firstDoubleQuote == -1)
                    break;

                var firstQuote = firstSingleQuote == -1 ? firstDoubleQuote : firstSingleQuote;
                //If both single and double quotes are found, use the one with the smallest index.
                if (firstSingleQuote != -1 && firstDoubleQuote != -1)
                    firstQuote = Math.Min(firstSingleQuote, firstDoubleQuote);

                //find the second quote based on the single/double value used for the firstQuote
                var secondQuote = -1;
                if (firstSingleQuote == firstQuote)
                    secondQuote = attributes.IndexOf('\'', firstQuote + 1);
                else
                    secondQuote = attributes.IndexOf('\"', firstQuote + 1);

                var name = attributes[attPos..(firstQuote - 1)];
                var value = attributes[(firstQuote + 1)..secondQuote];
                Attributes.Add(name, value);
                attPos = secondQuote + 2;
            }
        }

        private readonly int _nodeLength;

        internal void SelfCloseNode(string? content = null)
        {
            ClosedTagPosition = OpenTagPosition + _nodeLength;
            Content = content;
        }

        internal void CloseNode(int closePosition, string? content = null)
        {
            ClosedTagPosition = closePosition;
            Content = content;
        }

        public NodeType NodeType { get; internal set; }
        public string? Content { get; internal set; }
        public Dictionary<string, string> Attributes { get; internal set; }
        public int OpenTagPosition { get; internal set; }
        public int ClosedTagPosition { get; internal set; }
        public int Depth { get; internal set; }

        public override string ToString()
        {
            return $"{NodeType} {OpenTagPosition} {ClosedTagPosition}";
        }
    }
}
