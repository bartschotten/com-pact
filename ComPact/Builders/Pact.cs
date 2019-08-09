using System;
using System.Collections.Generic;
using System.Text;

namespace ComPact.Builders
{
    public static class Pact
    {
        public static RequestBuilder Request => new RequestBuilder();
        public static ResponseBuilder Response => new ResponseBuilder();
        public static ResponseBody ResponseBody => new ResponseBody();
    }
}
