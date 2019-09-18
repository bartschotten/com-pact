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

            PactVerifier.InvokeProviderStateHandler(
                new List<ProviderState> { new ProviderState { Name = "ps1" }, new ProviderState { Name = "ps2" } },
                (p) => invocations.Add(p.Name));

            Assert.AreEqual(2, invocations.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(PactException))]
        public void ShouldThrowPactExceptionWhenProviderStateHandlerIsNotConfigured()
        {
            try
            {
                PactVerifier.InvokeProviderStateHandler(new List<ProviderState> { new ProviderState { Name = "ps1" } }, null);
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
            var verificationMessages = PactVerifier.InvokeProviderStateHandler(
                new List<ProviderState> { new ProviderState { Name = "ps1" } },
                (p) => throw new PactVerificationException("Unknown provider state."));

            Assert.AreEqual($"Provider could not handle provider state \"ps1\": Unknown provider state.", verificationMessages.First());
        }

        [TestMethod]
        [ExpectedException(typeof(PactException))]
        public void ShouldThrowPactExceptionWhenProviderStateHandlerThrowsAnyOtherException()
        {
            try
            {
                PactVerifier.InvokeProviderStateHandler(new List<ProviderState> { new ProviderState { Name = "ps1" } }, (p) => throw new ArgumentNullException());
            }
            catch (PactException e)
            {
                Assert.AreEqual("Exception occured while invoking ProviderStateHandler.", e.Message);
                throw;
            }
        }
    }
}
