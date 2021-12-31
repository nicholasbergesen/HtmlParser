namespace HtmlParser
{
    public class Node
    {
        public Node(string node, int depth, int openTagPosition)
        {
            OpenTagPosition = openTagPosition;
            Depth = depth;
            ClosedTagPosition = -1;
            Attributes = new Dictionary<string, string>();

            var firstSpacePos = node.IndexOf(' ');
            if (firstSpacePos == -1)
            {
                NodeType = Enum.Parse<NodeType>(node, true);
                return;
            }

            NodeType = Enum.Parse<NodeType>(node[0..firstSpacePos], true);
            var attributes = node[(firstSpacePos + 1)..^0];
            var attPos = 0;
            while (attPos < attributes.Length)
            {
                var firstSingleQuote = attributes.IndexOf('\'', attPos);
                var firstDoubleQuote = attributes.IndexOf('\"', attPos);
                var firstQuote = firstSingleQuote == -1 ? firstDoubleQuote : firstSingleQuote;
                if (firstSingleQuote != -1 && firstDoubleQuote != -1)
                    firstQuote = Math.Min(firstSingleQuote, firstDoubleQuote);

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

        public NodeType NodeType { get; set; }
        public string? Content { get; set; }
        public Dictionary<string, string> Attributes { get; set; }
        public int OpenTagPosition { get; set; }
        public int ClosedTagPosition { get; set; }
        public int Depth { get; set; }

        public override string ToString()
        {
            return $"{NodeType} {OpenTagPosition} {ClosedTagPosition}";
        }
    }
}
