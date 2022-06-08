using PuppeteerSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppeteerHandler
{
    public class PageCollection : IEnumerable<Page>
    {
        private int DefaultTimeout;

        private readonly Browser Browser;
        private readonly List<Page> Pages;

        public int Count => Pages.Count;
        public Page this[int Index] => Pages[Index];
        public Page[] this[string Name] => Pages.Where(x => x.GetTitleAsync().Result.Contains(Name)).ToArray();

        public PageCollection(Browser Browser, int DefaultTimeout)
        {
            this.Browser = Browser;
            this.DefaultTimeout = DefaultTimeout;

            Pages = new List<Page>();
        }

        public void OverrideTimeout(int Timeout) => DefaultTimeout = Timeout;

        public async Task Add(string Page, WaitUntilNavigation WaitUntil = WaitUntilNavigation.DOMContentLoaded, string Authentication = null, int Width = 0, int Height = 0)
        {
            try
            {
                Page P = await Browser.NewPageAsync();

                if (!string.IsNullOrWhiteSpace(Authentication))
                    await P.AuthenticateAsync(new Credentials { Username = Authentication.Split(':')[0], Password = Authentication.Split(':')[1] });

                await P.GoToAsync(Page, new NavigationOptions { Timeout = DefaultTimeout, WaitUntil = new WaitUntilNavigation[1] { WaitUntil } });

                await Add(P, Width, Height);
            }
            catch (Exception e)
            {
                if (AsyncHandler.DebugMode)
                    Log.WL($"A severe error occurred when attempting to open a new page.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when attempting to open a new page.  Message: " + e.Message, ConsoleColor.DarkRed);
            }
        }
        public async Task Add(Page Page, int Width = 0, int Height = 0)
        {
            if (Page == null)
            {
                Log.WL($"An error occurred when attempting to open a new page.  The page was NULL!", ConsoleColor.Red);

                return;
            }

            try
            {
                if (Width > 150 && Height > 150)
                    await Page.SetViewportAsync(new ViewPortOptions { Width = Width, Height = Height });

                Pages.Add(Page);
            }
            catch (Exception e)
            {
                if (AsyncHandler.DebugMode)
                    Log.WL($"A severe error occurred when attempting to open a new page.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when attempting to open a new page.  Message: " + e.Message, ConsoleColor.DarkRed);
            }
        }

        public async Task Remove(Page Page)
        {
            try
            {
                if (Pages.Remove(Page))
                    await Page.CloseAsync();
            }
            catch (Exception e)
            {
                if (AsyncHandler.DebugMode)
                    Log.WL($"A severe error occurred when attempting to close a page.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when attempting to close a page.  Message: " + e.Message, ConsoleColor.DarkRed);
            }
        }

        public async Task Clear()
        {
            foreach (Page Page in Pages)
                await Remove(Page);
        }

        public IEnumerator<Page> GetEnumerator() => Pages.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
