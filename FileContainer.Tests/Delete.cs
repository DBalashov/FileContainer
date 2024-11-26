using NUnit.Framework;

namespace FileContainer.Tests
{
    public class TestDeletes : TestBase
    {
        [Test]
        [TestCase(0,                                            PersistentContainerCompressType.None)]
        [TestCase(0,                                            PersistentContainerCompressType.GZip)]
        [TestCase(0,                                            PersistentContainerCompressType.LZ4)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.None)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.GZip)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.LZ4)]
        public void Delete_Single(PersistentContainerFlags flags, PersistentContainerCompressType compressType) =>
            DoIt(factory =>
                 {
                     using (var store = factory())
                     {
                         store.Put(getRandomBlocks(store.PageSize));
                         store.Delete("dir/file01.txt");
                     }

                     using (var store = factory())
                     {
                         Assert.That(store.Get("dir/file01.txt")         == null);
                         Assert.That(store.Find("dir/file01.txt").Length == 0);
                     }
                 }, flags, compressType);

        [Test]
        [TestCase(0,                                            PersistentContainerCompressType.None)]
        [TestCase(0,                                            PersistentContainerCompressType.GZip)]
        [TestCase(0,                                            PersistentContainerCompressType.LZ4)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.None)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.GZip)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.LZ4)]
        public void Delete_Single_Mask(PersistentContainerFlags flags, PersistentContainerCompressType compressType) =>
            DoIt(factory =>
                 {
                     using (var store = factory())
                     {
                         store.Put(getRandomBlocks(store.PageSize));
                         store.Delete("dir/fileA*");
                     }

                     using (var store = factory())
                     {
                         Assert.That(store.Get("dir/fileA*")         == null);
                         Assert.That(store.Find("dir/fileA*").Length == 0);
                     }
                 }, flags, compressType);

        [Test]
        [TestCase(0,                                            PersistentContainerCompressType.None)]
        [TestCase(0,                                            PersistentContainerCompressType.GZip)]
        [TestCase(0,                                            PersistentContainerCompressType.LZ4)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.None)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.GZip)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.LZ4)]
        public void Delete_NonExisting_Single(PersistentContainerFlags flags, PersistentContainerCompressType compressType) =>
            DoIt(factory =>
                 {
                     using (var store = factory())
                         store.Put(getRandomBlocks(store.PageSize));

                     using (var store = factory())
                     {
                         var r = store.Delete("aaa");
                         Assert.That(r.Length == 0);
                     }
                 }, flags, compressType);

        [Test]
        [TestCase(0,                                            PersistentContainerCompressType.None)]
        [TestCase(0,                                            PersistentContainerCompressType.GZip)]
        [TestCase(0,                                            PersistentContainerCompressType.LZ4)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.None)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.GZip)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.LZ4)]
        public void Delete_NonExisting_Multi(PersistentContainerFlags flags, PersistentContainerCompressType compressType) =>
            DoIt(factory =>
                 {
                     using (var store = factory())
                         store.Put(getRandomBlocks(store.PageSize));

                     using (var store = factory())
                     {
                         var r = store.Delete("aaa", "bbb");
                         Assert.That(r.Length == 0);
                     }
                 }, flags, compressType);
    }
}