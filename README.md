</p>
    <a href="https://www.nuget.org/packages/ComPact" alt="https://www.nuget.org/packages/ComPact">
        <img src="https://img.shields.io/nuget/v/ComPact?color=green" /></a>
</p>

# ComPact
An alternative Pact implementation for .NET with support for Pact Specification v3.
## Introduction
### Why another Pact implementation?
1. Most importantly, it's a fun project which allows me to learn a lot about the details of the [Pact Specification](https://github.com/pact-foundation/pact-specification).
2. My impression is that while the idea to reuse the same (Ruby) core logic for almost all Pact implementations has a lot going for it, in practice it adds quite some accidental complexity and as a result the project isn't moving forward as fast as it could. (So in my infinite hubris I've decided to take a shot at making something better.)
3. I think it's healthy for a standard/specification to have more independent implementatations of it.
### What's not supported
This implementation will not support:
* Specification versions lower than 2.0.
* "Body is present, but is null"-semantics. Due to the practicalities of .NET, no distiction will be made between a body that is non-existent and one that is null.
* Matching rules on requests.

In the foreseeable future it might support:
* Data formats other than JSON (i.e. XML). For now the semantics of content-type headers and message metadata will be ignored.
* Example generators.

Also note that the [DSL](#pact-content-dsl) to define a contract will not allow you to express everything that is valid within the Pact Specification. The goal is not to be complete, but to be simple and user friendly in such a way that it makes the right thing easy to do, and the wrong thing hard.

## Usage
### As an API consumer
As the consumer of an API, you'll be the one to generate the Pact contract, so first you have to decide which version of the Pact Specification you want to use. To use V3, start with the following using statement in your test class:
```c#
using ComPact.Builders.V3;
```
Create a builder, and provide the base URL where the builder will create a mock provider service that you can call to verify your consumer code:
```c#
var url = "http://localhost:9393";
var builder = new PactBuilder("test-consumer", "test-provider", url);
```
Set up an interaction, which simply tells the mock provider that when it receives the specified request, it should return the specified response. Notice that the response body is described using a [DSL](#pact-content-dsl) specific for ComPact.
```c#
builder.SetUp(
	Pact.Interaction
        .UponReceiving("a request")
        .With(Pact.Request
            .WithHeader("Accept", "application/json")
            .WithMethod(Method.GET)
            .WithPath("/testpath"))
        .WillRespondWith(Pact.Response
            .WithStatus(200)
            .WithHeader("Content-Type", "application/json")
            .WithBody(Pact.JsonContent.With(Some.Element.WithTheExactValue("test")))));
```
To test the interaction, use your *actual* client code to call the mock provider and check if it can correctly handle the response. If the actual request your code produces cannot be matched to a request that has been set up, the mock provider will return a 400 response with some explanation in the response body.

Set up any number of interactions and verify them. Finally, when you're done with your tests, create the pact contract:
```c#
await builder.BuildAsync();
```
This will verify that all interactions that have been set up have actually been triggered, and will then write a Pact json file to the project directory (by default).

### As an API provider
When you are the provider of an API, create a PactVerifier that will act as a "mock consumer". It will send you requests as specified in a Pact contract and compare the actual response your code produces to the expected response.

Start by providing the PactVerifier with the base url where your actual API can be reached:
```c#
var url = "http://localhost:9494";

var pactVerifier = new PactVerifier(new PactVerifierConfig { ProviderBaseUrl = url });
```
Run your own API in such a way that it can be reached by the mock consumer, for example like this:
```c#
var cts = new CancellationTokenSource();
var hostTask = WebHost.CreateDefaultBuilder()
                .UseKestrel()
                .UseUrls(url)
                .UseStartup<TestStartup>()
                .Build().RunAsync(cts.Token);
```
If you have the Pact file your want to verify stored locally, call VerifyPactAsync with the file path as a parameter. The PactVerifier will throw an exception when the actual response does not match the expected response.
```c#
var buildDirectory = AppContext.BaseDirectory;
var pactDir = Path.GetFullPath($"{buildDirectory}{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}pacts{Path.DirectorySeparatorChar}");
await pactVerifier.VerifyPactAsync(pactDir + "recipe-consumer-recipe-service.json");
```
At the end of your test, clean up your hosted API.
```c#
cts.Cancel();
await hostTask;
```
If the interactions defined in the Pact include Provider States, the PactVerifier will first, for each provider state, invoke a ProviderStateHandler that you should provide to the PactVerifierConfig. In your ProviderStateHandler you should do everything that is necessary to set up the correct test data.

The handler could look something like this:
```c#
public class ProviderStateHandler
    {
        public void Handle(ProviderState providerState)
        {
            ...
```
Which would be then be passed to PactVerifierConfig like this:
```c#
new PactVerifierConfig { ProviderBaseUrl = url, ProviderStateHandler = ProviderStateHandler.Handle }
```

### As a Message consumer
Because Message Pacts are only supported in V3, start with:
```c#
using ComPact.Builders.V3;
```
Create a builder. You don't have to provide a URL because there is no HTTP communication involved:
```c#
var builder = new MessagePactBuilder("test-consumer", "test-provider");
```
Set up a message and verify it by calling VerifyConsumer. You have to provide the VerifyConsumer method with the type of the message you expect as well the code that should handle it. ComPact will only check that the message you set up can actually be deserialized to the provided message type, and that the handling code doesn't throw an exception. Anything else that you want to verify is up to you.
```c#
builder.SetUp(
	Pact.Message
        .Given(new ProviderState { 
            Name = "A new recipe has been added.",
            Params = new Dictionary<string, string> { { "recipeId", "7169de6d-df9b-4cf5-8cdc-2654062e5cdc" } } 
            })
        .ShouldSend("a RecipeAdded event.")
        .With(Pact.JsonContent.With(
            Some.String.Named("eventId").LikeGuid("f84fe18f-d871-4dad-9723-65b6dc9b0578"),
            Some.Object.Named("recipe").With(
                Some.Element.Named("name").Like("A Recipe"),
                Some.Element.Named("instructions").Like("Mix it up"),
                Some.Array.Named("ingredients").InWhichEveryElementIs(ingredient)
            )))
        .VerifyConsumer<RecipeAdded>(m => handler.Handle(m)));
```
Set up any number of messages and verify them. Finally, when you're done with your tests, create the pact contract:
```c#
await builder.BuildAsync();
```

### As a Message provider
Verifying a Message Pact is very similar to verifying an API Pact, but instead of connecting to your API, ComPact will instead invoke a MessageProducer function that you supply in the PactVerifierConfig:
```c#
var config = new PactVerifierConfig
{
    ProviderStateHandler = providerStateHandler.Handle,
    MessageProducer = messageSender.Send
};
```
This function should receive the description of the message as a parameter, and based on that description (and possibly the Provider State) it should return the actual message that your application produces. ComPact will compare this message with the expected message as defined by the contract.

## Pact Broker integration
It is highly recommended that you share Pact contracts using the [Pact Broker](https://github.com/pact-foundation/pact_broker). As a consumer ComPact allows you to publish the contracts you create, and as a provider ComPact allows you to publish the verification results.

To publish contract to the Pact Broker, you should provide a PactPublisher to the PactBuilder. To allow you maximum flexibility how to connect to your own Pact Broker, it's up to you to provide a HttpClient that can be used by ComPact. You should also configure the version of your consumer application and optionally a tag for this version:
```c#
var publisher = new PactPublisher(new HttpClient() { BaseAddress = new Uri("http://your-pact-broker") }, "1.0", "local");

var builder = new MessagePactBuilder("messageConsumer", "messageProvider", publisher);
```
To publish the verification results as a provider, a similar configuration can be added to the PactVerifierConfig:
```c#
var config = new PactVerifierConfig
{
    ProviderVersion = "1.0",
    PublishVerificationResults = true,
    PactBrokerClient = new HttpClient() { BaseAddress = new Uri("http://your-pact-broker")}
};
```
Please note that once you configure a PactBrokerClient, ComPact will no longer try to read Pact files from your local disk, but will instead try to retrieve them from the Pact Broker, so you should provide the path as a parameter to VerifyPactAsync:
```c#
await pactVerifier.VerifyPactAsync("pacts/provider/messageProvider/consumer/messageConsumer/latest");
```

## Pact Content DSL
To describe the contents of a message or the body of a response, ComPact uses a domain specific language via a fluent interface. The purpose of this is to make it easy to create a correct and useful Pact contract.

To define some JSON content with the accompanying matching rules, start by typing `Pact.JsonContent.With(...`

Then describe which elements the content consists of. For example, if the content is just a single string (yes, this is valid JSON) it would look like this: `Some.Element.Like("Hello world!")`. By using `Like()` a *"type" matching rule* will get generated, which means that you consider any string to be a valid response, and "Hello world!" is just an example.

To describe a JSON member (or name-value pair), you can use either `Some.Element.Named("greeting").Like("Hello world!")` or `Some.Element.Like("Hello world!").Named("greeting")`. This will result in the following JSON: `{ "greeting": "Hello world!" }`.

`Pact.JsonContent.With()`                                                                                  | JSON                               | Matching Rules                          
-----------------------------------------------------------------------------------------------------------|------------------------------------|----------------------------
`Some.Element.Like("Hello world!")`                                                                        | `"Hello world"`                    | `$` -> `{ "match": "type" }`
`Some.Element.Named("greeting").Like("Hello world!")`                                                      | `{ "greeting": "Hello world!" }`   | `$.greeting` -> `{ "match": "type" }`
`Some.Element.WithTheExactValue("Hello world!")`                                                           | `"Hello world"`                    | -
`Some.String.LikeRegex("Hello world", "Hello.*")`                                                          | `"Hello world"`                    | `$` -> `{ "match": "regex", "regex": "Hello.*" }`
`Some.String.LikeGuid("f3b5978d-944d-46f2-8663-0c81f27bc4da")`                                             | `"f3b5978d-944d-46f2-8663-0c81f27bc4da"` | `$` -> `{ "match": "regex", "regex": ""[0-9a-fA-F]{8}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{12}"" }`
`Some.Array.Named("anArray") .Of(Some.Element.Like("Hello world"))`                                         | `{ "anArray": [ "Hello world!" ] }` | `$.anArray[0]` -> `{ "match": "type" }`
`Some.Array.Named("anArray") .InWhichEveryElementIs(Some.Element.Like("Hello world"))`                      | `{ "anArray": [ "Hello world!" ] }` | `$.anArray` -> `{ "match": "type", "min": 1 }`, `$.anArray[*]` -> `{ "match": "type" }`
`Some.Array.Named("anArray").ContainingAtLeast(2) .Of(Some.Element.Like("Hello world"))`                    | `{ "anArray": [ "Hello world!" ] }` | `$.anArray` -> `{ "match": "type", "min": 2 }`, `$.anArray[0]` -> `{ "match": "type" }`
`Some.Array.Named("anArray").ContainingAtLeast(2) .InWhichEveryElementIs(Some.Element.Like("Hello world"))` | `{ "anArray": [ "Hello world!" ] }` | `$.anArray` -> `{ "match": "type", "min": 2 }`, `$.anArray[*]` -> `{ "match": "type" }`
`Some.Object.Named("anObject") .With(Some.Element.Named("aNumber").Like(1)))`                               | `{ "anObject": { "aNumber": 1 } }` | `$.anObject.aNumber` -> `{ "match": "type" }` 
`Some.Integer.Like(1)`                                                                                     | `1`                                | `$` -> `{ "match": "integer" }` 
`Some.Decimal.Like(1.1)`                                                                                   | `1.1`                              | `$` -> `{ "match": "decimal" }`
