using ComPact.MockProvider;
using ComPact.Models;
using ComPact.Models.V3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace ComPact.UnitTests.MockProvider
{
    [TestClass]
    public class MatchableInteractionListTests
    {
        [TestMethod]
        public void ShoudlBeAbleToAddUniqueInteraction()
        {
            var interaction = new Interaction();
            var differentInteraction = new Interaction { Request = new Request { Path = "/test" } };
            var list = new MatchableInteractionList { new MatchableInteraction(interaction) };

            list.AddUnique(new MatchableInteraction(differentInteraction));

            Assert.AreEqual(2, list.Count);
        }

        [TestMethod]
        public void ShoudlBeAbleToAddInteractionWithSameRequestIfProviderStateIsDifferent()
        {
            var interaction = new Interaction();
            var differentInteraction = new Interaction { ProviderStates = new List<ProviderState> { new ProviderState { Name = "some state" } } };
            var list = new MatchableInteractionList { new MatchableInteraction(interaction) };

            list.AddUnique(new MatchableInteraction(differentInteraction));

            Assert.AreEqual(2, list.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(PactException))]
        public void ShoudlNotBeAbleToAddInteractionThatIsNotDistinguishableFromExisting()
        {
            var interaction = new Interaction { Request = new Request { Path = "/test" } };
            var differentInteraction = new Interaction { Request = new Request { Path = "/test" } };
            var list = new MatchableInteractionList { new MatchableInteraction(interaction) };

            try
            {
                list.AddUnique(new MatchableInteraction(differentInteraction));
            }
            catch (PactException e)
            {
                Assert.AreEqual("Cannot add multiple interactions with the same provider states and requests. " +
                    "The provider will not be able to distinguish between them.", e.Message);
                throw;
            }
        }
    }
}
