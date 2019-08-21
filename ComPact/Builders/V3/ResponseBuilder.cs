using ComPact.Models.V3;
using System;

namespace ComPact.Builders.V3
{
    public class ResponseBuilder
    {
        private readonly Response _response;

        internal ResponseBuilder()
        {
            _response = new Response();
        }

        public ResponseBuilder WithStatus(int status)
        {
            _response.Status = status;
            return this;
        }

        public ResponseBuilder WithHeader(string key, string value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            _response.Headers.Add(key, value);
            return this;
        }

        /// <summary>
        /// Type Pact.ResponseBody...
        /// </summary>
        /// <param name="responseBody"></param>
        /// <returns></returns>
        public ResponseBuilder WithBody(ResponseBody responseBody)
        {
            _response.Body = responseBody.ToJToken();
            //_response.MatchingRules.Body = responseBody.CreateMatchingRules();
            return this;
        }

        internal Response Build()
        {
            return _response;
        }
    }
}
