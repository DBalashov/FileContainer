using System;
using NUnit.Framework;

namespace FileContainer.Tests
{
    public class TestDeletes : TestBase
    {
        [Test]
        public void Delete_Single() =>
            DoIt(factory =>
            {
                using (var store = factory())
                {
                    store.Put(getRandomBlocks(store.PageSize));
                    store.Delete("dir/file01.txt");
                }

                using (var store = factory())
                {
                    Assert.IsNull(store.Get("dir/file01.txt"));
                    Assert.IsEmpty(store.Find("dir/file01.txt"));
                }
            });

        [Test]
        public void Delete_Single_Mask() =>
            DoIt(factory =>
            {
                using (var store = factory())
                {
                    store.Put(getRandomBlocks(store.PageSize));
                    store.Delete("dir/fileA*");
                }

                using (var store = factory())
                {
                    Assert.IsNull(store.Get("dir/fileA*"));
                    Assert.IsEmpty(store.Find("dir/fileA*"));
                }
            });

        [Test]
        public void Delete_NonExisting_Single() =>
            DoIt(factory =>
            {
                using (var store = factory())
                {
                    store.Put(getRandomBlocks(store.PageSize));
                }

                using (var store = factory())
                {
                    var r = store.Delete("aaa");
                    Assert.IsNotNull(r);
                    Assert.IsEmpty(r);
                }
            });

        [Test]
        public void Delete_NonExisting_Multi() =>
            DoIt(factory =>
            {
                using (var store = factory())
                {
                    store.Put(getRandomBlocks(store.PageSize));
                }

                using (var store = factory())
                {
                    var r = store.Delete("aaa", "bbb");
                    Assert.IsNotNull(r);
                    Assert.IsEmpty(r);
                }
            });
    }
}