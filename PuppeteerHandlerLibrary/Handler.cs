using PuppeteerSharp;
using PuppeteerSharp.Input;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PuppeteerHandler
{
    public sealed class AsyncHandler : IDisposable
    {
        public static bool DebugMode { get; set; } = false;

        private int DefaultTimeout;

        public BrowserFetcher Fetcher { get; }
        public Browser Browser { get; }
        public BrowserContext Context { get; }
        public PageCollection Pages { get; }
        public Page CurrentPage { get; private set; }
        public Selectors CurrentSelector { get; private set; }
        public string Proxy { get; }
        public string Authentication { get; }

        public AsyncHandler(BrowserFetcher Fetcher, Browser Browser, BrowserContext Context, int DefaultTimeout, string Proxy, string Authentication)
        {
            this.Fetcher = Fetcher;
            this.Browser = Browser;
            this.Context = Context;
            this.Proxy = Proxy;
            this.Authentication = Authentication;

            Pages = new PageCollection(this.Browser, DefaultTimeout);
        }

        public AsyncHandler OverrideTimeout(int Timeout)
        {
            Pages.OverrideTimeout(Timeout);

            DefaultTimeout = Timeout;

            return this;
        }

        #region Page Manipulation ASYNC
        public async Task<AsyncHandler> MakeNewPageAsync(string Location, WaitUntilNavigation WaitUntil = WaitUntilNavigation.DOMContentLoaded)
        {
            if (string.IsNullOrWhiteSpace(Location))
                return this;

            await Pages.Add(Location, WaitUntil, Authentication);

            if (CurrentPage == null)
                CurrentPage = Pages.Last();

            return this;
        }
        public async Task<AsyncHandler> MakeNewPageAsync()
        {
            try
            {
                Page Page = await Browser.NewPageAsync();

                if (!string.IsNullOrWhiteSpace(Authentication))
                    await Page.AuthenticateAsync(new Credentials { Username = Authentication.Split(':')[0], Password = Authentication.Split(':')[1] });

                Pages.Add(Page);

                if (CurrentPage == null)
                    CurrentPage = Pages.Last();
            }
            catch (Exception e)
            {
                if (AsyncHandler.DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when attempting to open a new page.  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when attempting to open a new page.  Message: " + e.Message);
            }

            return this;
        }

        public async Task<AsyncHandler> ClosePageAsync() => await ClosePageAsync(CurrentPage);
        public async Task<AsyncHandler> ClosePageAsync(Page Page)
        {
            if (Page == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when attempting to close page.  The page was NULL!");

                return this;
            }

            await Pages.Remove(Page);

            if (Page == CurrentPage)
                CurrentPage = null;

            return this;
        }
        public async Task<AsyncHandler> ClosePagesAsync(string PageTitle) => await ClosePagesAsync(Pages[PageTitle]);
        public async Task<AsyncHandler> ClosePagesAsync(params Page[] Pages)
        {
            if (Pages == null || Pages.Length == 0)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when attempting to close pages.  The pages were NULL, or there were no pages in the array!");

                return this;
            }

            foreach (Page Page in Pages)
                await ClosePageAsync(Page);

            return this;
        }
        #endregion

        #region Page Navigation ASYNC
        public async Task<AsyncHandler> GoToUrlAsync(string Url, WaitUntilNavigation WaitUntil = WaitUntilNavigation.DOMContentLoaded) => await GoToUrlAsync(Url, CurrentPage, WaitUntil);
        public async Task<AsyncHandler> GoToUrlAsync(string Url, Page Page, WaitUntilNavigation WaitUntil = WaitUntilNavigation.DOMContentLoaded)
        {
            if (Page == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when trying to go to the URL.  The page was NULL!");

                return this;
            }

            try
            {
                await Page.GoToAsync(Url, DefaultTimeout, new WaitUntilNavigation[1] { WaitUntil });
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when trying to go to the URL.  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when trying to go to the URL.  Message: " + e.Message);
            }

            return this;
        }
        public async Task<AsyncHandler> BackOnePageAsync(WaitUntilNavigation WaitUntil = WaitUntilNavigation.DOMContentLoaded) => await BackOnePageAsync(CurrentPage, WaitUntil);
        public async Task<AsyncHandler> BackOnePageAsync(Page Page, WaitUntilNavigation WaitUntil = WaitUntilNavigation.DOMContentLoaded)
        {
            if (Page == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when trying to go back a page.  The page was NULL!");

                return this;
            }

            try
            {
                await Page.GoBackAsync(new NavigationOptions { Timeout = DefaultTimeout, WaitUntil = new WaitUntilNavigation[1] { WaitUntil } });
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when trying to go back a page.  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when trying to go back a page.  Message: " + e.Message);
            }

            return this;
        }
        public async Task<AsyncHandler> ForwardOnePageAsync(WaitUntilNavigation WaitUntil = WaitUntilNavigation.DOMContentLoaded) => await ForwardOnePageAsync(CurrentPage, WaitUntil);
        public async Task<AsyncHandler> ForwardOnePageAsync(Page Page, WaitUntilNavigation WaitUntil = WaitUntilNavigation.DOMContentLoaded)
        {
            if (Page == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when trying to go forward a page.  The page was NULL!");

                return this;
            }

            try
            {
                await Page.GoForwardAsync(new NavigationOptions { Timeout = DefaultTimeout, WaitUntil = new WaitUntilNavigation[1] { WaitUntil } });
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when trying to go forward a page.  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when trying to go forward a page.  Message: " + e.Message);
            }

            return this;
        }
        #endregion

        #region Page Waiting ASYNC
        public async Task<AsyncHandler> Wait(int Milliseconds)
        {
            try
            {
                await Task.Delay(Milliseconds);
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when delaying the task.  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when delaying the task.  Message: " + e.Message);
            }

            return this;
        }
        public async Task<AsyncHandler> WaitForDOMLoadAsync() => await WaitForDOMLoadAsync(CurrentPage);
        public async Task<AsyncHandler> WaitForDOMLoadAsync(Page Page)
        {
            if (Page == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when waiting for a page (DOM).  The page was NULL!");

                return this;
            }

            try
            {
                await Page.WaitForNavigationAsync(new NavigationOptions { Timeout = DefaultTimeout, WaitUntil = new WaitUntilNavigation[1] { WaitUntilNavigation.DOMContentLoaded } });
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when waiting for a page (DOM).  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when waiting for a page (DOM).  Message: " + e.Message);
            }

            return this;
        }
        public async Task<AsyncHandler> WaitForFullLoadAsync() => await WaitForFullLoadAsync(CurrentPage);
        public async Task<AsyncHandler> WaitForFullLoadAsync(Page Page)
        {
            if (Page == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when waiting for a page (Load).  The page was NULL!");

                return this;
            }

            try
            {
                await Page.WaitForNavigationAsync(new NavigationOptions { Timeout = DefaultTimeout, WaitUntil = new WaitUntilNavigation[1] { WaitUntilNavigation.Load } });
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when waiting for a page (Load).  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when waiting for a page (Load).  Message: " + e.Message);
            }

            return this;
        }
        public async Task<AsyncHandler> WaitForNetwork0LoadAsync() => await WaitForNetwork0LoadAsync(CurrentPage);
        public async Task<AsyncHandler> WaitForNetwork0LoadAsync(Page Page)
        {
            if (Page == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when waiting for a page (Net0).  The page was NULL!");

                return this;
            }

            try
            {
                await Page.WaitForNavigationAsync(new NavigationOptions { Timeout = DefaultTimeout, WaitUntil = new WaitUntilNavigation[1] { WaitUntilNavigation.Networkidle0 } });
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when waiting for a page (Net0).  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when waiting for a page (Net0).  Message: " + e.Message);
            }

            return this;
        }
        public async Task<AsyncHandler> WaitForNetwork2LoadAsync() => await WaitForNetwork2LoadAsync(CurrentPage);
        public async Task<AsyncHandler> WaitForNetwork2LoadAsync(Page Page)
        {
            if (Page == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when waiting for a page (Net2).  The page was NULL!");

                return this;
            }

            try
            {
                await Page.WaitForNavigationAsync(new NavigationOptions { Timeout = DefaultTimeout, WaitUntil = new WaitUntilNavigation[1] { WaitUntilNavigation.Networkidle2 } });
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when waiting for a page (Net2).  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when waiting for a page (Net2).  Message: " + e.Message);
            }

            return this;
        }
        public async Task<AsyncHandler> WaitForSelectorAsync(Selectors Selector) => await WaitForSelectorAsync(Selector, CurrentPage);
        public async Task<AsyncHandler> WaitForSelectorAsync(Selectors Selector, Page Page)
        {
            if (Page == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when waiting for a selector.  The page was NULL!");

                return this;
            }

            try
            {
                await Page.WaitForSelectorAsync(Selector, new WaitForSelectorOptions { Timeout = DefaultTimeout });
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when waiting for a selector.  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when waiting for a selector.  Message: " + e.Message);
            }

            return this;
        }
        public async Task<AsyncHandler> WaitForFunctionAsync(string Function) => await WaitForFunctionAsync(Function, CurrentPage);
        public async Task<AsyncHandler> WaitForFunctionAsync(string Function, Page Page)
        {
            if (Page == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when waiting for a function.  The page was NULL!");

                return this;
            }

            try
            {
                await Page.WaitForFunctionAsync(Function, new WaitForFunctionOptions { Timeout = DefaultTimeout });
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when waiting for a function.  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when waiting for a function.  Message: " + e.Message);
            }

            return this;
        }
        public async Task<AsyncHandler> WaitForTimeoutAsync(int MillisecondDuration) => await WaitForTimeoutAsync(MillisecondDuration, CurrentPage);
        public async Task<AsyncHandler> WaitForTimeoutAsync(int MillisecondDuration, Page Page)
        {
            if (Page == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when waiting for timeout.  The page was NULL!");

                return this;
            }

            try
            {
                await Page.WaitForTimeoutAsync(MillisecondDuration);
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when waiting for timeout.  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when waiting for timeout.  Message: " + e.Message);
            }

            return this;
        }
        public async Task<AsyncHandler> WaitForExpressionAsync(string Expression) => await WaitForExpressionAsync(Expression, CurrentPage);
        public async Task<AsyncHandler> WaitForExpressionAsync(string Expression, Page Page)
        {
            if (Page == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when waiting for an expression.  The page was NULL!");

                return this;
            }

            try
            {
                await Page.WaitForExpressionAsync(Expression, new WaitForFunctionOptions { Timeout = DefaultTimeout });
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when waiting for an expression.  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when waiting for an expression.  Message: " + e.Message);
            }

            return this;
        }
        #endregion

        #region Page Input ASYNC
        public async Task<AsyncHandler> ClickPageAsync() => await ClickPageAsync(CurrentSelector, MouseButton.Left, 0, 1, CurrentPage);
        public async Task<AsyncHandler> ClickPageAsync(Selectors Selector) => await ClickPageAsync(Selector, MouseButton.Left, 0, 1, CurrentPage);
        public async Task<AsyncHandler> ClickPageAsync(Page Page) => await ClickPageAsync(CurrentSelector, MouseButton.Left, 0, 1, Page);
        public async Task<AsyncHandler> ClickPageAsync(Selectors Selector, Page Page) => await ClickPageAsync(Selector, MouseButton.Left, 0, 1, Page);
        public async Task<AsyncHandler> ClickPageAsync(MouseButton Button) => await ClickPageAsync(CurrentSelector, Button, 0, 1, CurrentPage);
        public async Task<AsyncHandler> ClickPageAsync(Selectors Selector, MouseButton Button) => await ClickPageAsync(Selector, Button, 0, 1, CurrentPage);
        public async Task<AsyncHandler> ClickPageAsync(MouseButton Button, Page Page) => await ClickPageAsync(CurrentSelector, Button, 0, 1, Page);
        public async Task<AsyncHandler> ClickPageAsync(Selectors Selector, MouseButton Button, Page Page) => await ClickPageAsync(Selector, Button, 0, 1, Page);
        public async Task<AsyncHandler> ClickPageAsync(MouseButton Button, int Delay) => await ClickPageAsync(CurrentSelector, Button, Delay, 1, CurrentPage);
        public async Task<AsyncHandler> ClickPageAsync(Selectors Selector, MouseButton Button, int Delay) => await ClickPageAsync(Selector, Button, Delay, 1, CurrentPage);
        public async Task<AsyncHandler> ClickPageAsync(MouseButton Button, int Delay, Page Page) => await ClickPageAsync(CurrentSelector, Button, Delay, 1, Page);
        public async Task<AsyncHandler> ClickPageAsync(Selectors Selector, MouseButton Button, int Delay, Page Page) => await ClickPageAsync(Selector, Button, Delay, 1, Page);
        public async Task<AsyncHandler> ClickPageAsync(MouseButton Button, int Delay, int ClickAmount) => await ClickPageAsync(CurrentSelector, Button, Delay, ClickAmount, CurrentPage);
        public async Task<AsyncHandler> ClickPageAsync(Selectors Selector, MouseButton Button, int Delay, int ClickAmount) => await ClickPageAsync(Selector, Button, Delay, ClickAmount, CurrentPage);
        public async Task<AsyncHandler> ClickPageAsync(MouseButton Button, int Delay, int ClickAmount, Page Page) => await ClickPageAsync(CurrentSelector, Button, Delay, ClickAmount, Page);
        public async Task<AsyncHandler> ClickPageAsync(Selectors Selector, MouseButton Button, int Delay, int ClickAmount, Page Page)
        {
            if (Page == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when attempting to click the page.  The page was NULL!");

                return this;
            }

            if (Selector == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when attempting to click the page.  The selector was NULL!");

                return this;
            }

            try
            {
                await Page.ClickAsync(Selector, new ClickOptions { Button = Button, ClickCount = ClickAmount, Delay = Delay });
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when attempting to click the page.  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when attempting to click the page.  Message: " + e.Message);
            }

            return this;
        }

        public async Task<AsyncHandler> TypeAsync(object Message) => await TypeAsync(CurrentSelector, Message, 0, CurrentPage);
        public async Task<AsyncHandler> TypeAsync(Selectors Selector, object Message) => await TypeAsync(Selector, Message, 0, CurrentPage);
        public async Task<AsyncHandler> TypeAsync(object Message, Page Page) => await TypeAsync(CurrentSelector, Message, 0, Page);
        public async Task<AsyncHandler> TypeAsync(Selectors Selector, object Message, Page Page) => await TypeAsync(Selector, Message, 0, Page);
        public async Task<AsyncHandler> TypeAsync(object Message, int Delay) => await TypeAsync(CurrentSelector, Message, Delay, CurrentPage);
        public async Task<AsyncHandler> TypeAsync(Selectors Selector, object Message, int Delay) => await TypeAsync(Selector, Message, Delay, CurrentPage);
        public async Task<AsyncHandler> TypeAsync(object Message, int Delay, Page Page) => await TypeAsync(CurrentSelector, Message, Delay, Page);
        public async Task<AsyncHandler> TypeAsync(Selectors Selector, object Message, int Delay, Page Page)
        {
            if (Page == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when attempting to type on the page.  The page was NULL!");

                return this;
            }

            if (Selector == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when attempting to type on the page.  The selector was NULL!");

                return this;
            }

            try
            {
                if (Message == null)
                    Message = "NULL";

                await Page.TypeAsync(Selector, Message.ToString(), new TypeOptions { Delay = Delay });
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when attempting to type on the page.  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when attempting to type on the page.  Message: " + e.Message);
            }

            return this;
        }

        public async Task<AsyncHandler> HoverAsync() => await HoverAsync(CurrentSelector, CurrentPage);
        public async Task<AsyncHandler> HoverAsync(Selectors Selector) => await HoverAsync(Selector, CurrentPage);
        public async Task<AsyncHandler> HoverAsync(Page Page) => await HoverAsync(CurrentSelector, Page);
        public async Task<AsyncHandler> HoverAsync(Selectors Selector, Page Page)
        {
            if (Page == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when attempting to hover an element on the page.  The page was NULL!");

                return this;
            }

            if (Selector == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when attempting to hover an element on the page.  The selector was NULL!");

                return this;
            }

            try
            {
                await Page.HoverAsync(Selector);
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when attempting to hover an element on the page.  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when attempting to hover an element on the page.  Message: " + e.Message);
            }

            return this;
        }
        #endregion

        public async Task PauseAsync() => await Task.Delay(-1);

        public async Task<string> RetrieveContentAsync() => await RetrieveContentAsync(CurrentPage);
        public async Task<string> RetrieveContentAsync(Page Page)
        {
            if (Page == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when attempting to retrieve the page's content.  The page was NULL!");

                return null;
            }

            try
            {
                return await Page.GetContentAsync();
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when attempting to hover an element on the page.  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when attempting to hover an element on the page.  Message: " + e.Message);

                return null;
            }
        }

        public AsyncHandler SetSelector(string Id) => SetSelector(new Selectors(Id));
        public AsyncHandler SetSelector(Selectors Selector)
        {
            CurrentSelector = Selector;

            return this;
        }

        public AsyncHandler ChoosePage(int Index) => ChoosePage(Pages[Index]);
        public AsyncHandler ChoosePage(string PageTitle, int Index) => ChoosePage(Pages[PageTitle][Index]);
        public AsyncHandler ChoosePage(Page Page)
        {
            CurrentPage = Page;

            return this;
        }

        public async Task CloseBrowserAsync()
        {
            try
            {
                await Browser.CloseAsync();
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when attempting to close the browser.  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when attempting to close the browser.  Message: " + e.Message);
            }

            Dispose();
        }

        public Handler ToHandler() => new Handler(Fetcher, Browser, Context, DefaultTimeout, Proxy, Authentication);

        public void Dispose()
        {
            for (int i = 0; i < Pages.Count; i++)
                Pages[i].CloseAsync().RunSynchronously();

            if (Browser != null)
                Browser.Dispose();

            Fetcher.Dispose();
        }

        public sealed class Builder
        {
            private readonly int DefaultTimeout;
            private readonly BrowserFetcher Fetcher;
            private string Proxy;
            private string Authentication;
            private Browser Browser;
            private BrowserContext Context;

            public Builder(int DefaultTimeout = 30000, Product Browser = Product.Chrome)
            {
                Fetcher = new BrowserFetcher(new BrowserFetcherOptions { Product = Browser });

                this.DefaultTimeout = DefaultTimeout;
            }

            public Builder SetBrowserProxy(ProxyRotation Proxies)
            {
                if (Proxies == null || Proxies.Count == 0)
                    return this;

                Proxy = Proxies.Next();
                Authentication = Proxies[Proxy];

                return this;
            }

            public async Task<Builder> CreateBrowserAsync(bool Headless = false, bool Incognito = false)
            {
                try
                {
                    LaunchOptions Options = new LaunchOptions
                    {
                        Headless = Headless,
                        Args = !string.IsNullOrWhiteSpace(Proxy) ? new string[1] { "--proxy-server=" + Proxy } : new string[0]
                    };

                    await Fetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);

                    Browser = await Puppeteer.LaunchAsync(Options);

                    if (Incognito)
                        Context = await Browser.CreateIncognitoBrowserContextAsync();
                    else
                        Context = Browser.DefaultContext;
                }
                catch (Exception e)
                {
                    if (DebugMode)
                        Log.WL($"{ ChatColor.Red }A severe error occurred when attempting to create a new browser.  Stacktrace: " + e);
                    else
                        Log.WL($"{ ChatColor.Red }A severe error occurred when attempting to create a new browser.  Message: " + e.Message);
                }

                return this;
            }

            public Handler Build() => new Handler(Fetcher, Browser, Context, DefaultTimeout, Proxy, Authentication);
            public AsyncHandler BuildAsync() => new AsyncHandler(Fetcher, Browser, Context, DefaultTimeout, Proxy, Authentication);
        }
    }

    public sealed class Handler : IDisposable
    {
        public static bool DebugMode { get; set; } = false;

        private int DefaultTimeout;

        public BrowserFetcher Fetcher { get; }
        public Browser Browser { get; }
        public BrowserContext Context { get; }
        public PageCollection Pages { get; }
        public Page CurrentPage { get; private set; }
        public Selectors CurrentSelector { get; private set; }
        public string Proxy { get; }
        public string Authentication { get; }

        public Handler(BrowserFetcher Fetcher, Browser Browser, BrowserContext Context, int DefaultTimeout, string Proxy, string Authentication)
        {
            this.Fetcher = Fetcher;
            this.Browser = Browser;
            this.Context = Context;
            this.Proxy = Proxy;
            this.Authentication = Authentication;

            Pages = new PageCollection(this.Browser, DefaultTimeout);
        }

        public Handler OverrideTimeout(int Timeout)
        {
            Pages.OverrideTimeout(Timeout);

            DefaultTimeout = Timeout;

            return this;
        }

        #region Page Manipulation
        public Handler MakeNewPage(string Location, WaitUntilNavigation WaitUntil = WaitUntilNavigation.DOMContentLoaded)
        {
            if (string.IsNullOrWhiteSpace(Location))
                return this;

            Pages.Add(Location, WaitUntil, Authentication).GetAwaiter().GetResult();

            if (CurrentPage == null)
                CurrentPage = Pages.Last();

            return this;
        }
        public Handler MakeNewPage()
        {
            try
            {
                Page Page = Browser.NewPageAsync().Result;

                if (!string.IsNullOrWhiteSpace(Authentication))
                    Page.AuthenticateAsync(new Credentials { Username = Authentication.Split(':')[0], Password = Authentication.Split(':')[1] }).GetAwaiter().GetResult();

                Pages.Add(Page);

                if (CurrentPage == null)
                    CurrentPage = Pages.Last();
            }
            catch (Exception e)
            {
                if (AsyncHandler.DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when attempting to open a new page.  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when attempting to open a new page.  Message: " + e.Message);
            }

            return this;
        }

        public Handler ClosePage() => ClosePage(CurrentPage);
        public Handler ClosePage(Page Page)
        {
            if (Page == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when attempting to close page.  The page was NULL!");

                return this;
            }

            Pages.Remove(Page).GetAwaiter().GetResult();

            if (Page == CurrentPage)
                CurrentPage = null;

            return this;
        }
        public Handler ClosePages(string PageTitle) => ClosePages(Pages[PageTitle]);
        public Handler ClosePages(params Page[] Pages)
        {
            if (Pages == null || Pages.Length == 0)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when attempting to close pages.  The pages were NULL, or there were no pages in the array!");

                return this;
            }

            foreach (Page Page in Pages)
                ClosePage(Page);

            return this;
        }
        #endregion

        #region Page Navigation
        public Handler GoToUrl(string Url, WaitUntilNavigation WaitUntil = WaitUntilNavigation.DOMContentLoaded) => GoToUrl(Url, CurrentPage, WaitUntil);
        public Handler GoToUrl(string Url, Page Page, WaitUntilNavigation WaitUntil = WaitUntilNavigation.DOMContentLoaded)
        {
            if (Page == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when trying to go to the URL.  The page was NULL!");

                return this;
            }

            try
            {
                _ = Page.GoToAsync(Url, DefaultTimeout, new WaitUntilNavigation[1] { WaitUntil }).Result;
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when trying to go to the URL.  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when trying to go to the URL.  Message: " + e.Message);
            }

            return this;
        }
        public Handler BackOnePage(WaitUntilNavigation WaitUntil = WaitUntilNavigation.DOMContentLoaded) => BackOnePage(CurrentPage, WaitUntil);
        public Handler BackOnePage(Page Page, WaitUntilNavigation WaitUntil = WaitUntilNavigation.DOMContentLoaded)
        {
            if (Page == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when trying to go back a page.  The page was NULL!");

                return this;
            }

            try
            {
                _ = Page.GoBackAsync(new NavigationOptions { Timeout = DefaultTimeout, WaitUntil = new WaitUntilNavigation[1] { WaitUntil } }).Result;
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when trying to go back a page.  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when trying to go back a page.  Message: " + e.Message);
            }

            return this;
        }
        public Handler ForwardOnePage(WaitUntilNavigation WaitUntil = WaitUntilNavigation.DOMContentLoaded) => ForwardOnePage(CurrentPage, WaitUntil);
        public Handler ForwardOnePage(Page Page, WaitUntilNavigation WaitUntil = WaitUntilNavigation.DOMContentLoaded)
        {
            if (Page == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when trying to go forward a page.  The page was NULL!");

                return this;
            }

            try
            {
                _ = Page.GoForwardAsync(new NavigationOptions { Timeout = DefaultTimeout, WaitUntil = new WaitUntilNavigation[1] { WaitUntil } }).Result;
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when trying to go forward a page.  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when trying to go forward a page.  Message: " + e.Message);
            }

            return this;
        }
        #endregion

        #region Page Waiting ASYNC
        public Handler Wait(int Milliseconds)
        {
            try
            {
                Task.Delay(Milliseconds).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when delaying the task.  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when delaying the task.  Message: " + e.Message);
            }

            return this;
        }
        public Handler WaitForDOMLoad() => WaitForDOMLoad(CurrentPage);
        public Handler WaitForDOMLoad(Page Page)
        {
            if (Page == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when waiting for a page (DOM).  The page was NULL!");

                return this;
            }

            try
            {
                _ = Page.WaitForNavigationAsync(new NavigationOptions { Timeout = DefaultTimeout, WaitUntil = new WaitUntilNavigation[1] { WaitUntilNavigation.DOMContentLoaded } }).Result;
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when waiting for a page (DOM).  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when waiting for a page (DOM).  Message: " + e.Message);
            }

            return this;
        }
        public Handler WaitForFullLoad() => WaitForFullLoad(CurrentPage);
        public Handler WaitForFullLoad(Page Page)
        {
            if (Page == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when waiting for a page (Load).  The page was NULL!");

                return this;
            }

            try
            {
                _ = Page.WaitForNavigationAsync(new NavigationOptions { Timeout = DefaultTimeout, WaitUntil = new WaitUntilNavigation[1] { WaitUntilNavigation.Load } }).Result;
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when waiting for a page (Load).  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when waiting for a page (Load).  Message: " + e.Message);
            }

            return this;
        }
        public Handler WaitForNetwork0Load() => WaitForNetwork0Load(CurrentPage);
        public Handler WaitForNetwork0Load(Page Page)
        {
            if (Page == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when waiting for a page (Net0).  The page was NULL!");

                return this;
            }

            try
            {
                _ = Page.WaitForNavigationAsync(new NavigationOptions { Timeout = DefaultTimeout, WaitUntil = new WaitUntilNavigation[1] { WaitUntilNavigation.Networkidle0 } }).Result;
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when waiting for a page (Net0).  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when waiting for a page (Net0).  Message: " + e.Message);
            }

            return this;
        }
        public Handler WaitForNetwork2Load() => WaitForNetwork2Load(CurrentPage);
        public Handler WaitForNetwork2Load(Page Page)
        {
            if (Page == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when waiting for a page (Net2).  The page was NULL!");

                return this;
            }

            try
            {
                _ = Page.WaitForNavigationAsync(new NavigationOptions { Timeout = DefaultTimeout, WaitUntil = new WaitUntilNavigation[1] { WaitUntilNavigation.Networkidle2 } }).Result;
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when waiting for a page (Net2).  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when waiting for a page (Net2).  Message: " + e.Message);
            }

            return this;
        }
        public Handler WaitForSelector(Selectors Selector) => WaitForSelector(Selector, CurrentPage);
        public Handler WaitForSelector(Selectors Selector, Page Page)
        {
            if (Page == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when waiting for a selector.  The page was NULL!");

                return this;
            }

            try
            {
                _ = Page.WaitForSelectorAsync(Selector, new WaitForSelectorOptions { Timeout = DefaultTimeout }).Result;
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when waiting for a selector.  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when waiting for a selector.  Message: " + e.Message);
            }

            return this;
        }
        public Handler WaitForFunction(string Function) => WaitForFunction(Function, CurrentPage);
        public Handler WaitForFunction(string Function, Page Page)
        {
            if (Page == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when waiting for a function.  The page was NULL!");

                return this;
            }

            try
            {
                _ = Page.WaitForFunctionAsync(Function, new WaitForFunctionOptions { Timeout = DefaultTimeout }).Result;
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when waiting for a function.  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when waiting for a function.  Message: " + e.Message);
            }

            return this;
        }
        public Handler WaitForTimeout(int MillisecondDuration) => WaitForTimeout(MillisecondDuration, CurrentPage);
        public Handler WaitForTimeout(int MillisecondDuration, Page Page)
        {
            if (Page == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when waiting for timeout.  The page was NULL!");

                return this;
            }

            try
            {
                Page.WaitForTimeoutAsync(MillisecondDuration).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when waiting for timeout.  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when waiting for timeout.  Message: " + e.Message);
            }

            return this;
        }
        public Handler WaitForExpression(string Expression) => WaitForExpression(Expression, CurrentPage);
        public Handler WaitForExpression(string Expression, Page Page)
        {
            if (Page == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when waiting for an expression.  The page was NULL!");

                return this;
            }

            try
            {
                _ = Page.WaitForExpressionAsync(Expression, new WaitForFunctionOptions { Timeout = DefaultTimeout }).Result;
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when waiting for an expression.  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when waiting for an expression.  Message: " + e.Message);
            }

            return this;
        }
        #endregion

        #region Page Input ASYNC
        public Handler ClickPage() => ClickPage(CurrentSelector, MouseButton.Left, 0, 1, CurrentPage);
        public Handler ClickPage(Selectors Selector) => ClickPage(Selector, MouseButton.Left, 0, 1, CurrentPage);
        public Handler ClickPage(Page Page) => ClickPage(CurrentSelector, MouseButton.Left, 0, 1, Page);
        public Handler ClickPage(Selectors Selector, Page Page) => ClickPage(Selector, MouseButton.Left, 0, 1, Page);
        public Handler ClickPage(MouseButton Button) => ClickPage(CurrentSelector, Button, 0, 1, CurrentPage);
        public Handler ClickPage(Selectors Selector, MouseButton Button) => ClickPage(Selector, Button, 0, 1, CurrentPage);
        public Handler ClickPage(MouseButton Button, Page Page) => ClickPage(CurrentSelector, Button, 0, 1, Page);
        public Handler ClickPage(Selectors Selector, MouseButton Button, Page Page) => ClickPage(Selector, Button, 0, 1, Page);
        public Handler ClickPage(MouseButton Button, int Delay) => ClickPage(CurrentSelector, Button, Delay, 1, CurrentPage);
        public Handler ClickPage(Selectors Selector, MouseButton Button, int Delay) => ClickPage(Selector, Button, Delay, 1, CurrentPage);
        public Handler ClickPage(MouseButton Button, int Delay, Page Page) => ClickPage(CurrentSelector, Button, Delay, 1, Page);
        public Handler ClickPage(Selectors Selector, MouseButton Button, int Delay, Page Page) => ClickPage(Selector, Button, Delay, 1, Page);
        public Handler ClickPage(MouseButton Button, int Delay, int ClickAmount) => ClickPage(CurrentSelector, Button, Delay, ClickAmount, CurrentPage);
        public Handler ClickPage(Selectors Selector, MouseButton Button, int Delay, int ClickAmount) => ClickPage(Selector, Button, Delay, ClickAmount, CurrentPage);
        public Handler ClickPage(MouseButton Button, int Delay, int ClickAmount, Page Page) => ClickPage(CurrentSelector, Button, Delay, ClickAmount, Page);
        public Handler ClickPage(Selectors Selector, MouseButton Button, int Delay, int ClickAmount, Page Page)
        {
            if (Page == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when attempting to click the page.  The page was NULL!");

                return this;
            }

            if (Selector == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when attempting to click the page.  The selector was NULL!");

                return this;
            }

            try
            {
                Page.ClickAsync(Selector, new ClickOptions { Button = Button, ClickCount = ClickAmount, Delay = Delay }).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when attempting to click the page.  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when attempting to click the page.  Message: " + e.Message);
            }

            return this;
        }

        public Handler Type(object Message) => Type(CurrentSelector, Message, 0, CurrentPage);
        public Handler Type(Selectors Selector, object Message) => Type(Selector, Message, 0, CurrentPage);
        public Handler Type(object Message, Page Page) => Type(CurrentSelector, Message, 0, Page);
        public Handler Type(Selectors Selector, object Message, Page Page) => Type(Selector, Message, 0, Page);
        public Handler Type(object Message, int Delay) => Type(CurrentSelector, Message, Delay, CurrentPage);
        public Handler Type(Selectors Selector, object Message, int Delay) => Type(Selector, Message, Delay, CurrentPage);
        public Handler Type(object Message, int Delay, Page Page) => Type(CurrentSelector, Message, Delay, Page);
        public Handler Type(Selectors Selector, object Message, int Delay, Page Page)
        {
            if (Page == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when attempting to type on the page.  The page was NULL!");

                return this;
            }

            if (Selector == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when attempting to type on the page.  The selector was NULL!");

                return this;
            }

            try
            {
                if (Message == null)
                    Message = "NULL";

                Page.TypeAsync(Selector, Message.ToString(), new TypeOptions { Delay = Delay }).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when attempting to type on the page.  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when attempting to type on the page.  Message: " + e.Message);
            }

            return this;
        }

        public Handler Hover() => Hover(CurrentSelector, CurrentPage);
        public Handler Hover(Selectors Selector) => Hover(Selector, CurrentPage);
        public Handler Hover(Page Page) => Hover(CurrentSelector, Page);
        public Handler Hover(Selectors Selector, Page Page)
        {
            if (Page == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when attempting to hover an element on the page.  The page was NULL!");

                return this;
            }

            if (Selector == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when attempting to hover an element on the page.  The selector was NULL!");

                return this;
            }

            try
            {
                Page.HoverAsync(Selector).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when attempting to hover an element on the page.  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when attempting to hover an element on the page.  Message: " + e.Message);
            }

            return this;
        }
        #endregion

        public void Pause() => Task.Delay(-1).GetAwaiter().GetResult();

        public string RetrieveContent() => RetrieveContent(CurrentPage);
        public string RetrieveContent(Page Page)
        {
            if (Page == null)
            {
                Log.WL($"{ ChatColor.Red }An error occurred when attempting to retrieve the page's content.  The page was NULL!");

                return null;
            }

            try
            {
                return Page.GetContentAsync().Result;
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when attempting to hover an element on the page.  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when attempting to hover an element on the page.  Message: " + e.Message);

                return null;
            }
        }

        public Handler SetSelector(string Id) => SetSelector(new Selectors(Id));
        public Handler SetSelector(Selectors Selector)
        {
            CurrentSelector = Selector;

            return this;
        }

        public Handler ChoosePage(int Index) => ChoosePage(Pages[Index]);
        public Handler ChoosePage(string PageTitle, int Index) => ChoosePage(Pages[PageTitle][Index]);
        public Handler ChoosePage(Page Page)
        {
            CurrentPage = Page;

            return this;
        }

        public void CloseBrowser()
        {
            try
            {
                Browser.CloseAsync().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                if (DebugMode)
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when attempting to close the browser.  Stacktrace: " + e);
                else
                    Log.WL($"{ ChatColor.DarkRed }A severe error occurred when attempting to close the browser.  Message: " + e.Message);
            }

            Dispose();
        }

        public AsyncHandler ToAsyncHandler() => new AsyncHandler(Fetcher, Browser, Context, DefaultTimeout, Proxy, Authentication);

        public void Dispose()
        {
            for (int i = 0; i < Pages.Count; i++)
                Pages[i].CloseAsync().GetAwaiter().GetResult();

            if (Browser != null)
                Browser.Dispose();

            Fetcher.Dispose();
        }

        public sealed class Builder
        {
            private readonly int DefaultTimeout;
            private readonly BrowserFetcher Fetcher;
            private Browser Browser;
            private BrowserContext Context;
            private string Proxy;
            private string Authentication;

            public Builder(int DefaultTimeout = 30000, Product Browser = Product.Chrome)
            {
                Fetcher = new BrowserFetcher(new BrowserFetcherOptions { Product = Browser });

                this.DefaultTimeout = DefaultTimeout;
            }

            public Builder SetBrowserProxy(ProxyRotation Proxies)
            {
                if (Proxies == null || Proxies.Count == 0)
                    return this;

                Proxy = Proxies.Next();
                Authentication = Proxies[Proxy];

                return this;
            }

            public Builder CreateBrowser(bool Headless = false, bool Incognito = false)
            {
                try
                {
                    LaunchOptions Options = new LaunchOptions
                    {
                        Headless = Headless,
                        Args = !string.IsNullOrWhiteSpace(Proxy) ? new string[1] { "--proxy-server=" + Proxy } : new string[0]
                    };

                    _ = Fetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision).Result;

                    Browser = Puppeteer.LaunchAsync(Options).Result;

                    if (Incognito)
                        Context = Browser.CreateIncognitoBrowserContextAsync().Result;
                    else
                        Context = Browser.DefaultContext;
                }
                catch (Exception e)
                {
                    if (DebugMode)
                        Log.WL($"{ ChatColor.Red }A severe error occurred when attempting to create a new browser.  Stacktrace: " + e);
                    else
                        Log.WL($"{ ChatColor.Red }A severe error occurred when attempting to create a new browser.  Message: " + e.Message);
                }

                return this;
            }

            public Handler Build() => new Handler(Fetcher, Browser, Context, DefaultTimeout, Proxy, Authentication);
            public AsyncHandler BuildAsync() => new AsyncHandler(Fetcher, Browser, Context, DefaultTimeout, Proxy, Authentication);
        }
    }
}
