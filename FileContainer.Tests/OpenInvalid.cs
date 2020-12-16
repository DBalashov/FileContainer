using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace FileContainer.Tests
{
    public class TestOpenInvalid : TestBase
    {
        [Test]
        public void Open_Broken_Sign() =>
            DoIt(factory =>
            {
                using (var store = factory())
                {
                    store.Put(getRandomBlocks(store.PageSize));

                    store.stm.Position = 0;
                    store.stm.Write(BitConverter.GetBytes(123), 0, 4); // портим sign
                }

                Assert.Catch<InvalidDataException>(() =>
                {
                    using (var store = factory())
                    {
                    }
                });
            });

        [Test]
        public void Open_Broken_PageSize() =>
            DoIt(factory =>
            {
                using (var store = factory())
                {
                    store.Put(getRandomBlocks(store.PageSize));

                    store.stm.Position = 4;
                    store.stm.Write(BitConverter.GetBytes(1), 0, 4); // портим размер страницы
                }

                Assert.Catch<ArgumentException>(() =>
                {
                    using (var store = factory())
                    {
                    }
                });
            });
    }
}