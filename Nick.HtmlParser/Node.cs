namespace HtmlParser
{
    public interface INode
    {
        public string Name { get; }
        public NodeType Type { get; }
        public string? Content { get; }
        public Dictionary<string, string> Attributes { get; }
        public int OpenPosition { get; }
        public int ClosedPosition { get; }
        public int Depth { get; }
        public INode? Parent { get; }
        public IReadOnlyCollection<INode>? Children { get; }
    }

    public class Node : INode
    {
        internal Node(string tagName, int depth, int openTagPosition)
        {
            _nodeLength = tagName.Length;
            OpenPosition = openTagPosition;
            Depth = depth;
            ClosedPosition = -1;
            Attributes = new Dictionary<string, string>();
            var firstSeparatorPos = 0;
            while (++firstSeparatorPos < tagName.Length && !char.IsWhiteSpace(tagName, firstSeparatorPos)) ;
            Name = firstSeparatorPos == tagName.Length ? tagName : tagName[0..firstSeparatorPos];
            if (Enum.TryParse(Name, ignoreCase: true, out NodeType nodeType))
                Type = nodeType;
            else
                Type = NodeType.unknown;

            if (firstSeparatorPos == tagName.Length)
                return;

            var attributes = tagName[(firstSeparatorPos + 1)..^0];
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
                int secondQuote;
                if (firstSingleQuote == firstQuote)
                    secondQuote = attributes.IndexOf('\'', firstQuote + 1);
                else
                    secondQuote = attributes.IndexOf('\"', firstQuote + 1);

                //possibly malformed tag, fail node gracefully
                if (secondQuote < 0)
                {
                    break;
                }

                var name = attributes[attPos..(firstQuote - 1)];
                var value = attributes[(firstQuote + 1)..secondQuote];
                //TODO: Include in document errors when there are duplicates.
                Attributes.TryAdd(name, value);
                attPos = secondQuote + 2;
            }
        }

        private readonly int _nodeLength;

        internal void SelfCloseNode(string? content = null)
        {
            ClosedPosition = OpenPosition + _nodeLength;
            Content = content;
        }

        internal void CloseNode(int closePosition, IReadOnlyCollection<INode>? children = null, string? content = null)
        {
            Children = children;
            ClosedPosition = closePosition;
            Content = content;
        }

        public string Name { get; internal set; }
        public NodeType Type { get; internal set; }
        public string? Content { get; internal set; }
        public Dictionary<string, string> Attributes { get; internal set; }
        public int OpenPosition { get; internal set; }
        public int ClosedPosition { get; internal set; }
        public int Depth { get; internal set; }
        public INode? Parent { get; internal set; }
        public IReadOnlyCollection<INode>? Children { get; internal set; }

        public override string ToString()
        {
            return $"{Name} {OpenPosition} {ClosedPosition}";
        }
    }
}
