using ComPact.Exceptions;
using ComPact.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace ComPact.Builders
{
    public class PactJsonContent
    {
        internal Element _rootElement;

        /// <summary>
        /// Type Some...
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public PactJsonContent With(Element element)
        {
            _rootElement = element;
            return this;
        }

        /// <summary>
        /// Type Some...
        /// </summary>
        /// <param name="members"></param>
        /// <returns></returns>
        public PactJsonContent With(params Member[] members)
        {
            _rootElement = new Object(members);
            return this;
        }

        public PactJsonContent Empty()
        {
            _rootElement = new Object();
            return this;
        }

        internal JToken ToJToken()
        {
            return _rootElement.ToJToken();
        }

        internal Dictionary<string, Matcher> CreateV2MatchingRules()
        {
            var matchingRules = new Dictionary<string, Matcher>();
            _rootElement.AddV2MatchingRules(matchingRules, "$.body");
            if (!matchingRules.Any())
            {
                return null;
            }
            return matchingRules;
        }

        internal Dictionary<string, MatcherList> CreateV3MatchingRules()
        {
            var matchingRules = new Dictionary<string, MatcherList>();
            _rootElement.AddV3MatchingRules(matchingRules, "$");
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

        internal virtual void AddV2MatchingRules(Dictionary<string, Matcher> matchingRules, string path)
        {
            if (MatcherList != null && MatcherList.Matchers.Any(m => m.MatcherType != MatcherType.equality))
            {
                matchingRules[path] = MatcherList.Matchers.First();
            }
        }

        internal virtual void AddV3MatchingRules(Dictionary<string, MatcherList> matchingRules, string path)
        {
            if (MatcherList != null)
            {
                matchingRules[path] = MatcherList;
            }
        }
    }

    public class UnknownSimpleValue
    {
        public SimpleValueName Named(string name) => new SimpleValueName(name);
        public SimpleValue Like(object example) => new SimpleValue(example, MatcherType.type);
        public SimpleValue WithTheExactValue(string example) => new SimpleValue(example, MatcherType.equality);
        public SimpleValue WithTheExactValue(bool example) => new SimpleValue(example, MatcherType.equality);
        public SimpleValue WithTheExactValue(long example) => new SimpleValue(example, MatcherType.equality);
        public SimpleValue WithTheExactValue(decimal example) => new SimpleValue(example, MatcherType.equality);
    }

    public class UnknownString
    {
        public StringName Named(string name) => new StringName(name);
        public SimpleValue Like(string example) => new SimpleValue(example, MatcherType.type);
        public String LikeRegex(string example, string regex) => new String(example, regex);
        public String LikeGuid(string example) => new String(example, "^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$");
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
        public StarArray InWhichEveryElementIs(Element element) => (StarArray)(new StarArray(element).ContainingAtLeast(1));
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
        public Member WithTheExactValue(string example) => new Member(Name, new SimpleValue(example, MatcherType.equality));
        public Member WithTheExactValue(bool example) => new Member(Name, new SimpleValue(example, MatcherType.equality));
        public Member WithTheExactValue(long example) => new Member(Name, new SimpleValue(example, MatcherType.equality));
        public Member WithTheExactValue(decimal example) => new Member(Name, new SimpleValue(example, MatcherType.equality));
    }

    public class StringName : MemberName
    {
        public StringName(string name) : base(name) { }
        public Member Like(string example) => new Member(Name, new SimpleValue(example, MatcherType.type));
        public Member LikeRegex(string example, string regex) => new Member(Name, new String(example, regex));
        public Member LikeGuid(string example) => new Member(Name, new String(example, "^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$"));
    }

    public class ObjectName : MemberName
    {
        public ObjectName(string name) : base(name) { }
        public Member With(params Member[] members) => new Member(Name, new Object(members));
    }

    public class ArrayName : MemberName
    {
        private int Min { get; set; }
        public ArrayName(string name) : base(name) { }
        public Member Of(params Element[] elements) => new Member(Name, new Array(elements).ContainingAtLeast(Min));
        public Member InWhichEveryElementIs(Element element) => new Member(Name, new StarArray(element).ContainingAtLeast(Min != 0 ? Min : 1));
        public ArrayName ContainingAtLeast(int numberOfElements)
        {
            Min = numberOfElements;
            return this;
        }
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

        internal void AddV2MatchingRules(Dictionary<string, Matcher> matchingRules, string path)
        {
            var extendedPath = path + "." + Name;
            Element.AddV2MatchingRules(matchingRules, extendedPath);
        }

        internal void AddV3MatchingRules(Dictionary<string, MatcherList> matchingRules, string path)
        {
            var extendedPath = path + "." + Name;
            Element.AddV3MatchingRules(matchingRules, extendedPath);
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

    public class String : Element
    {
        public string Example { get; set; }

        public String(string example, string regex)
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

        internal override void AddV2MatchingRules(Dictionary<string, Matcher> matchingRules, string path)
        {
            base.AddV2MatchingRules(matchingRules, path);
            Members.ForEach(m => m.AddV2MatchingRules(matchingRules, path));
        }

        internal override void AddV3MatchingRules(Dictionary<string, MatcherList> matchingRules, string path)
        {
            base.AddV3MatchingRules(matchingRules, path);
            Members.ForEach(m => m.AddV3MatchingRules(matchingRules, path));
        }
    }

    public class Array : Element
    { 
        public Element[] Elements { get; set; }

        public Array(params Element[] elements)
        {
            Elements = elements;
        }

        public Array ContainingAtLeast(int numberOfElements)
        {
            if (numberOfElements > 0)
            {
                MatcherList = new MatcherList
                {
                    Matchers = new List<Matcher>
                    {
                        new Matcher { MatcherType = MatcherType.type, Min = numberOfElements }
                    }
                };
            }
            return this;
        }

        internal override JToken ToJToken()
        {
            return JToken.FromObject(Elements.Select(e => e.ToJToken()));
        }

        internal override void AddV2MatchingRules(Dictionary<string, Matcher> matchingRules, string path)
        {
            base.AddV2MatchingRules(matchingRules, path);

            if (!matchingRules.ContainsKey(path + "[*]"))
            {
                for (var i = 0; i < Elements.Length; i++)
                {
                    Elements[i].AddV2MatchingRules(matchingRules, path + "[" + i + "]");
                }
            }
        }

        internal override void AddV3MatchingRules(Dictionary<string, MatcherList> matchingRules, string path)
        {
            base.AddV3MatchingRules(matchingRules, path);

            for (var i = 0; i < Elements.Length; i++)
            {
                Elements[i].AddV3MatchingRules(matchingRules, path + "[" + i + "]");
            }
        }
    }

    public class StarArray : Array
    {
        public StarArray(Element element) : base(element) { }

        internal override void AddV2MatchingRules(Dictionary<string, Matcher> matchingRules, string path)
        {
            matchingRules[path] = MatcherList.Matchers.First();
            Elements[0].AddV2MatchingRules(matchingRules, path + "[*]");
        }

        internal override void AddV3MatchingRules(Dictionary<string, MatcherList> matchingRules, string path)
        {
            matchingRules[path] = MatcherList;
            Elements[0].AddV3MatchingRules(matchingRules, path + "[*]");
        }
    }
}
