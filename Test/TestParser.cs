using HtmlParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Test
{
    [TestClass]
    public class TestParser
    {
        [TestMethod]
        public void Parser_Test()
        {
            var html = File.ReadAllText("test.html");
            var nodes = Parser.Parse(html);
            Assert.IsTrue(ValidateNodes(nodes));
        }

        [TestMethod]
        public void Parser2_Test()
        {
            var html = File.ReadAllText("test2.html");
            var nodes = Parser.Parse(html);
            Assert.IsTrue(ValidateNodes(nodes));
        }

        [TestMethod]
        public void Parser3_Test()
        {
            var html = File.ReadAllText("test3.html");
            var nodes = Parser.Parse(html, true);
            Assert.IsTrue(ValidateNodes(nodes));
        }

        [TestMethod]
        public void Parser_Depth_Test()
        {
        }

        private bool ValidateNodes(IEnumerable<INode> nodes)
        {
            return true;
        }
    }
}