using PuppeteerSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public async Task Add(string Page, WaitUntilNavigation WaitUntil = WaitUntilNavigation.DOMContentLoaded, string Authentication = null)
        {
            try
            {
                Page P = await Browser.NewPageAsync();

                if (!string.IsNullOrWhiteSpace(Authentication))
                    await P.AuthenticateAsync(new Credentials { Username = Authentication.Split(':')[0], Password = Authentication.Split(':')[1] });

                await P.GoToAsync(Page, new NavigationOptions { Timeout = DefaultTimeout, WaitUntil = new WaitUntilNavigation[1] { WaitUntil } });

                Add(P);
            }
            catch (Exception e)
            {
                if (AsyncHandler.DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when attempting to open a new page.  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when attempting to open a new page.  Message: " + e.Message);
            }
        }
        public void Add(Page Page) => Pages.Add(Page);

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
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when attempting to close a page.  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when attempting to close a page.  Message: " + e.Message);
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
