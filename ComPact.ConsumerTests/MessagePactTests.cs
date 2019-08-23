using ComPact.Builders.V3;
using ComPact.ConsumerTests.Handler;
using ComPact.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace ComPact.ConsumerTests
{
    [TestClass]
    public class MessagePactTests
    {
        private MessageBuilder _messageBuilder;

        [TestInitialize]
        public void SetupMockMessageProvider()
        {
            var ingredient = Some.Object.With(
                                Some.Element.Named("name").Like("Salt"),
                                Some.Element.Named("amount").Like(5.5),
                                Some.Element.Named("unit").Like("gram"));

            _messageBuilder = Pact.Message
                .Given(new ProviderState { Name = "A new recipe has been added." })
                .ShouldSend("a RecipeAdded event.")
                .With(Pact.JsonContent.With(
                    Some.String.Named("eventId").LikeGuid("f84fe18f-d871-4dad-9723-65b6dc9b0578"),
                    Some.Object.Named("recipe").With(
                        Some.Element.Named("name").Like("A Recipe"),
                        Some.Element.Named("instructions").Like("Mix it up"),
                        Some.Array.Named("ingredients").InWhichEveryElementIsLike(ingredient)
                    )));
        }

        [TestMethod]
        public void ShouldCreateMessagePactAfterVerifyingConsumer()
        {
            var handler = new RecipeAddedHandler();

            var builder = new MessagePactBuilder("messageConsumer", "messageProvider");

            builder.SetupMessage(_messageBuilder
                .VerifyConsumer<RecipeAdded>(m => handler.Handle(m)))
                .Build();

            Assert.IsNotNull(handler.ReceivedRecipes.FirstOrDefault());
            Assert.AreEqual("A Recipe", handler.ReceivedRecipes.First().Name);
        }

        [TestMethod]
        public void ShouldBeAbleToGetSerializedMessage()
        {
            string receivedMessage = null;

            var builder = new MessagePactBuilder("messageConsumer", "messageProvider");

            builder.SetupMessage(_messageBuilder.VerifyConsumer(m => receivedMessage = m));

            Assert.IsNotNull(receivedMessage);
            var deserializedMessage = JsonConvert.DeserializeObject<RecipeAdded>(receivedMessage);
            Assert.AreEqual("A Recipe", deserializedMessage.Recipe.Name);
        }

        [TestMethod]
        public void ShouldThrowPactExceptionWhenHandlerThrowsException()
        {
            var builder = new MessagePactBuilder("messageConsumer", "messageProvider");

            try
            {
                builder.SetupMessage(_messageBuilder.VerifyConsumer(m => throw new NullReferenceException()));
            }
            catch (PactException e)
            {
                Assert.AreEqual("Message handler threw an exception", e.Message);
            }
        }
    }
}
