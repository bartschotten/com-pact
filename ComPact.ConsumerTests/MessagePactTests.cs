﻿using ComPact.Builders;
using ComPact.Builders.V3;
using ComPact.ConsumerTests.Handler;
using ComPact.ConsumerTests.TestSupport;
using ComPact.Exceptions;
using ComPact.Models;
using ComPact.Tests.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

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
                .Given(new ProviderState { Name = "A new recipe has been added.", Params = new Dictionary<string, string> { { "recipeId", "7169de6d-df9b-4cf5-8cdc-2654062e5cdc" } } })
                .ShouldSend("a RecipeAdded event.")
                .With(Pact.JsonContent.With(
                    Some.String.Named("eventId").LikeGuid("f84fe18f-d871-4dad-9723-65b6dc9b0578"),
                    Some.Object.Named("recipe").With(
                        Some.Element.Named("name").Like("A Recipe"),
                        Some.Element.Named("instructions").Like("Mix it up"),
                        Some.Array.Named("ingredients").InWhichEveryElementIs(ingredient)
                    )));
        }

        [TestMethod]
        public async Task ShouldCreateMessagePactAfterVerifyingConsumer()
        {
            var handler = new RecipeAddedHandler();

            var fakePactBrokerMessageHandler = new FakePactBrokerMessageHandler();
            fakePactBrokerMessageHandler.Configure(HttpMethod.Put, "http://localhost:9292").RespondsWith(HttpStatusCode.Created);
            var publisher = new PactPublisher(new HttpClient(fakePactBrokerMessageHandler) { BaseAddress = new Uri("http://localhost:9292") }, "1.0", "local");

            var builder = new MessagePactBuilder("messageConsumer", "messageProvider", publisher);

            await builder.SetUp(_messageBuilder
                .VerifyConsumer<RecipeAdded>(m => handler.Handle(m)))
                .BuildAsync();

            // Check if handler has been called
            Assert.IsNotNull(handler.ReceivedRecipes.FirstOrDefault());
            Assert.AreEqual("A Recipe", handler.ReceivedRecipes.First().Name);

            // Check if pact has been published and tagged
            Assert.AreEqual("messageConsumer",
                JsonConvert.DeserializeObject<Contract>(fakePactBrokerMessageHandler
                        .GetStatus(HttpMethod.Put, "http://localhost:9292").SentRequestContents.First().Value).Consumer
                    .Name);
            Assert.IsTrue(fakePactBrokerMessageHandler.GetStatus(HttpMethod.Put, "http://localhost:9292").SentRequestContents.Last().Key.Contains("local"));

            // Check if pact has been written to project directory.
            var buildDirectory = AppContext.BaseDirectory;
            var pactDir =
                Path.GetFullPath(
                    $"{buildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}");
            var pactFile = File.ReadAllText(pactDir + "messageConsumer-messageProvider.json");
            Assert.AreEqual("messageConsumer", JsonConvert.DeserializeObject<Contract>(pactFile).Consumer.Name);
            File.Delete(pactDir + "messageConsumer-messageProvider.json");
        }

        [TestMethod]
        public async Task ShouldCreateMessagePactAfterVerifyingConsumerAsynchronously()
        {
            var handler = new RecipeAddedHandler();

            var fakePactBrokerMessageHandler = new FakePactBrokerMessageHandler();
            fakePactBrokerMessageHandler.Configure(HttpMethod.Put, "http://localhost:9292").RespondsWith(HttpStatusCode.Created);
            var publisher = new PactPublisher(new HttpClient(fakePactBrokerMessageHandler) { BaseAddress = new Uri("http://localhost:9292") }, "1.0", "local");

            var builder = new MessagePactBuilder("messageConsumer", "messageProvider", publisher);
            var messsageBuilder = await _messageBuilder
                .VerifyConsumerAsync<RecipeAdded>(m => handler.HandleAsync(m));

            await builder.SetUp(messsageBuilder)
                .BuildAsync();

            // Check if handler has been called
            Assert.IsNotNull(handler.ReceivedRecipes.FirstOrDefault());
            Assert.AreEqual("A Recipe", handler.ReceivedRecipes.First().Name);

            // Check if pact has been published and tagged
            Assert.AreEqual("messageConsumer",
                JsonConvert.DeserializeObject<Contract>(fakePactBrokerMessageHandler
                        .GetStatus(HttpMethod.Put, "http://localhost:9292").SentRequestContents.First().Value).Consumer
                    .Name);
            Assert.IsTrue(fakePactBrokerMessageHandler.GetStatus(HttpMethod.Put, "http://localhost:9292").SentRequestContents.Last().Key.Contains("local"));

            // Check if pact has been written to project directory.
            var buildDirectory = AppContext.BaseDirectory;
            var pactDir =
                Path.GetFullPath(
                    $"{buildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}");
            var pactFile = File.ReadAllText(pactDir + "messageConsumer-messageProvider.json");
            Assert.AreEqual("messageConsumer", JsonConvert.DeserializeObject<Contract>(pactFile).Consumer.Name);
            File.Delete(pactDir + "messageConsumer-messageProvider.json");
        }

        [TestMethod]
        public void ShouldBeAbleToGetSerializedMessage()
        {
            string receivedMessage = null;

            var builder = new MessagePactBuilder("messageConsumer", "messageProvider");

            builder.SetUp(_messageBuilder.VerifyConsumer(m => receivedMessage = m));

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
                builder.SetUp(_messageBuilder.VerifyConsumer(m => throw new NullReferenceException()));
            }
            catch (PactException e)
            {
                Assert.AreEqual("Message handler threw an exception", e.Message);
            }
        }
    }
}
