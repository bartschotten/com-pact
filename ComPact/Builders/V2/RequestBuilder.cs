using ComPact.Models;
using ComPact.Models.V2;

namespace ComPact.Builders.V2
{
    public class RequestBuilder
    {
        private readonly Request _request;

        internal RequestBuilder()
        {
            _request = new Request();
        }

        public RequestBuilder ToPath(string path)
        {
            _request.Path = path ?? throw new System.ArgumentNullException(nameof(path));
            return this;
        }

        public RequestBuilder WithMethod(Method method)
        {
            _request.Method = method;
            return this;
        }

        public RequestBuilder WithHeader(string key, string value)
        {
            if (key == null)
            {
                throw new System.ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new System.ArgumentNullException(nameof(value));
            }

            _request.Headers.Add(key, value);
            return this;
        }

        public RequestBuilder WithQuery(string query)
        {
            _request.Query = query ?? throw new System.ArgumentNullException(nameof(query));
            return this;
        }

        public RequestBuilder WithBody(object body)
        {
            _request.Body = body ?? throw new System.ArgumentNullException(nameof(body));
            return this;
        }

        internal Request Build()
        {
            return _request;
        }
    }
}
