using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace ComPact
{
    public static class PactJsonBody
    {
        public static JToken With(Element element)
        {
            return element.ToJToken();
        }

        public static JToken With(params Member[] members)
        {
            var rootObject = new Object(members);
            return rootObject.ToJToken();
        }
    }

    public static class Some
    {
        public static UnknownSimpleValue Element => new UnknownSimpleValue();
        public static UnknownRegexString String => new UnknownRegexString();
        public static UnknownObject Object => new UnknownObject();
        public static UnknownArray Array => new UnknownArray();
    }

    public abstract class Element
    {
        public Member Named(string name)
        {
            return new Member(name, this);
        }

        public abstract JToken ToJToken();

        public abstract void AddMatchingRules(Dictionary<string, MatchingRule> matchingRules, string path);
    }

    public class UnknownSimpleValue
    {
        public SimpleValueName Named(string name) => new SimpleValueName(name);
        public SimpleValue Like(object example) => new SimpleValue(example, MatchType.Type);
        public SimpleValue WithTheExactValue(object example) => new SimpleValue(example, MatchType.Exact);
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
        public Member Like(object example) => new Member(Name, new SimpleValue(example, MatchType.Type));
        public Member WithTheExactValue(object example) => new Member(Name, new SimpleValue(example, MatchType.Exact));
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

        public void AddMatchingRules(Dictionary<string, MatchingRule> matchingRules, string path)
        {
            var extendedPath = path + "." + Name;
            Element.AddMatchingRules(matchingRules, extendedPath);
        }
    }

    public class SimpleValue: Element
    {
        public object Example { get; set; }
        public MatchType Match { get; set; }

        public SimpleValue(object example, MatchType match)
        {
            Example = example;
            Match = match;
        }

        public override JToken ToJToken()
        {
            return JToken.FromObject(Example);
        }

        public override void AddMatchingRules(Dictionary<string, MatchingRule> matchingRules, string path)
        {
            matchingRules[path] = new MatchingRule { Match = "type" };
        }
    }

    public class RegexString : Element
    {
        public string Example { get; set; }
        public MatchType Match { get; set; }
        public string Regex { get; set; }

        public RegexString(string example, string regex)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(example, regex))
            {
                throw new PactException($"The provided example {example} does not match the regular expression {regex}.");
            }

            Example = example;
            Match = MatchType.Regex;
            Regex = regex;
        }

        public override JToken ToJToken()
        {
            return JToken.FromObject(Example);
        }

        public override void AddMatchingRules(Dictionary<string, MatchingRule> matchingRules, string path)
        {
            matchingRules[path] = new MatchingRule { Match = "regex", Regex = Regex };
        }
    }

    public class Object: Element
    {
        public List<Member> Members { get; set; }

        public Object(params Member[] members)
        {
            Members = members.ToList();
        }

        public override JToken ToJToken()
        {
            var membersDictionary = new Dictionary<string, JToken>();
            Members.ForEach(m => membersDictionary.Add(m.Name, m.Element.ToJToken()));
            return JToken.FromObject(membersDictionary);
        }

        public override void AddMatchingRules(Dictionary<string, MatchingRule> matchingRules, string path)
        {
            Members.ForEach(m => m.AddMatchingRules(matchingRules, path));
        }
    }

    public class Array : Element
    { 
        public Element[] Elements { get; set; }
        public MatchType Match { get; set; }
        public uint Min { get; set; }

        public Array(params Element[] elements)
        {
            Elements = elements;
            Match = MatchType.Exact;
        }

        public Array ContainingAtLeast(uint numberOfElements)
        {
            Match = MatchType.Type;
            Min = numberOfElements;
            return this;
        }

        public override JToken ToJToken()
        {
            return JToken.FromObject(Elements.Select(e => e.ToJToken()));
        }

        public override void AddMatchingRules(Dictionary<string, MatchingRule> matchingRules, string path)
        {
            if (Match == MatchType.Type)
            {
                matchingRules[path] = new MatchingRule { Match = "type", Min = Min };
            }

            for (var i = 0; i < Elements.Length; i++)
            {
                Elements[i].AddMatchingRules(matchingRules, path + "[" + i + "]");
            }
        }
    }

    public enum MatchType
    {
        Exact,
        Type,
        Regex
    }

    public class MatchingRule
    {
        public string Match { get; set; }
        public uint? Min { get; set; }
        public string Regex { get; set; }
    }
}
