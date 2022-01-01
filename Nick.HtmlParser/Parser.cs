using System.Security.Cryptography.X509Certificates;

namespace HtmlParser
{
    public class Parser
    {
        private static readonly HashSet<NodeType> _voidTags = new()
        {
            NodeType.area,
            NodeType.baseTag,
            NodeType.br,
            NodeType.col,
            NodeType.command,
            NodeType.embed,
            NodeType.hr,
            NodeType.img,
            NodeType.input,
            NodeType.keygen,
            NodeType.link,
            NodeType.meta,
            NodeType.param,
            NodeType.source,
            NodeType.track,
            NodeType.wbr
        };

        private static readonly HashSet<NodeType> _skipTag = new()
        {
            NodeType.script,
            NodeType.style
        };

        public static IEnumerable<INode> Parse(string html, bool loadContent = false)
        {
            List<Node> nodes = new();
            int pos = 0;
            int depth = 0;
            while (pos < html.Length)
            {
                char cur = html[pos];
                if (cur == '<')
                {
                    //skip over comments in the document
                    if (html[pos + 1] == '!')
                    {
                        bool isDoctype = html[(pos + 2)..(pos + 9)].Equals("DOCTYPE", StringComparison.InvariantCultureIgnoreCase);

                        if (isDoctype)
                        {
                            pos += 9;//skip ahead doctype chars.
                            while (html[++pos] != '>') ;
                            pos++;
                        }
                        else //if not doctype then assume is comment
                        {
                            while (!(html[pos] == '-' && html[pos + 1] == '-' && html[pos + 2] == '>'))
                            {
                                pos++;
                            }
                            pos += 3;
                        }
                        continue;
                    }

                    bool isCloseTag = html[pos + 1] == '/';
                    var closeBracketPos = pos;
                    while (html[++closeBracketPos] != '>') ;
                    var isSelfClosing = html[closeBracketPos - 1] == '/';

                    var startNodeTextPos = pos + 1;
                    var endNodeTextPos = closeBracketPos;
                    if (isCloseTag)
                        startNodeTextPos = pos + 2;
                    if (isSelfClosing)
                        endNodeTextPos = closeBracketPos - 1;
                    var nodeText = html[startNodeTextPos..endNodeTextPos];

                    var node = new Node(nodeText, depth, pos);
                    var isSkipTag = _skipTag.Contains(node.NodeType);

                    if (isSelfClosing || _voidTags.Contains(node.NodeType))
                    {
                        node.SelfCloseNode(content: loadContent ? html[node.OpenTagPosition..(node.OpenTagPosition + nodeText.Length + 1)] : null);
                        nodes.Add(node);
                    }
                    else if (isSkipTag)
                    {
                        var closeTag = $"</{node.NodeType}>";
                        var closeTagPos = html.IndexOf(closeTag, pos);
                        if (closeTagPos == -1)
                            throw new Exception($"Unable to find close tag for {node.NodeType} at char position {pos}");
                        var closePos = closeTagPos + closeTag.Length;
                        node.CloseNode(closePosition: closePos, content: loadContent ? html[node.OpenTagPosition..(closePos + 1)] : null);
                        pos = closePos;
                    }
                    else if (isCloseTag)
                    {
                        depth--;
                        var unclosedTag = nodes.FirstOrDefault(x =>
                            x.NodeType == node.NodeType
                            && x.Depth == depth
                            && x.ClosedTagPosition == -1);

                        //if is null then its possible there are unclosed tags causing depth calculation to be incorrect.
                        //Solution: Check for unclosed tags in previous depth, and close them as self closed tags.
                        //depth-- for each unclosed tag. All tags after the unclosed tag up to the current position will need their depth value corrected.
                        //Note: Could be multiple unclosed tags.
                        if (unclosedTag is null)
                        {
                            var openTag = nodes
                                .OrderByDescending(x => x.OpenTagPosition)
                                .FirstOrDefault(x =>
                                    x.NodeType == node.NodeType &&
                                    x.ClosedTagPosition == -1);

                            //if no matching open tag found, then ignore rogue closing tag.
                            //TODO: log as document error.
                            if (openTag is null)
                            {
                                continue;
                            }

                            var unclosedChildren = nodes
                                .Where(x => openTag.Depth < x.Depth
                                    && openTag.OpenTagPosition < x.OpenTagPosition
                                    && x.ClosedTagPosition == -1);

                            var closedChildren = nodes
                                .Where(x => openTag.Depth < x.Depth
                                    && openTag.OpenTagPosition < x.OpenTagPosition
                                    && x.ClosedTagPosition < pos)
                                .ToList(); //call .ToList() to execute .Where before populating self closing in unclosedChildren enumerable.

                            //close all unclosed children
                            int depthCorrection = 0;
                            foreach (var child in unclosedChildren)
                            {
                                child.SelfCloseNode();
                                depthCorrection++;
                            }

                            //correct child depths by correction amount
                            depth -= depthCorrection;
                            foreach (var child in closedChildren)
                            {
                                child.Depth -= depthCorrection;
                            }

                            //attempt to refetch unclosed tag with updated depth
                            unclosedTag = nodes.FirstOrDefault(x =>
                                x.NodeType == node.NodeType
                                && x.Depth == depth
                                && x.ClosedTagPosition == -1);

                            if (unclosedTag is null)
                                throw new Exception($"Unable to parse document. Error occored parsing character at position {pos}. Possible issue with {openTag.NodeType} as char position {openTag.OpenTagPosition}");

                        }

                        unclosedTag.CloseNode(closePosition: closeBracketPos, content: loadContent ? html[unclosedTag.OpenTagPosition..(closeBracketPos + 1)] : null);
                    }
                    else
                    {
                        nodes.Add(node);
                        depth++;
                    }
                }

                pos++;
            }

            return nodes;
        }
    }
}
