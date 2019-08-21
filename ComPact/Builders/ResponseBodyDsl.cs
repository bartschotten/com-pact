using ComPact.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace ComPact.Builders
{
    public class ResponseBody
    {
        internal Element _rootElement;

        public ResponseBody With(Element element)
        {
            _rootElement = element;
            return this;
        }

        public ResponseBody With(params Member[] members)
        {
            _rootElement = new Object(members);
            return this;
        }

        public ResponseBody Empty()
        {
            _rootElement = new Object();
            return this;
        }

        internal JToken ToJToken()
        {
            return _rootElement.ToJToken();
        }

        internal Dictionary<string, Matcher> CreateMatchingRules()
        {
            var matchingRules = new Dictionary<string, Matcher>();
            _rootElement.AddMatchingRules(matchingRules, "$.body");
            return matchingRules;
        }
    }

    public abstract class Element
    {
        public MatcherList MatcherList { get; set; }

        public Member Named(string name)
        {
            return new Member(name, this);
        }

        internal abstract JToken ToJToken();

        internal virtual void AddMatchingRules(Dictionary<string, Matcher> matchingRules, string path)
        {
            if (MatcherList != null && MatcherList.Matchers.Any())
            {
                matchingRules[path] = MatcherList.Matchers.First();
            }
        }
    }

    public class UnknownSimpleValue
    {
        public SimpleValueName Named(string name) => new SimpleValueName(name);
        public SimpleValue Like(object example) => new SimpleValue(example, MatcherType.type);
        public SimpleValue WithTheExactValue(object example) => new SimpleValue(example, MatcherType.equality);
    }

    public class UnknownRegexString
    {
        public RegexStringName Named(string name) => new RegexStringName(name);
        public RegexString Like(string example, string regex) => new RegexString(example, regex);
        public RegexString LikeGuid(string example) => new RegexString(example, "^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$");
    }

    public class UnknownObject
    {
        public ObjectName Named(string name) => new ObjectName(name);
        public Object With(params Member[] members) => new Object(members);
    }

    public class UnknownArray
    {
        public ArrayName Named(string name) => new ArrayName(name);
        public Array Of(params Element[] elements) => new Array(elements);
    }

    public abstract class MemberName
    {
        public string Name { get; set; }

        public MemberName(string name)
        {
            Name = name;
        }
    }

    public class SimpleValueName : MemberName
    {
        public SimpleValueName(string name) : base(name) { }
        public Member Like(object example) => new Member(Name, new SimpleValue(example, MatcherType.type));
        public Member WithTheExactValue(object example) => new Member(Name, new SimpleValue(example, MatcherType.equality));
    }

    public class RegexStringName : MemberName
    {
        public RegexStringName(string name) : base(name) { }
        public Member Like(string example, string regex) => new Member(Name, new RegexString(example, regex));
        public Member LikeGuid(string example) => new Member(Name, new RegexString(example, "^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$"));
    }

    public class ObjectName : MemberName
    {
        public ObjectName(string name) : base(name) { }
        public Member With(params Member[] members) => new Member(Name, new Object(members));
    }

    public class ArrayName : MemberName
    {
        public ArrayName(string name) : base(name) { }
        public Member Of(params Element[] elements) => new Member(Name, new Array(elements));
    }

    public class Member
    {
        public string Name { get; set; }
        public Element Element { get; set; }

        public Member(string name, Element element)
        {
            Name = name;
            Element = element;
        }

        internal void AddMatchingRules(Dictionary<string, Matcher> matchingRules, string path)
        {
            var extendedPath = path + "." + Name;
            Element.AddMatchingRules(matchingRules, extendedPath);
        }
    }

    public class SimpleValue: Element
    {
        public object Example { get; set; }

        public SimpleValue(object example, MatcherType match)
        {
            Example = example;
            MatcherList = new MatcherList { Matchers = new List<Matcher> { new Matcher { MatcherType = match } } };
        }

        internal override JToken ToJToken()
        {
            return JToken.FromObject(Example);
        }
    }

    public class RegexString : Element
    {
        public string Example { get; set; }

        public RegexString(string example, string regex)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(example, regex))
            {
                throw new PactException($"The provided example {example} does not match the regular expression {regex}.");
            }

            Example = example;
            MatcherList = new MatcherList { Matchers = new List<Matcher> {
                new Matcher { MatcherType = MatcherType.regex, Regex = regex }
            } };
        }

        internal override JToken ToJToken()
        {
            return JToken.FromObject(Example);
        }
    }

    public class Object: Element
    {
        public List<Member> Members { get; set; }

        public Object(params Member[] members)
        {
            Members = members.ToList();
        }

        internal override JToken ToJToken()
        {
            var membersDictionary = new Dictionary<string, JToken>();
            Members.ForEach(m => membersDictionary.Add(m.Name, m.Element.ToJToken()));
            return JToken.FromObject(membersDictionary);
        }

        internal override void AddMatchingRules(Dictionary<string, Matcher> matchingRules, string path)
        {
            base.AddMatchingRules(matchingRules, path);
            Members.ForEach(m => m.AddMatchingRules(matchingRules, path));
        }
    }

    public class Array : Element
    { 
        public Element[] Elements { get; set; }

        public Array(params Element[] elements)
        {
            Elements = elements;
        }

        public Array ContainingAtLeast(uint numberOfElements)
        {
            MatcherList = new MatcherList
            {
                Matchers = new List<Matcher>
                {
                    new Matcher { MatcherType = MatcherType.type, Min = numberOfElements }
                }
            };
            return this;
        }

        internal override JToken ToJToken()
        {
            return JToken.FromObject(Elements.Select(e => e.ToJToken()));
        }

        internal override void AddMatchingRules(Dictionary<string, Matcher> matchingRules, string path)
        {
            base.AddMatchingRules(matchingRules, path);

            for (var i = 0; i < Elements.Length; i++)
            {
                Elements[i].AddMatchingRules(matchingRules, path + "[" + i + "]");
            }
        }
    }
}
