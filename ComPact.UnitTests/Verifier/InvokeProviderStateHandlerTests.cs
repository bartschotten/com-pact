using ComPact.Exceptions;
using ComPact.Models;
using ComPact.Verifier;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ComPact.UnitTests.Verifier
{
    [TestClass]
    public class InvokeProviderStateHandlerTests
    {
        [TestMethod]
        public void ShouldInvokeProviderStateHandlerForEveryProviderState()
        {
            var invocations = new List<string>();

            var config = new PactVerifierConfig { ProviderStateHandler = (p) => invocations.Add(p.Name) };

            var verifier = new PactVerifier(config);
            verifier.InvokeProviderStateHandler(new List<ProviderState> { new ProviderState { Name = "ps1" }, new ProviderState { Name = "ps2" } });

            Assert.AreEqual(2, invocations.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(PactException))]
        public void ShouldThrowPactExceptionWhenProviderStateHandlerIsNotConfigured()
        {
            var verifier = new PactVerifier(new PactVerifierConfig());
            try
            {
                verifier.InvokeProviderStateHandler(new List<ProviderState> { new ProviderState { Name = "ps1" } });
            }
            catch (PactException e)
            {
                Assert.AreEqual("Cannot verify this Pact contract because a ProviderStateHandler was not configured.", e.Message);
                throw;
            }
        }

        [TestMethod]
        public void ShouldReturnVerificationMessagesWhenHandlerThrowsPactVerificationException()
        {
            var verifier = new PactVerifier(new PactVerifierConfig { ProviderStateHandler = (p) => throw new PactVerificationException("Unknown provider state.") });

            var verificationMessages = verifier.InvokeProviderStateHandler(new List<ProviderState> { new ProviderState { Name = "ps1" } });

            Assert.AreEqual($"Provider could not handle provider state \"ps1\": Unknown provider state.", verificationMessages.First());
        }

        [TestMethod]
        [ExpectedException(typeof(PactException))]
        public void ShouldThrowPactExceptionWhenProviderStateHandlerThrowsAnyOtherException()
        {
            var verifier = new PactVerifier(new PactVerifierConfig { ProviderStateHandler = (p) => throw new ArgumentNullException() });
            try
            {
                verifier.InvokeProviderStateHandler(new List<ProviderState> { new ProviderState { Name = "ps1" } });
            }
            catch (PactException e)
            {
                Assert.AreEqual("Exception occured while invoking ProviderStateHandler.", e.Message);
                throw;
            }
        }
    }
}
