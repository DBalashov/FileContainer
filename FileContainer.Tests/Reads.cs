using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace FileContainer.Tests
{
    public class TestReads : TestBase
    {
        [Test]
        [TestCase(0,                                            PersistentContainerCompressType.None)]
        [TestCase(0,                                            PersistentContainerCompressType.GZip)]
        [TestCase(0,                                            PersistentContainerCompressType.LZ4)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.None)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.GZip)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.LZ4)]
        public void Read_Batch(PersistentContainerFlags flags, PersistentContainerCompressType compressType) =>
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

                         Assert.That(r.Keys.OrderBy(p => p).ToArray().SequenceEqual(getPages.OrderBy(p => p.Key).Select(p => p.Key).ToArray()));
                         foreach (var item in r)
                         {
                             Assert.That(item.Value != null);
                             Assert.That(item.Value!.SequenceEqual(r[item.Key]));
                         }
                     }
                 }, flags, compressType);

        [Test]
        [TestCase(0,                                            PersistentContainerCompressType.None)]
        [TestCase(0,                                            PersistentContainerCompressType.GZip)]
        [TestCase(0,                                            PersistentContainerCompressType.LZ4)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.None)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.GZip)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.LZ4)]
        public void Read_Single(PersistentContainerFlags flags, PersistentContainerCompressType compressType) =>
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

                         Assert.That(r.Keys.OrderBy(p => p).ToArray().SequenceEqual(getPages.OrderBy(p => p.Key).Select(p => p.Key).ToArray()));
                         foreach (var item in r)
                         {
                             Assert.That(item.Value != null);
                             Assert.That(item.Value!.SequenceEqual(r[item.Key]));
                         }
                     }
                 }, flags, compressType);

        [Test]
        [TestCase(0,                                            PersistentContainerCompressType.None)]
        [TestCase(0,                                            PersistentContainerCompressType.GZip)]
        [TestCase(0,                                            PersistentContainerCompressType.LZ4)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.None)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.GZip)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.LZ4)]
        public void Read_Batch_WithOffset(PersistentContainerFlags flags, PersistentContainerCompressType compressType)
        {
            // todo
        }

        [Test]
        [TestCase(0,                                            PersistentContainerCompressType.None)]
        [TestCase(0,                                            PersistentContainerCompressType.GZip)]
        [TestCase(0,                                            PersistentContainerCompressType.LZ4)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.None)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.GZip)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.LZ4)]
        public void Read_Single_WithOffset(PersistentContainerFlags flags, PersistentContainerCompressType compressType)
        {
            // todo
        }

        [Test]
        [TestCase(0,                                            PersistentContainerCompressType.None)]
        [TestCase(0,                                            PersistentContainerCompressType.GZip)]
        [TestCase(0,                                            PersistentContainerCompressType.LZ4)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.None)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.GZip)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.LZ4)]
        public void Read_Batch_WithOffsetLength(PersistentContainerFlags flags, PersistentContainerCompressType compressType)
        {
            // todo
        }

        [Test]
        [TestCase(0,                                            PersistentContainerCompressType.None)]
        [TestCase(0,                                            PersistentContainerCompressType.GZip)]
        [TestCase(0,                                            PersistentContainerCompressType.LZ4)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.None)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.GZip)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.LZ4)]
        public void Read_Single_WithOffsetLength(PersistentContainerFlags flags, PersistentContainerCompressType compressType)
        {
            // todo
        }
    }
}