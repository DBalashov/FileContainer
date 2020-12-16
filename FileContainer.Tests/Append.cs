using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace FileContainer.Tests
{
    public class TestAppends : TestBase
    {
        [Test]
        public void Append_Single() =>
            DoIt(factory =>
            {
                var randomBlocks = new Dictionary<string, byte[]>();
                using (var store = factory())
                {
                    randomBlocks.Add("small", getRandomBytes(store.PageSize / 2));
                    randomBlocks.Add("normal", getRandomBytes(store.PageSize / 2));
                    randomBlocks.Add("big", getRandomBytes(store.PageSize / 2));
                    store.Put(randomBlocks);
                }

                using (var store = factory())
                {
                    randomBlocks["small"]  = randomBlocks["small"].Concat(getRandomBytes(store.PageSize / 2 - 8)).ToArray(); // чтобы уложить на страницу
                    randomBlocks["normal"] = randomBlocks["normal"].Concat(getRandomBytes(store.PageSize)).ToArray();        // чтобы вылезть на следующую страницу
                    randomBlocks["big"]    = randomBlocks["big"].Concat(getRandomBytes(store.PageSize * 3)).ToArray();       // чтобы вылезть на несколько следующих страниц

                    foreach (var item in randomBlocks)
                        store.Append(item.Key, item.Value);

                    var r = new Dictionary<string, byte[]>();
                    foreach (var item in randomBlocks)
                        r.Add(item.Key, store.Get(item.Key));

                    Assert.IsTrue(r.Keys.OrderBy(p => p).ToArray().SequenceEqual(randomBlocks.OrderBy(p => p.Key).Select(p => p.Key).ToArray()));
                    foreach (var item in r)
                    {
                        Assert.NotNull(item.Value);
                        Assert.IsTrue(item.Value.SequenceEqual(r[item.Key]));
                    }
                }
            });

        [Test]
        public void Append_Multi() =>
            DoIt(factory =>
            {
                var randomBlocks = new Dictionary<string, byte[]>();
                using (var store = factory())
                {
                    randomBlocks.Add("small", getRandomBytes(store.PageSize / 2));
                    randomBlocks.Add("normal", getRandomBytes(store.PageSize / 2));
                    randomBlocks.Add("big", getRandomBytes(store.PageSize / 2));
                    store.Put(randomBlocks);
                }

                using (var store = factory())
                {
                    randomBlocks["small"]  = randomBlocks["small"].Concat(getRandomBytes(store.PageSize / 2 - 8)).ToArray(); // чтобы уложить на страницу
                    randomBlocks["normal"] = randomBlocks["normal"].Concat(getRandomBytes(store.PageSize)).ToArray();        // чтобы вылезть на следующую страницу
                    randomBlocks["big"]    = randomBlocks["big"].Concat(getRandomBytes(store.PageSize * 3)).ToArray();       // чтобы вылезть на несколько следующих страниц

                    store.Append(randomBlocks);

                    var r = store.Get(randomBlocks.Keys.ToArray());

                    Assert.IsTrue(r.Keys.OrderBy(p => p).ToArray().SequenceEqual(randomBlocks.OrderBy(p => p.Key).Select(p => p.Key).ToArray()));
                    foreach (var item in r)
                    {
                        Assert.NotNull(item.Value);
                        Assert.IsTrue(item.Value.SequenceEqual(r[item.Key]));
                    }
                }
            });
    }
}