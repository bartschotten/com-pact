# ComPact
An alternative Pact implementation for .NET
# Why another Pact implementation?
1. Most importantly, it's a fun project which allows me to learn a lot about the details of the [Pact Specification](https://github.com/pact-foundation/pact-specification).
2. My impression is that while the idea to reuse the same (Ruby) core logic for almost all Pact implementations has a lot going for it, in practice it adds quite some accidental complexity and as a result the project isn't moving forward as fast as it could. (So in my infinite hubris I've decided to take a shot at making something better.)
3. I think it's healthy for a standard/specification to have more independent implementatations of it.
 # What's Not Supported
 For the foreseeable future, this implementation will not support:
* Specification versions lower than 2.0.
* Data formats other than JSON. The semantics of content-type headers and message metadata will be ignored.
* Example generators.
* "Body is present, but is null"-semantics. Due to the practicalities of .NET, no distiction will be made between a body that is non-existent and one that is null.

Also note that the DSL to define a contract will not allow you to express everything that is valid within the Pact Specification. The goal is not to be complete, but to be simple and user friendly in such a way that it makes the right thing easy to do, and the wrong thing hard.