using System.Collections.Generic;

namespace PuppeteerHandler
{
    public class Selectors
    {
        private readonly string[] Values;

        public Selectors(string Id) => Values = new string[1] { "#" + Id };
        private Selectors(string[] Selectors) => this.Values = Selectors;

        public static Selectors FromSelector(string Selector) => new Selectors(new string[1] { Selector });

        public static implicit operator string(Selectors Selector) => Selector.ToString();

        public override string ToString() => string.Join(", ", Values);

        public class Builder
        {
            private readonly List<string> Selectors;
            private readonly List<string> Current;

            public Builder()
            {
                Selectors = new List<string>();
                Current = new List<string>();
            }

            public Builder(string Id)
            {
                Selectors = new List<string>()
                {
                    "#" + Id
                };
                Current = new List<string>();
            }

            public Builder AddClass(string Class) => Add("." + Class);
            public Builder AddId(string Id) => Add("#" + Id);
            public Builder AddElement(string Element) => Add(Element);
            public Builder AddAttribute(string Attribute) => AddAttribute(Attribute, null);
            public Builder AddAttribute(string Attribute, string Value) => Add("[" + Attribute + (Value == null ? "" : "=" + Value) + "]");
            public Builder AddContainsAttribute(string Attribute, string Value) => Add("[" + Attribute + (Value == null ? "" : "~=" + Value) + "]");
            public Builder AddDescendantClass(string Class) => Add(" ." + Class);
            public Builder AddDescendantElement(string Element) => Add(" " + Element);
            public Builder AddDescendantAttribute(string Attribute) => AddDescendantAttribute(Attribute, null);
            public Builder AddDescendantAttribute(string Attribute, string Value) => Add(" [" + Attribute + (Value == null ? "" : "=" + Value) + "]");
            public Builder AddDescendantContainsAttribute(string Attribute, string Value) => Add(" [" + Attribute + (Value == null ? "" : "~=" + Value) + "]");
            public Builder AddChildClass(string Class) => Add(" > ." + Class);
            public Builder AddChildElement(string Element) => Add(" > " + Element);
            public Builder AddChildAttribute(string Attribute) => AddChildAttribute(Attribute, null);
            public Builder AddChildAttribute(string Attribute, string Value) => Add(" > [" + Attribute + (Value == null ? "" : "=" + Value) + "]");
            public Builder AddChildContainsAttribute(string Attribute, string Value) => Add(" > [" + Attribute + (Value == null ? "" : "~=" + Value) + "]");
            public Builder AddSiblingClass(string Class) => Add(" + ." + Class);
            public Builder AddSiblingElement(string Element) => Add(" + " + Element);
            public Builder AddSiblingAttribute(string Attribute) => AddSiblingAttribute(Attribute, null);
            public Builder AddSiblingAttribute(string Attribute, string Value) => Add(" + [" + Attribute + (Value == null ? "" : "=" + Value) + "]");
            public Builder AddSiblingContainsAttribute(string Attribute, string Value) => Add(" + [" + Attribute + (Value == null ? "" : "~=" + Value) + "]");
            public Builder AddPreceedingClass(string Class) => Add(" ~ ." + Class);
            public Builder AddPreceedingElement(string Element) => Add(" ~ " + Element);
            public Builder AddPreceedingAttribute(string Attribute) => AddPreceedingAttribute(Attribute, null);
            public Builder AddPreceedingAttribute(string Attribute, string Value) => Add(" ~ [" + Attribute + (Value == null ? "" : "=" + Value) + "]");
            public Builder AddPreceedingContainsAttribute(string Attribute, string Value) => Add(" ~ [" + Attribute + (Value == null ? "" : "~=" + Value) + "]");
            public Builder Add(string Selector)
            {
                Current.Add(Selector);

                return this;
            }

            public Builder NewSelector(bool IgnoreClear = false)
            {
                string Final = string.Join("", Current);

                Selectors.Add(Final);

                if (!IgnoreClear)
                    ClearSelection();

                return this;
            }

            public Builder ClearSelection()
            {
                Current.Clear();

                return this;
            }

            public Selectors Build()
            {
                string Final = string.Join("", Current);

                Selectors.Add(Final);

                return new Selectors(Selectors.ToArray());
            }
        }
    }
}
