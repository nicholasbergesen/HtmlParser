using System.Security.Cryptography.X509Certificates;

namespace HtmlParser
{
    //test https://www.facebook.com/news24
    public class Parser
    {
        private static readonly HashSet<NodeType> _voidTags = new()
        {
            NodeType.area,
            NodeType.@base,
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
            NodeType.script, //valid '<' characters inside script can throw off parsing.
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
                    else if (html[pos + 1] == '?')
                    {
                        while (!(html[++pos] == '?' && html[pos + 1] == '>')) ;
                        pos += 2;
                        continue;
                    }

                    bool isCloseTag = html[pos + 1] == '/';
                    var closeBracketPos = pos;
                    while (html[++closeBracketPos] != '>') ;
                    var isSelfClosing = html[closeBracketPos - 1] == '/';

                    var startNodeTextPos = pos + 1;
                    if (isCloseTag)
                        startNodeTextPos = pos + 2;
                    var nodeText = html[startNodeTextPos..(isSelfClosing ? closeBracketPos - 1 : closeBracketPos)];
                    var node = new Node(nodeText, depth, pos);
                    var isSkipTag = _skipTag.Contains(node.Type);

                    if (isSelfClosing || _voidTags.Contains(node.Type))
                    {
                        node.SelfCloseNode(content: loadContent ? html[node.OpenPosition..(closeBracketPos + 1)] : null);
                        nodes.Add(node);
                    }
                    else if (isSkipTag)
                    {
                        var openTag = $"<{node.Name}>";
                        var closeTag = $"</{node.Name}>";
                        int closeTagCounter = 0;
                        int closeTagPos = closeBracketPos;
                        //this look caters for nested skip tags. e.g <sciprt><script></script></script>
                        while(closeTagCounter < 1)
                        {
                            closeTagPos++;

                            if(closeTagPos > (html.Length - closeTag.Length))
                                throw new Exception($"Unable to find close tag for {node.Name} at char position {pos}");

                            if (html[closeTagPos..(openTag.Length + closeTagPos)] == openTag)
                                closeTagCounter--;
                            else if (html[closeTagPos..(closeTag.Length + closeTagPos)] == closeTag)
                                closeTagCounter++;
                        }

                        node.CloseNode(closePosition: closeTagPos + closeTag.Length, content: loadContent ? html[node.OpenPosition..(closeTagPos + closeTag.Length)] : null);
                        nodes.Add(node);
                        pos = closeTagPos;
                    }
                    else if (isCloseTag)
                    {
                        depth--;
                        var unclosedTag = nodes.FirstOrDefault(x =>
                            x.Name == node.Name
                            && x.Depth == depth
                            && x.ClosedPosition == -1);

                        //if is null then its possible there are unclosed tags causing depth calculation to be incorrect.
                        //Solution: Check for unclosed tags in previous depth, and close them as self closed tags.
                        //depth-- for each unclosed tag. All tags after the unclosed tag up to the current position will need their depth value corrected.
                        //Note: Could be multiple unclosed tags.
                        if (unclosedTag is null)
                        {
                            var openTag = nodes
                                .OrderByDescending(x => x.OpenPosition)
                                .FirstOrDefault(x =>
                                    x.Name == node.Name &&
                                    x.ClosedPosition == -1);

                            //if no matching open tag found, then ignore rogue closing tag.
                            //TODO: log as document error.
                            if (openTag is null)
                            {
                                pos++;
                                depth++;//restore depth
                                continue;
                            }

                            var unclosedChildren = nodes
                                .Where(x => openTag.Depth < x.Depth
                                    && openTag.OpenPosition < x.OpenPosition
                                    && x.ClosedPosition == -1);

                            var closedChildren = nodes
                                .Where(x => openTag.Depth < x.Depth
                                    && openTag.OpenPosition < x.OpenPosition
                                    && x.ClosedPosition < pos)
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
                                x.Name == node.Name
                                && x.Depth == depth
                                && x.ClosedPosition == -1);

                            if (unclosedTag is null)
                                throw new Exception($"Unable to parse document. Error occored parsing character at position {pos}. Possible issue with {openTag.Name} as char position {openTag.OpenPosition}");

                        }

                        unclosedTag.CloseNode(closePosition: closeBracketPos, content: loadContent ? html[unclosedTag.OpenPosition..(closeBracketPos + 1)] : null);
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
