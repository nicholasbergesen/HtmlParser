namespace HtmlParser
{
    public class Parser
    {
        private static readonly HashSet<string> _voidTags = new()
        {
            "area",
            "base",
            "br",
            "col",
            "command",
            "embed",
            "hr",
            "img",
            "input",
            "keygen",
            "link",
            "meta",
            "param",
            "source",
            "track",
            "wbr"
        };

        public static IEnumerable<Node> Parse(string html)
        {
            List<Node> nodes = new();
            int pos = 0;
            int depth = 0;
            while (pos < html.Length)
            {
                char cur = html[pos];
                //open tag
                if (cur == '<')
                {
                    //skip over comments in the document
                    if (html[pos + 1] == '!')
                    {
                        while (!(html[pos] == '-' && html[pos + 1] == '-' && html[pos + 2] == '>'))
                        {
                            pos++;
                        }
                        pos += 3;
                        continue;
                    }

                    bool isEndTag = html[pos + 1] == '/';
                    var limit = Math.Min(html.Length - pos, 1000);
                    var closingBracketPos = pos;
                    while (html[++closingBracketPos] != '>') ;
                    var isSelfClosing = html[closingBracketPos - 1] == '/';
                    var nodeText = isEndTag ? html[(pos + 2)..closingBracketPos] : html[(pos + 1)..closingBracketPos];
                    var node = new Node(nodeText, depth, pos);
                    isSelfClosing |= _voidTags.Contains(node.NodeType.ToString());

                    if (isSelfClosing)
                    {
                        node.ClosedTagPosition = pos + nodeText.Length;
                        nodes.Add(node);
                    }
                    else
                    {
                        if (isEndTag)
                        {
                            depth--;
                            var unclosedTag = nodes.First(x =>
                                x.NodeType == node.NodeType
                                && x.Depth == depth
                                && x.ClosedTagPosition == -1);

                            unclosedTag.ClosedTagPosition = closingBracketPos;
                        }
                        else
                        {
                            nodes.Add(node);
                            depth++;
                        }
                    }
                }

                pos++;
            }

            return nodes;
        }
    }
}
