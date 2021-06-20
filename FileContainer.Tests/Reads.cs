using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace FileContainer.Tests
{
    public class TestReads : TestBase
    {
        [Test]
        public void Read_Batch() =>
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
                    var getPages = randomBlocks.Take(3).ToArray();
                    var r        = store.Get(getPages.Select(p => p.Key).ToArray());

                    Assert.IsTrue(r.Keys.OrderBy(p => p).ToArray().SequenceEqual(getPages.OrderBy(p => p.Key).Select(p => p.Key).ToArray()));
                    foreach (var item in r)
                    {
                        Assert.NotNull(item.Value);
                        Assert.IsTrue(item.Value.SequenceEqual(r[item.Key]));
                    }
                }
            });

        [Test]
        public void Read_Single() =>
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
                    var getPages = randomBlocks.Take(3).ToArray();
                    var r        = new Dictionary<string, byte[]>();
                    foreach (var item in getPages)
                        r.Add(item.Key, store.Get(item.Key));

                    Assert.IsTrue(r.Keys.OrderBy(p => p).ToArray().SequenceEqual(getPages.OrderBy(p => p.Key).Select(p => p.Key).ToArray()));
                    foreach (var item in r)
                    {
                        Assert.NotNull(item.Value);
                        Assert.IsTrue(item.Value.SequenceEqual(r[item.Key]));
                    }
                }
            });

        [Test]
        public void Read_Batch_WithOffset()
        {
            // todo
        }

        [Test]
        public void Read_Single_WithOffset()
        {
            // todo
        }

        [Test]
        public void Read_Batch_WithOffsetLength()
        {
            // todo
        }

        [Test]
        public void Read_Single_WithOffsetLength()
        {
            // todo
        }
    }
}