using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class LazyDictionaryTest
{
    [TestMethod]
    public void LazyDictionaryIntListInt_LazyAssign_ListIntCreatedValueAssigned()
    {
        var lazyDictionary = new LazyDictionary<int, List<int>>();
        lazyDictionary[4].Add(5);
        Assert.IsTrue(lazyDictionary[4] != null);
        Assert.IsTrue(lazyDictionary[4].Contains(5));
    }

}
