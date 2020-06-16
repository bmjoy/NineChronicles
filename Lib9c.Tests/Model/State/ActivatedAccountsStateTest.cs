using Bencodex.Types;
using Libplanet;
using Nekoyume.Model.State;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;

namespace Lib9c.Tests.Model.State
{
    public class ActivatedAccountsStateTest
    {
        [Fact]
        public void Serialize()
        {
            var accounts = ImmutableHashSet<Address>.Empty.Add(new Address());
            var state = new ActivatedAccountsState(accounts);

            var serialized = (Dictionary)state.Serialize();
            var deserialized = new ActivatedAccountsState(serialized);

            Assert.Equal(accounts, deserialized.Accounts);
        }

        [Fact]
        public void SerializeWithDotNetAPI()
        {
            var accounts = ImmutableHashSet<Address>.Empty.Add(new Address());
            var state = new ActivatedAccountsState(accounts);

            var formatter = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                formatter.Serialize(ms, state);
                ms.Seek(0, SeekOrigin.Begin);

                var deserialized = (ActivatedAccountsState)formatter.Deserialize(ms);
                Assert.Equal(accounts, deserialized.Accounts);
            }
        }
    }
}
