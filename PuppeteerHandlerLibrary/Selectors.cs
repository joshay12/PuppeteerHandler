using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppeteerHandler
{
    public class Selectors
    {
        private readonly string[] Values;

        public Selectors(string Id) => Values = FromSelector("#" + Id).Values;
        private Selectors(string[] Selectors) => Values = Selectors;

        public static Selectors FromSelector(string Selector) => new Selectors(new string[1] { Selector });

        public static implicit operator string(Selectors Selector) => Selector.ToString();

        public ElementHandle GetElement(Page Page) => GetElementAsync(Page).Result;
        public async Task<ElementHandle> GetElementAsync(Page Page)
        {
            if (Page == null)
            {
                Log.WL($"An error occurred when attempting to retrieve the element of a selector.  The page was NULL!", ConsoleColor.Red);

                return null;
            }

            try
            {
                return await Page.QuerySelectorAsync(this);
            }
            catch (Exception e)
            {
                if (AsyncHandler.DebugMode)
                    Log.WL($"A severe error occurred when attempting to retrieve the element of a selector.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when attempting to retrieve the element of a selector.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return null;
        }

        public string GetContent(Page Page) => GetContentAsync(Page).Result;
        public async Task<string> GetContentAsync(Page Page)
        {
            ElementHandle Element = await GetElementAsync(Page);

            if (Element == null)
            {
                Log.WL($"An error occurred when attempting to retrieve an element's content.  The element was NULL!", ConsoleColor.Red);

                return null;
            }

            try
            {
                return (await Page.EvaluateFunctionAsync("e => e.textContent", Element))?.ToString();
            }
            catch (Exception e)
            {
                if (AsyncHandler.DebugMode)
                    Log.WL($"A severe error occurred when attempting to retrieve an element's content.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when attempting to retrieve an element's content.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return null;
        }

        public string GetAttribute(Page Page, string Name) => GetAttributeAsync(Page, Name).Result;
        public async Task<string> GetAttributeAsync(Page Page, string Name)
        {
            ElementHandle Element = await GetElementAsync(Page);

            if (Element == null)
            {
                Log.WL($"An error occurred when attempting to retrieve an element's attribute.  The element was NULL!", ConsoleColor.Red);

                return null;
            }

            try
            {
                return (await Page.EvaluateFunctionAsync("e => e.getAttribute(\"" + Name + "\")", Element))?.ToString();
            }
            catch (Exception e)
            {
                if (AsyncHandler.DebugMode)
                    Log.WL($"A severe error occurred when attempting to retrieve an element's attribute.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when attempting to retrieve an element's attribute.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return null;
        }

        public void Screenshot(Page Page, string SaveAs) => ScreenshotAsync(Page, SaveAs).GetAwaiter().GetResult();
        public async Task ScreenshotAsync(Page Page, string SaveAs)
        {
            ElementHandle Element = await GetElementAsync(Page);

            if (Element == null)
            {
                Log.WL($"An error occurred when attempting to screenshot an element.  The element was NULL!", ConsoleColor.Red);

                return;
            }

            try
            {
                await Element.ScreenshotAsync(SaveAs);
            }
            catch (Exception e)
            {
                if (AsyncHandler.DebugMode)
                    Log.WL($"A severe error occurred when attempting to screenshot the page.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when attempting to screenshot the page.  Message: " + e.Message, ConsoleColor.DarkRed);
            }
        }

        public override string ToString() => string.Join(", ", Values.Where(x => !string.IsNullOrWhiteSpace(x)));

        public class Builder
        {
            private readonly List<string> Selectors;
            private readonly List<string> Current;

            public Builder()
            {
                Selectors = new List<string>();
                Current = new List<string>();
            }

            [Obsolete("This constructor is deprecated.  Please use \"new Selectors(\"ID\");\" instead.", false)]
            public Builder(string Id)
            {
                Selectors = new string[1] { "#" + Id }.ToList();
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
