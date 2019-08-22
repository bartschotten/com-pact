using ComPact.Builders.V3;
using ComPact.ConsumerTests.Handler;
using ComPact.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace ComPact.ConsumerTests
{
    [TestClass]
    public class MessagePactTests
    {
        [TestMethod]
        public void ShouldCreateMessagePactAfterVerifyingConsumer()
        {
            var ingredient = Some.Object.With(
                                Some.Element.Named("name").Like("Salt"),
                                Some.Element.Named("amount").Like(5.5),
                                Some.Element.Named("unit").Like("gram"));

            var handler = new RecipeAddedHandler();

            var builder = new MessagePactBuilder("messageConsumer", "messageProvider");

            builder.SetupMessage(
                Pact.Message
                .Given(new ProviderState { Name = "A new recipe has been added." })
                .ShouldSend("a RecipeAdded event.")
                .With(Pact.JsonContent.With(
                    Some.String.Named("eventId").LikeGuid("f84fe18f-d871-4dad-9723-65b6dc9b0578"), 
                    Some.Object.Named("recipe").With(
                        Some.Element.Named("name").Like("A Recipe"),
                        Some.Element.Named("instructions").Like("Mix it up"),
                        Some.Array.Named("ingredients").InWhichEveryElementIsLike(ingredient)
                    )))
                .VerifyConsumer<RecipeAdded>(m => handler.Handle(m)))
                .Build();

            Assert.IsNotNull(handler.ReceivedRecipes.FirstOrDefault());
            Assert.AreEqual("A Recipe", handler.ReceivedRecipes.First().Name);
        }
    }
}
