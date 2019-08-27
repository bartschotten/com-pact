using ComPact.Models;
using ComPact.Models.V2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace ComPact.UnitTests.Models.V2
{
    [TestClass]
    public class SetEmptyValuesToNullTests
    {
        [TestMethod]
        public void ShouldSetEmptyValuesToNull()
        {
            var interaction = new Interaction();

            interaction.SetEmptyValuesToNull();

            Assert.IsNull(interaction.ProviderState);
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
            var interaction = new Interaction
            {
                ProviderState = "provider state",
                Request = new Request
                {
                    Headers = new Headers { { "Accept", "application/json" } },
                    Body = "test",
                    Query = "skip=100&take=10"
                },
                Response = new Response
                {
                    Headers = new Headers { { "Content-Type", "application/json" } },
                    Body = "test",
                    MatchingRules = new Dictionary<string, Matcher> { { "$", new Matcher { MatcherType = MatcherType.type } } }
                }
            };

            interaction.SetEmptyValuesToNull();

            Assert.IsNotNull(interaction.ProviderState);
            Assert.IsNotNull(interaction.Description);
            Assert.IsNotNull(interaction.Request.Headers);
            Assert.IsNotNull(interaction.Request.Body);
            Assert.IsNotNull(interaction.Request.Query);
            Assert.IsNotNull(interaction.Request.Path);
            Assert.IsNotNull(interaction.Request.Method);
            Assert.IsNotNull(interaction.Response.Headers);
            Assert.IsNotNull(interaction.Response.Body);
            Assert.IsNotNull(interaction.Response.MatchingRules);
        }
    }
}
