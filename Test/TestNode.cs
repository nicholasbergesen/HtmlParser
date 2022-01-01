using HtmlParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    [TestClass]
    internal class TestNode
    {
        [TestMethod]
        public void NodeCreation_Test()
        {
            var node = new Node("html class=\" p24_live\" lang=\'en\' xml:lang=\"en\" xmlns=\"http://www.w3.org/1999/xhtml\"", 0, 0);
            Assert.AreEqual(NodeType.html, node.NodeType);
            Assert.AreEqual(0, node.Depth);
            Assert.AreEqual(0, node.OpenTagPosition);
            Assert.AreEqual(-1, node.ClosedTagPosition);
            Assert.AreEqual(4, node.Attributes.Count);
            Assert.AreEqual(" p24_live", node.Attributes["class"]);
            Assert.AreEqual("en", node.Attributes["lang"]);
            Assert.AreEqual("en", node.Attributes["xml:lang"]);
            Assert.AreEqual("http://www.w3.org/1999/xhtml", node.Attributes["xmlns"]);
        }
    }
}
