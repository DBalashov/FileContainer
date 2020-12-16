using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace FileContainer.Tests
{
    public class TestFinds : TestBase
    {
        [Test]
        public void Find_All() =>
            DoIt(factory =>
                {
                    Dictionary<string, byte[]> randomBlocks;
                    using (var store = factory())
                    {
                        randomBlocks = getRandomBlocks(store.PageSize);
                        store.Put(randomBlocks);
                    }

                    using (var store = factory())
                    {
                        var r = store.Find();
                        Assert.IsNotNull(r);
                        Assert.IsTrue(r.Select(p => p.Name).Distinct(StringComparer.InvariantCultureIgnoreCase).Count() == randomBlocks.Count);

                        var dt = DateTime.UtcNow;
                        foreach (var item in r)
                        {
                            Assert.NotNull(item);
                            Assert.NotNull(item.Name);
                            Assert.IsTrue(item.FirstPage > 0);
                            Assert.IsTrue(item.Length > 0);
                            Assert.IsTrue(randomBlocks.ContainsKey(item.Name));
                            Assert.IsTrue(item.Length == randomBlocks[item.Name].Length);
                            Assert.IsTrue(item.Modified <= dt);
                        }
                    }
                }
            );
        
        [Test]
        public void Find_Single() =>
            DoIt(factory =>
                {
                    Dictionary<string, byte[]> randomBlocks;
                    using (var store = factory())
                    {
                        randomBlocks = getRandomBlocks(store.PageSize);
                        store.Put(randomBlocks);
                    }

                    using (var store = factory())
                    {
                        var r = store.Find("dir/*");
                        Assert.IsNotNull(r);
                        Assert.IsTrue(r.Select(p => p.Name).Distinct(StringComparer.InvariantCultureIgnoreCase).Count() == randomBlocks.Count);

                        var dt = DateTime.UtcNow;
                        foreach (var item in r)
                        {
                            Assert.NotNull(item);
                            Assert.NotNull(item.Name);
                            Assert.IsTrue(item.FirstPage > 0);
                            Assert.IsTrue(item.Length > 0);
                            Assert.IsTrue(randomBlocks.ContainsKey(item.Name));
                            Assert.IsTrue(item.Length == randomBlocks[item.Name].Length);
                            Assert.IsTrue(item.Modified <= dt);
                        }
                    }
                }
            );

        [Test]
        public void Find_Multi() =>
            DoIt(factory =>
            {
                Dictionary<string, byte[]> randomBlocks;
                using (var store = factory())
                {
                    randomBlocks = getRandomBlocks(store.PageSize);
                    store.Put(randomBlocks);
                }

                using (var store = factory())
                {
                    var r = store.Find("dir/*", "dir/file*");
                    Assert.IsNotNull(r);
                    Assert.IsTrue(r.Select(p => p.Name).Distinct(StringComparer.InvariantCultureIgnoreCase).Count() == randomBlocks.Count);

                    var dt = DateTime.UtcNow;
                    foreach (var item in r)
                    {
                        Assert.NotNull(item);
                        Assert.NotNull(item.Name);
                        Assert.IsTrue(item.FirstPage > 0);
                        Assert.IsTrue(item.Length > 0);
                        Assert.IsTrue(randomBlocks.ContainsKey(item.Name));
                        Assert.IsTrue(item.Length == randomBlocks[item.Name].Length);
                        Assert.IsTrue(item.Modified <= dt);
                    }
                }
            });

        [Test]
        public void Find_NonExisting() =>
            DoIt(factory =>
                {
                    using (var store = factory())
                    {
                        store.Put(getRandomBlocks(store.PageSize));
                    }

                    using (var store = factory())
                    {
                        var r = store.Find("zz");
                        Assert.IsNotNull(r);
                        Assert.IsEmpty(r);
                    }
                }
            );
    }
}