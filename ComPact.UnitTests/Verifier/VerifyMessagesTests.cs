using ComPact.Exceptions;
using ComPact.Models.V3;
using ComPact.Verifier;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace ComPact.UnitTests.Verifier
{
    [TestClass]
    public class VerifyMessagesTests
    {
        private List<Message> _messages;

        public VerifyMessagesTests()
        {
            _messages = new List<Message>
            {
                new Message
                {
                    Description = "A message",
                    ProviderStates = new List<ComPact.Models.ProviderState> {new ComPact.Models.ProviderState { Name = "Some state"} },
                    Contents = new { text = "test" }
                },
                new Message
                {
                    Description = "Another message",
                    ProviderStates = new List<ComPact.Models.ProviderState> {new ComPact.Models.ProviderState { Name = "Other state"} },
                    Contents = new { text = "test" }
                }
            };
        }

        [TestMethod]
        public void ShouldReturnSuccessfulTest()
        {
            var tests = PactVerifier.VerifyMessages(_messages, (p) => { }, (d) => new { text = "test" });

            Assert.AreEqual(2, tests.Count);
            Assert.AreEqual("passed", tests.First().Status);
            Assert.AreEqual("A message", tests.First().Description);
        }

        [TestMethod]
        public void ShouldReturnFailedTestWhenWrongMessageIsReturned()
        {
            var tests = PactVerifier.VerifyMessages(_messages, (p) => { }, (d) => new { text = "wrong" });

            Assert.AreEqual(2, tests.Count);
            Assert.AreEqual("failed", tests.First().Status);
            Assert.AreEqual("A message", tests.First().Description);
        }

        [TestMethod]
        public void ShouldReturnFailedTestWhenHandlerThrowsPactVerificationException()
        {
            var tests = PactVerifier.VerifyMessages(_messages, (p) => throw new PactVerificationException("Unknown provider state."), (d) => new { text = "test" });

            Assert.AreEqual(2, tests.Count);
            Assert.AreEqual("failed", tests.First().Status);
            Assert.AreEqual("A message", tests.First().Description);
            Assert.IsTrue(tests.First().Issues.First().Contains("Unknown provider state."));
        }

        [TestMethod]
        public void ShouldReturnFailedTestWhenMessageProducerThrowsPactVerificationException()
        {
            var tests = PactVerifier.VerifyMessages(_messages, (p) => { }, (d) => throw new PactVerificationException("Unknown description."));

            Assert.AreEqual(2, tests.Count);
            Assert.AreEqual("failed", tests.First().Status);
            Assert.AreEqual("A message", tests.First().Description);
            Assert.IsTrue(tests.First().Issues.First().Contains("Unknown description."));
        }
    }
}
