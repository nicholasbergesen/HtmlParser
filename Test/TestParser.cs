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
            Assert.AreEqual(1403, nodes.Count);
        }

        [TestMethod]
        public void Parser2_Test()
        {
            var html = File.ReadAllText("test2.html");
            var nodes = Parser.Parse(html);
            Assert.AreEqual(811, nodes.Count);
        }

        [TestMethod]
        public void Parser3_Test()
        {
            var html = File.ReadAllText("test3.html");
            var nodes = Parser.Parse(html, true);
            Assert.AreEqual(1410, nodes.Count);
        }
    }
}