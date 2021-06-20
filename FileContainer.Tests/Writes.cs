using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace FileContainer.Tests
{
    public class TestWrites : TestBase
    {
        [Test]
        public void Write_Batch() =>
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
                    foreach (var item in randomBlocks)
                    {
                        var value = store.Get(item.Key);
                        Assert.NotNull(value);
                        Assert.IsTrue(value.SequenceEqual(item.Value));
                    }
                }
            });

        [Test]
        public void Write_Single() =>
            DoIt(factory =>
            {
                Dictionary<string, byte[]> randomBlocks;
                using (var store = factory())
                {
                    randomBlocks = getRandomBlocks(store.PageSize);
                    foreach (var item in randomBlocks)
                        store.Put(item.Key, item.Value);
                }

                using (var store = factory())
                {
                    foreach (var item in randomBlocks)
                    {
                        var value = store.Get(item.Key);
                        Assert.NotNull(value);
                        Assert.IsTrue(value.SequenceEqual(item.Value));
                    }
                }
            });

        #region expand

        /// <summary> write N bytes with batch + write N*2 into same blocks </summary>
        [Test]
        public void Write_Batch_WithExpand() =>
            DoIt(factory =>
            {
                Dictionary<string, byte[]> expandedBlocks;
                using (var store = factory())
                {
                    store.Put(getRandomBlocks(store.PageSize));

                    expandedBlocks = new Dictionary<string, byte[]>();
                    foreach (var item in getRandomBlocks(store.PageSize))
                        expandedBlocks.Add(item.Key, item.Value.Concat(item.Value).ToArray());
                    store.Put(expandedBlocks);
                }

                using (var store = factory())
                {
                    foreach (var item in expandedBlocks)
                    {
                        var value = store.Get(item.Key);
                        Assert.NotNull(value);
                        Assert.IsTrue(value.SequenceEqual(item.Value));
                    }
                }
            });

        /// <summary> write N bytes as single operations + write N*2 into same blocks </summary>
        [Test]
        public void Write_Single_WithExpand() =>
            DoIt(factory =>
            {
                Dictionary<string, byte[]> expandedBlocks;
                using (var store = factory())
                {
                    foreach (var item in getRandomBlocks(store.PageSize))
                        store.Put(item.Key, item.Value);

                    expandedBlocks = new Dictionary<string, byte[]>();
                    foreach (var item in getRandomBlocks(store.PageSize))
                        expandedBlocks.Add(item.Key, item.Value.Concat(item.Value).ToArray());
                    foreach (var item in expandedBlocks)
                        store.Put(item.Key, item.Value);
                }

                using (var store = factory())
                {
                    foreach (var item in expandedBlocks)
                    {
                        var value = store.Get(item.Key);
                        Assert.NotNull(value);
                        Assert.IsTrue(value.SequenceEqual(item.Value));
                    }
                }
            });

        #endregion

        #region shrink

        /// <summary> write N*2 bytes with batch + write N bytes into same blocks </summary>
        [Test]
        public void Write_Batch_WithShrink() =>
            DoIt(factory =>
            {
                Dictionary<string, byte[]> shrinkedBlocks;
                using (var store = factory())
                {
                    var expandedBlocks = new Dictionary<string, byte[]>();
                    foreach (var item in getRandomBlocks(store.PageSize))
                        expandedBlocks.Add(item.Key, item.Value.Concat(item.Value).ToArray());

                    store.Put(expandedBlocks);

                    shrinkedBlocks = new Dictionary<string, byte[]>();
                    foreach (var item in getRandomBlocks(store.PageSize))
                        shrinkedBlocks.Add(item.Key, item.Value);
                    store.Put(shrinkedBlocks);
                }

                using (var store = factory())
                {
                    foreach (var item in shrinkedBlocks)
                    {
                        var value = store.Get(item.Key);
                        Assert.NotNull(value);
                        Assert.IsTrue(value.SequenceEqual(item.Value));
                    }
                }
            });

        /// <summary> write N*2 bytes as single operations + write N bytes into same blocks </summary>
        [Test]
        public void Write_Single_WithShrink() =>
            DoIt(factory =>
            {
                Dictionary<string, byte[]> shrinkedBlocks;
                using (var store = factory())
                {
                    foreach (var item in getRandomBlocks(store.PageSize))
                        store.Put(item.Key, item.Value.Concat(item.Value).ToArray());

                    shrinkedBlocks = getRandomBlocks(store.PageSize);
                    foreach (var item in shrinkedBlocks)
                        store.Put(item.Key, item.Value);
                }

                using (var store = factory())
                {
                    foreach (var item in shrinkedBlocks)
                    {
                        var value = store.Get(item.Key);
                        Assert.NotNull(value);
                        Assert.IsTrue(value.SequenceEqual(item.Value));
                    }
                }
            });

        #endregion

        [Test]
        public void Write_Batch_Than_Single() =>
            DoIt(factory =>
            {
                Dictionary<string, byte[]> randomBlocks;
                using (var store = factory())
                {
                    randomBlocks = getRandomBlocks(store.PageSize);
                    store.Put(randomBlocks);

                    randomBlocks = getRandomBlocks(store.PageSize);
                    foreach (var item in randomBlocks)
                        store.Put(item.Key, item.Value);
                }

                using (var store = factory())
                {
                    foreach (var item in randomBlocks)
                    {
                        var value = store.Get(item.Key);
                        Assert.NotNull(value);
                        Assert.IsTrue(value.SequenceEqual(item.Value));
                    }
                }
            });

        [Test]
        public void Write_Single_Than_Batch() =>
            DoIt(factory =>
            {
                Dictionary<string, byte[]> randomBlocks;
                using (var store = factory())
                {
                    randomBlocks = getRandomBlocks(store.PageSize);
                    foreach (var item in randomBlocks)
                        store.Put(item.Key, item.Value);

                    randomBlocks = getRandomBlocks(store.PageSize);
                    store.Put(randomBlocks);
                }

                using (var store = factory())
                {
                    foreach (var item in randomBlocks)
                    {
                        var value = store.Get(item.Key);
                        Assert.NotNull(value);
                        Assert.IsTrue(value.SequenceEqual(item.Value));
                    }
                }
            });
    }
}