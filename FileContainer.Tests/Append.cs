using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace FileContainer.Tests
{
    public class TestAppends : TestBase
    {
        [Test]
        [TestCase(0,                                            PersistentContainerCompressType.None)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.None)]
        public virtual void Append_Single(PersistentContainerFlags flags, PersistentContainerCompressType compression) =>
            DoIt(factory =>
                 {
                     var randomBlocks = new Dictionary<string, byte[]>();
                     using (var store = factory())
                     {
                         randomBlocks.Add("small",  getRandomBytes(store.PageSize / 2));
                         randomBlocks.Add("normal", getRandomBytes(store.PageSize / 2));
                         randomBlocks.Add("big",    getRandomBytes(store.PageSize / 2));
                         store.Put(randomBlocks);
                     }

                     using (var store = factory())
                     {
                         randomBlocks["small"]  = randomBlocks["small"].Concat(getRandomBytes(store.PageSize / 2 - 8)).ToArray(); // only one page
                         randomBlocks["normal"] = randomBlocks["normal"].Concat(getRandomBytes(store.PageSize)).ToArray();        // one + part of second page
                         randomBlocks["big"]    = randomBlocks["big"].Concat(getRandomBytes(store.PageSize * 3)).ToArray();       // several pages

                         foreach (var item in randomBlocks)
                             store.Append(item.Key, item.Value);

                         var r = new Dictionary<string, byte[]>();
                         foreach (var item in randomBlocks)
                             r.Add(item.Key, store.Get(item.Key));

                         Assert.That(r.Keys.OrderBy(p => p).ToArray().SequenceEqual(randomBlocks.OrderBy(p => p.Key).Select(p => p.Key).ToArray()));
                         foreach (var item in r)
                         {
                             Assert.That(item.Value != null);
                             Assert.That(item.Value!.SequenceEqual(r[item.Key]));
                         }
                     }
                 }, flags, compression);

        [Test]
        [TestCase(0,                                            PersistentContainerCompressType.None)]
        [TestCase(PersistentContainerFlags.WriteDirImmediately, PersistentContainerCompressType.None)]
        public virtual void Append_Multi(PersistentContainerFlags flags, PersistentContainerCompressType compression) =>
            DoIt(factory =>
                 {
                     var randomBlocks = new Dictionary<string, byte[]>();
                     using (var store = factory())
                     {
                         randomBlocks.Add("small",  getRandomBytes(store.PageSize / 2));
                         randomBlocks.Add("normal", getRandomBytes(store.PageSize / 2));
                         randomBlocks.Add("big",    getRandomBytes(store.PageSize / 2));
                         store.Put(randomBlocks);
                     }

                     using (var store = factory())
                     {
                         randomBlocks["small"]  = randomBlocks["small"].Concat(getRandomBytes(store.PageSize / 2 - 8)).ToArray(); // only one page
                         randomBlocks["normal"] = randomBlocks["normal"].Concat(getRandomBytes(store.PageSize)).ToArray();        // one + part of second page
                         randomBlocks["big"]    = randomBlocks["big"].Concat(getRandomBytes(store.PageSize * 3)).ToArray();       // several pages

                         store.Append(randomBlocks);

                         var r = store.Get(randomBlocks.Keys.ToArray());

                         Assert.That(r.Keys.OrderBy(p => p).ToArray().SequenceEqual(randomBlocks.OrderBy(p => p.Key).Select(p => p.Key).ToArray()));
                         foreach (var item in r)
                         {
                             Assert.That(item.Value != null);
                             Assert.That(item.Value!.SequenceEqual(r[item.Key]));
                         }
                     }
                 }, flags, compression);
    }
}