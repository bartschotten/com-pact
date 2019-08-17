namespace ComPact.Builders.V2
{
    public static class Pact
    {
        public static RequestBuilder Request => new RequestBuilder();
        public static ResponseBuilder Response => new ResponseBuilder();
        public static ResponseBody ResponseBody => new ResponseBody();
    }
}
