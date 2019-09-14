namespace ComPact.Builders.V3
{
    public static class Pact
    {
        public static InteractionBuilder Interaction => new InteractionBuilder();
        public static RequestBuilder Request => new RequestBuilder();
        public static ResponseBuilder Response => new ResponseBuilder();
        public static MessageBuilder Message => new MessageBuilder();
        public static PactJsonContent JsonContent => new PactJsonContent();
    }

    public static class Some
    {
        public static UnknownSimpleValue Element => new UnknownSimpleValue();
        public static UnknownRegexString String => new UnknownRegexString();
        public static UnknownObject Object => new UnknownObject();
        public static UnknownArray Array => new UnknownArray();
        public static UnknownInteger Integer => new UnknownInteger();
        public static UnknownDecimal Decimal => new UnknownDecimal();
    }
}
