using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace FileContainer.Tests
{
    public class TestFinds : TestBase
    {
        [Test]
        [TestCase(0,                                            PersistentContainerCompressType.None)]
        [TestCase(0,                                            PersistentContainerCompressType.GZip)]
        [TestCase(0,                                            PersistentContainerCompressType.LZ4)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.None)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.GZip)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.LZ4)]
        public void Find_All(PersistentContainerFlags flags, PersistentContainerCompressType compressType) =>
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
                         Assert.That(r.Select(p => p.Name).Distinct(StringComparer.InvariantCultureIgnoreCase).Count() == randomBlocks.Count);

                         var dt = DateTime.UtcNow;
                         foreach (var item in r)
                         {
                             Assert.That(item           != null);
                             Assert.That(item.Name      != null);
                             Assert.That(item.FirstPage > 0);
                             Assert.That(item.Length    > 0);
                             Assert.That(randomBlocks.ContainsKey(item.Name));
                             Assert.That(item.Length   == randomBlocks[item.Name].Length);
                             Assert.That(item.Modified <= dt);
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
        public void Find_Single(PersistentContainerFlags flags, PersistentContainerCompressType compressType) =>
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
                         Assert.That(r                                                                                 != null);
                         Assert.That(r.Select(p => p.Name).Distinct(StringComparer.InvariantCultureIgnoreCase).Count() == randomBlocks.Count);

                         var dt = DateTime.UtcNow;
                         foreach (var item in r)
                         {
                             Assert.That(item           != null);
                             Assert.That(item.Name      != null);
                             Assert.That(item.FirstPage > 0);
                             Assert.That(item.Length    > 0);
                             Assert.That(randomBlocks.ContainsKey(item.Name));
                             Assert.That(item.Length   == randomBlocks[item.Name].Length);
                             Assert.That(item.Modified <= dt);
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
        public void Find_Multi(PersistentContainerFlags flags, PersistentContainerCompressType compressType) =>
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
                         Assert.That(r                                                                                 != null);
                         Assert.That(r.Select(p => p.Name).Distinct(StringComparer.InvariantCultureIgnoreCase).Count() == randomBlocks.Count);

                         var dt = DateTime.UtcNow;
                         foreach (var item in r)
                         {
                             Assert.That(item           != null);
                             Assert.That(item.Name      != null);
                             Assert.That(item.FirstPage > 0);
                             Assert.That(item.Length    > 0);
                             Assert.That(randomBlocks.ContainsKey(item.Name));
                             Assert.That(item.Length   == randomBlocks[item.Name].Length);
                             Assert.That(item.Modified <= dt);
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
        public void Find_NonExisting(PersistentContainerFlags flags, PersistentContainerCompressType compressType) =>
            DoIt(factory =>
                 {
                     using (var store = factory())
                         store.Put(getRandomBlocks(store.PageSize));

                     using (var store = factory())
                     {
                         var r = store.Find("zz");
                         Assert.That(r        != null);
                         Assert.That(r.Length == 0);
                     }
                 }, flags, compressType);
    }
}