using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SudokuAutoTest;

namespace SudokuTestCase
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestPanel()
        {
            SudokuTester tester = new SudokuTester("");
        }
    }
}
