# ComPact
An alternative Pact implementation for .NET
# Why another Pact implementation?
Most importantly, it's a fun project which allows me to learn a lot about the details of the [Pact Specification](https://github.com/pact-foundation/pact-specification).
But also, my impression is that while the idea to reuse the same (Ruby) core logic for almost all Pact implementations has a lot going for it, in practice it adds quite some accidental complexity and as a result the project isn't moving forward as fast as it could. So in my infinite hubris I've decided to take a shot at making something better.
# Goals
The main goal for this project is simplicity. The name "ComPact" is also meant to express this. 

This means:
 1. Simplicity for the maintainers by not trying to support every possible thing in the Pact universe when it comes to generating contracts, but only supporting the full Pact Specification when it comes to verifying contracts (following Postel's law).
 2. Simplicity for the user, by making the right thing easy to do and the wrong thing hard.