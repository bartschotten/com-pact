namespace ComPact.Builders.V2
{
    public static class Pact
    {
        public static InteractionBuilder Interaction => new InteractionBuilder();
        public static RequestBuilder Request => new RequestBuilder();
        public static ResponseBuilder Response => new ResponseBuilder();
        public static PactJsonContent JsonContent => new PactJsonContent();
    }

    public static class Some
    {
        public static UnknownSimpleValue Element => new UnknownSimpleValue();
        public static UnknownString String => new UnknownString();
        public static UnknownObject Object => new UnknownObject();
        public static UnknownArray Array => new UnknownArray();
    }
}
