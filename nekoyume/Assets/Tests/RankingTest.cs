using System;
using System.Linq;
using Libplanet;
using Libplanet.Crypto;
using Nekoyume.State;
using Nekoyume.UI;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class RankingTest : PlayModeTest
    {
        [Test]
        public void GetAgentAddressesEmpty()
        {
            var state = new RankingState();
            var result = state.GetAgentAddresses(1, null);
            Assert.IsEmpty(result);
        }

        [Test]
        public void GetAgentAddressesFilterDuplicated()
        {
            var state = new RankingState();
            var agentAddress = GetNewAddress();
            var avatar1 = new AvatarState(GetNewAddress(), agentAddress, 1, 20);
            var avatar2 = new AvatarState(GetNewAddress(), agentAddress, 1, 20);
            state.Update(avatar1);
            state.Update(avatar2);
            var result = state.GetAgentAddresses(2, null);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(agentAddress, result.First());
        }

        [Test]
        public void GetAgentAddresses()
        {
            var state = new RankingState();
            var avatar1 = new AvatarState(GetNewAddress(), GetNewAddress(), 1, 20);
            var avatar2 = new AvatarState(GetNewAddress(), GetNewAddress(), 1, 20);
            state.Update(avatar1);
            state.Update(avatar2);
            var result = state.GetAgentAddresses(3, null);
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(avatar1.agentAddress, result.First());
            Assert.AreEqual(avatar2.agentAddress, result.Last());
        }

        private static Address GetNewAddress()
        {
            return new PrivateKey().PublicKey.ToAddress();
        }

        [Test]
        public void GetAvatars()
        {
            var state = new RankingState();
            var avatar1 = new AvatarState(GetNewAddress(), GetNewAddress(), 1, 20)
            {
                exp = 2, updatedAt = DateTimeOffset.UtcNow
            };
            var avatar2 = new AvatarState(GetNewAddress(), GetNewAddress(), 1, 20)
            {
                exp = 2, updatedAt = DateTimeOffset.UtcNow
            };
            
            Assert.Greater(avatar2.updatedAt, avatar1.updatedAt);
            state.Update(avatar2);
            state.Update(avatar1);
            var result = state.GetAvatars(null);
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(avatar1.updatedAt, result.First().updatedAt);
            Assert.AreEqual(avatar2.updatedAt, result.Last().updatedAt);
        }

        [Test]
        public void GetAvatarsWithTimeStamp()
        {
            var state = new RankingState();
            var avatar1 = new AvatarState(GetNewAddress(), GetNewAddress(), 1, 20)
            {
                exp = 2, updatedAt = DateTimeOffset.UtcNow
            };
            var avatar2 = new AvatarState(GetNewAddress(), GetNewAddress(), 1, 20)
            {
                exp = 2, updatedAt = DateTimeOffset.UtcNow
            };
            var avatar3 = new AvatarState(GetNewAddress(), GetNewAddress(), 1, 20)
            {
                exp = 3, updatedAt = DateTimeOffset.UtcNow.AddDays(-2)
            };

            Assert.Greater(avatar2.updatedAt, avatar1.updatedAt);
            state.Update(avatar2);
            state.Update(avatar1);
            state.Update(avatar3);
            Assert.AreEqual(3, state.GetAvatars(null).First().exp);
            var result = state.GetAvatars(DateTimeOffset.UtcNow);
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(avatar1.updatedAt, result.First().updatedAt);
            Assert.AreEqual(avatar2.updatedAt, result.Last().updatedAt);
        }

        [Test]
        public void Update()
        {
            var state = new RankingState();
            var avatar1 = new AvatarState(GetNewAddress(), GetNewAddress(), 1, 20)
            {
                exp = 2, updatedAt = DateTimeOffset.UtcNow
            };

            state.Update(avatar1);
            Assert.AreEqual(2, state.GetAvatars(null).First().exp);

            avatar1.exp = 1;
            state.Update(avatar1);
            Assert.AreEqual(1, state.GetAvatars(null).First().exp);

            avatar1.exp = 3;
            state.Update(avatar1);
            Assert.AreEqual(3, state.GetAvatars(null).First().exp);
        }

        [Test]
        public void RankingInfo()
        {
            var widget = Widget.Find<RankingBoard>();
            var rankingInfo = widget.rankingBase;
            var agentAddress = GetNewAddress();
            var avatar = new AvatarState(GetNewAddress(), agentAddress, 1, 20);
            rankingInfo.Set(1, avatar);
            Assert.NotNull(rankingInfo.icon.sprite);
        }
    }
}
