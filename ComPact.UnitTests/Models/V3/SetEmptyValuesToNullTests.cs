using ComPact.Models;
using ComPact.Models.V3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace ComPact.UnitTests.Models.V3
{
    [TestClass]
    public class SetEmptyValuesToNullTests
    {
        [TestMethod]
        public void ShouldSetEmptyValuesToNull()
        {
            var interaction = new Interaction();

            interaction.SetEmptyValuesToNull();

            Assert.IsNull(interaction.ProviderStates);
            Assert.IsNull(interaction.Request.Headers);
            Assert.IsNull(interaction.Request.Body);
            Assert.IsNull(interaction.Request.Query);
            Assert.IsNull(interaction.Response.Headers);
            Assert.IsNull(interaction.Response.Body);
            Assert.IsNull(interaction.Response.MatchingRules);
        }

        [TestMethod]
        public void ShouldNotSetNonEmptyValuesToNull()
        {
            var request = new Request
            {
                Headers = new Headers { { "Accept", "application/json" } },
                Body = "test",
                Query = new Query("skip=100&take=10")
            };

            var response = new Response
            {
                Headers = new Headers { { "Content-Type", "application/json" } },
                Body = "test",
                MatchingRules = new MatchingRuleCollection(new Dictionary<string, Matcher> { { "$.body", new Matcher { MatcherType = MatcherType.type } } })
            };

            var interaction = new Interaction
            {
                ProviderStates = new List<ProviderState> { new ProviderState { Name = "provider-state" } },
                Request = request,
                Response = response
            };

            interaction.SetEmptyValuesToNull();

            Assert.IsNotNull(interaction.ProviderStates);
            Assert.IsNotNull(interaction.Description);
            Assert.IsNotNull(interaction.Request.Headers);
            Assert.IsNotNull(interaction.Request.Body);
            Assert.IsNotNull(interaction.Request.Query);
            Assert.IsNotNull(interaction.Request.Path);
            Assert.IsNotNull(interaction.Request.Method);
            Assert.IsNotNull(interaction.Response.Headers);
            Assert.IsNotNull(interaction.Response.Body);
            Assert.IsNotNull(interaction.Response.MatchingRules.Body);
            Assert.IsNotNull(interaction.Response.MatchingRules.Header);
        }

        [TestMethod]
        public void MessageShouldSetEmptyValuesToNull()
        {
            var message = new Message();

            message.SetEmptyValuesToNull();

            Assert.IsNull(message.ProviderStates);
            Assert.IsNull(message.MatchingRules);
        }

        [TestMethod]
        public void MessageShouldNotSetNonEmptyValuesToNull()
        {
            var message = new Message
            {
                ProviderStates = new List<ProviderState> { new ProviderState { Name = "provider-state" } },
                MatchingRules = new MatchingRuleCollection(new Dictionary<string, Matcher> { { "$.body", new Matcher { MatcherType = MatcherType.type } } })
            };

            message.SetEmptyValuesToNull();

            Assert.IsNotNull(message.ProviderStates);
            Assert.IsNotNull(message.Description);
            Assert.IsNotNull(message.MatchingRules.Body);
            Assert.IsNotNull(message.MatchingRules.Header);
            Assert.IsNotNull(message.Metadata);
        }
    }
}
