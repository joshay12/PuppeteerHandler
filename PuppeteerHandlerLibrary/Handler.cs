using PuppeteerSharp;
using PuppeteerSharp.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PuppeteerHandler
{
    public enum AsyncMethods
    {
        MakeNewPageAsync,
        GoToUrlAsync,
        BackOnePageAsync,
        ForwardOnePageAsync,
        WaitAsync,
        WaitForDOMLoadAsync,
        WaitForFullLoadAsync,
        WaitForNetwork0LoadAsync,
        WaitForNetwork2LoadAsync,
        WaitForSelectorAsync,
        WaitForFunctionAsync,
        WaitForTimeoutAsync,
        WaitForExpressionAsync,
        ClickPageAsync,
        TypeAsync,
        HoverAsync,
        ClearFieldAsync,
        AddDialogEvent,
        SolveCaptchaImageAsync,
        ScreenshotAsync,
        DownloadMediaAsync,
        RetrieveContentAsync,
        CloseBrowserAsync
    }

    public enum Methods
    {
        MakeNewPage,
        GoToUrl,
        BackOnePage,
        ForwardOnePage,
        Wait,
        WaitForDOMLoad,
        WaitForFullLoad,
        WaitForNetwork0Load,
        WaitForNetwork2Load,
        WaitForSelector,
        WaitForFunction,
        WaitForTimeout,
        WaitForExpression,
        ClickPage,
        Type,
        Hover,
        ClearField,
        AddDialogEvent,
        SolveImageCaptcha,
        Screenshot,
        DownloadMedia,
        RetrieveContent,
        CloseBrowser
    }

    public sealed class AsyncHandler : IDisposable
    {
        public delegate void ExceptionThrownEvent(Exception Exception, AsyncMethods Method, params object[] MethodParameters);
        public event ExceptionThrownEvent ExceptionThrown;

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
        public string CaptchaKey { get; }
        public int Width { get; }
        public int Height { get; }

        public AsyncHandler(BrowserFetcher Fetcher, Browser Browser, BrowserContext Context, int DefaultTimeout, string Proxy, string Authentication, string CaptchaKey, int Width, int Height)
        {
            this.Fetcher = Fetcher;
            this.Browser = Browser;
            this.Context = Context;
            this.Proxy = Proxy;
            this.Authentication = Authentication;
            this.CaptchaKey = CaptchaKey;
            this.Width = Width;
            this.Height = Height;

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

            try
            {
                await Pages.Add(Location, WaitUntil, Authentication, Width, Height);
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, AsyncMethods.MakeNewPageAsync, Location, WaitUntil);

                if (DebugMode)
                    Log.WL($"A severe error occurred when attempting to open a new page.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when attempting to open a new page.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

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

                await Pages.Add(Page, Width, Height);

                if (CurrentPage == null)
                    CurrentPage = Pages.Last();
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, AsyncMethods.MakeNewPageAsync);

                if (DebugMode)
                    Log.WL($"A severe error occurred when attempting to open a new page.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when attempting to open a new page.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return this;
        }

        public async Task<AsyncHandler> ClosePageAsync() => await ClosePageAsync(CurrentPage);
        public async Task<AsyncHandler> ClosePageAsync(Page Page)
        {
            if (Page == null)
            {
                Log.WL($"An error occurred when attempting to close page.  The page was NULL!", ConsoleColor.Red);

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
                Log.WL($"An error occurred when attempting to close pages.  The pages were NULL, or there were no pages in the array!", ConsoleColor.Red);

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
                Log.WL($"An error occurred when trying to go to the URL.  The page was NULL!", ConsoleColor.DarkRed);

                return this;
            }

            try
            {
                await Page.GoToAsync(Url, DefaultTimeout, new WaitUntilNavigation[1] { WaitUntil });
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, AsyncMethods.GoToUrlAsync, Url, Page, WaitUntil);

                if (DebugMode)
                    Log.WL($"A severe error occurred when trying to go to the URL.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when trying to go to the URL.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return this;
        }
        public async Task<AsyncHandler> BackOnePageAsync(WaitUntilNavigation WaitUntil = WaitUntilNavigation.DOMContentLoaded) => await BackOnePageAsync(CurrentPage, WaitUntil);
        public async Task<AsyncHandler> BackOnePageAsync(Page Page, WaitUntilNavigation WaitUntil = WaitUntilNavigation.DOMContentLoaded)
        {
            if (Page == null)
            {
                Log.WL($"An error occurred when trying to go back a page.  The page was NULL!", ConsoleColor.Red);

                return this;
            }

            try
            {
                await Page.GoBackAsync(new NavigationOptions { Timeout = DefaultTimeout, WaitUntil = new WaitUntilNavigation[1] { WaitUntil } });
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, AsyncMethods.BackOnePageAsync, Page, WaitUntil);

                if (DebugMode)
                    Log.WL($"A severe error occurred when trying to go back a page.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when trying to go back a page.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return this;
        }
        public async Task<AsyncHandler> ForwardOnePageAsync(WaitUntilNavigation WaitUntil = WaitUntilNavigation.DOMContentLoaded) => await ForwardOnePageAsync(CurrentPage, WaitUntil);
        public async Task<AsyncHandler> ForwardOnePageAsync(Page Page, WaitUntilNavigation WaitUntil = WaitUntilNavigation.DOMContentLoaded)
        {
            if (Page == null)
            {
                Log.WL($"An error occurred when trying to go forward a page.  The page was NULL!", ConsoleColor.Red);

                return this;
            }

            try
            {
                await Page.GoForwardAsync(new NavigationOptions { Timeout = DefaultTimeout, WaitUntil = new WaitUntilNavigation[1] { WaitUntil } });
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, AsyncMethods.ForwardOnePageAsync, Page, WaitUntil);

                if (DebugMode)
                    Log.WL($"A severe error occurred when trying to go forward a page.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when trying to go forward a page.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return this;
        }
        #endregion

        #region Page Waiting ASYNC
        [Obsolete("This method is obsolete.  Please use \"WaitAsync(Milliseconds);\" instead.", false)]
        public async Task<AsyncHandler> Wait(int Milliseconds) => await WaitAsync(Milliseconds);
        public async Task<AsyncHandler> WaitAsync(int Milliseconds)
        {
            try
            {
                await Task.Delay(Milliseconds);
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, AsyncMethods.WaitAsync, Milliseconds);

                if (DebugMode)
                    Log.WL($"A severe error occurred when delaying the task.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when delaying the task.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return this;
        }
        public async Task<AsyncHandler> WaitForDOMLoadAsync() => await WaitForDOMLoadAsync(CurrentPage);
        public async Task<AsyncHandler> WaitForDOMLoadAsync(Page Page)
        {
            if (Page == null)
            {
                Log.WL($"An error occurred when waiting for a page (DOM).  The page was NULL!", ConsoleColor.Red);

                return this;
            }

            try
            {
                await Page.WaitForNavigationAsync(new NavigationOptions { Timeout = DefaultTimeout, WaitUntil = new WaitUntilNavigation[1] { WaitUntilNavigation.DOMContentLoaded } });
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, AsyncMethods.WaitForDOMLoadAsync, Page);

                if (DebugMode)
                    Log.WL($"A severe error occurred when waiting for a page (DOM).  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when waiting for a page (DOM).  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return this;
        }
        public async Task<AsyncHandler> WaitForFullLoadAsync() => await WaitForFullLoadAsync(CurrentPage);
        public async Task<AsyncHandler> WaitForFullLoadAsync(Page Page)
        {
            if (Page == null)
            {
                Log.WL($"An error occurred when waiting for a page (Load).  The page was NULL!", ConsoleColor.Red);

                return this;
            }

            try
            {
                await Page.WaitForNavigationAsync(new NavigationOptions { Timeout = DefaultTimeout, WaitUntil = new WaitUntilNavigation[1] { WaitUntilNavigation.Load } });
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, AsyncMethods.WaitForFullLoadAsync, Page);

                if (DebugMode)
                    Log.WL($"A severe error occurred when waiting for a page (Load).  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when waiting for a page (Load).  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return this;
        }
        public async Task<AsyncHandler> WaitForNetwork0LoadAsync() => await WaitForNetwork0LoadAsync(CurrentPage);
        public async Task<AsyncHandler> WaitForNetwork0LoadAsync(Page Page)
        {
            if (Page == null)
            {
                Log.WL($"An error occurred when waiting for a page (Net0).  The page was NULL!", ConsoleColor.Red);

                return this;
            }

            try
            {
                await Page.WaitForNavigationAsync(new NavigationOptions { Timeout = DefaultTimeout, WaitUntil = new WaitUntilNavigation[1] { WaitUntilNavigation.Networkidle0 } });
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, AsyncMethods.WaitForNetwork0LoadAsync, Page);

                if (DebugMode)
                    Log.WL($"A severe error occurred when waiting for a page (Net0).  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when waiting for a page (Net0).  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return this;
        }
        public async Task<AsyncHandler> WaitForNetwork2LoadAsync() => await WaitForNetwork2LoadAsync(CurrentPage);
        public async Task<AsyncHandler> WaitForNetwork2LoadAsync(Page Page)
        {
            if (Page == null)
            {
                Log.WL($"An error occurred when waiting for a page (Net2).  The page was NULL!", ConsoleColor.Red);

                return this;
            }

            try
            {
                await Page.WaitForNavigationAsync(new NavigationOptions { Timeout = DefaultTimeout, WaitUntil = new WaitUntilNavigation[1] { WaitUntilNavigation.Networkidle2 } });
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, AsyncMethods.WaitForNetwork2LoadAsync, Page);

                if (DebugMode)
                    Log.WL($"A severe error occurred when waiting for a page (Net2).  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when waiting for a page (Net2).  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return this;
        }
        public async Task<AsyncHandler> WaitForSelectorAsync(Selectors Selector) => await WaitForSelectorAsync(Selector, CurrentPage);
        public async Task<AsyncHandler> WaitForSelectorAsync(Selectors Selector, Page Page)
        {
            if (Page == null)
            {
                Log.WL($"An error occurred when waiting for a selector.  The page was NULL!", ConsoleColor.Red);

                return this;
            }

            try
            {
                await Page.WaitForSelectorAsync(Selector, new WaitForSelectorOptions { Timeout = DefaultTimeout });
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, AsyncMethods.WaitForSelectorAsync, Selector, Page);

                if (DebugMode)
                    Log.WL($"A severe error occurred when waiting for a selector.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when waiting for a selector.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return this;
        }
        public async Task<AsyncHandler> WaitForFunctionAsync(string Function) => await WaitForFunctionAsync(Function, CurrentPage);
        public async Task<AsyncHandler> WaitForFunctionAsync(string Function, Page Page)
        {
            if (Page == null)
            {
                Log.WL($"An error occurred when waiting for a function.  The page was NULL!", ConsoleColor.Red);

                return this;
            }

            try
            {
                await Page.WaitForFunctionAsync(Function, new WaitForFunctionOptions { Timeout = DefaultTimeout });
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, AsyncMethods.WaitForFunctionAsync, Function, Page);

                if (DebugMode)
                    Log.WL($"A severe error occurred when waiting for a function.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when waiting for a function.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return this;
        }
        public async Task<AsyncHandler> WaitForTimeoutAsync(int MillisecondDuration) => await WaitForTimeoutAsync(MillisecondDuration, CurrentPage);
        public async Task<AsyncHandler> WaitForTimeoutAsync(int MillisecondDuration, Page Page)
        {
            if (Page == null)
            {
                Log.WL($"An error occurred when waiting for timeout.  The page was NULL!", ConsoleColor.Red);

                return this;
            }

            try
            {
                await Page.WaitForTimeoutAsync(MillisecondDuration);
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, AsyncMethods.WaitForTimeoutAsync, MillisecondDuration, Page);

                if (DebugMode)
                    Log.WL($"A severe error occurred when waiting for timeout.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when waiting for timeout.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return this;
        }
        public async Task<AsyncHandler> WaitForExpressionAsync(string Expression) => await WaitForExpressionAsync(Expression, CurrentPage);
        public async Task<AsyncHandler> WaitForExpressionAsync(string Expression, Page Page)
        {
            if (Page == null)
            {
                Log.WL($"An error occurred when waiting for an expression.  The page was NULL!", ConsoleColor.Red);

                return this;
            }

            try
            {
                await Page.WaitForExpressionAsync(Expression, new WaitForFunctionOptions { Timeout = DefaultTimeout });
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, AsyncMethods.WaitForExpressionAsync, Expression, Page);

                if (DebugMode)
                    Log.WL($"A severe error occurred when waiting for an expression.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when waiting for an expression.  Message: " + e.Message, ConsoleColor.DarkRed);
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
                Log.WL($"An error occurred when attempting to click the page.  The page was NULL!", ConsoleColor.DarkRed);

                return this;
            }

            if (Selector == null)
            {
                Log.WL($"An error occurred when attempting to click the page.  The selector was NULL!", ConsoleColor.DarkRed);

                return this;
            }

            try
            {
                await Page.ClickAsync(Selector, new ClickOptions { Button = Button, ClickCount = ClickAmount, Delay = Delay });
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, AsyncMethods.ClickPageAsync, Selector, Button, Delay, ClickAmount, Page);

                if (DebugMode)
                    Log.WL($"A severe error occurred when attempting to click the page.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when attempting to click the page.  Message: " + e.Message, ConsoleColor.DarkRed);
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
                Log.WL($"An error occurred when attempting to type on the page.  The page was NULL!", ConsoleColor.Red);

                return this;
            }

            if (Selector == null)
            {
                Log.WL($"An error occurred when attempting to type on the page.  The selector was NULL!", ConsoleColor.Red);

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
                ExceptionThrown?.Invoke(e, AsyncMethods.TypeAsync, Selector, Message, Delay, Page);

                if (DebugMode)
                    Log.WL($"A severe error occurred when attempting to type on the page.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when attempting to type on the page.  Message: " + e.Message, ConsoleColor.DarkRed);
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
                Log.WL($"An error occurred when attempting to hover an element on the page.  The page was NULL!", ConsoleColor.Red);

                return this;
            }

            if (Selector == null)
            {
                Log.WL($"An error occurred when attempting to hover an element on the page.  The selector was NULL!", ConsoleColor.Red);

                return this;
            }

            try
            {
                await Page.HoverAsync(Selector);
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, AsyncMethods.HoverAsync, Selector, Page);

                if (DebugMode)
                    Log.WL($"A severe error occurred when attempting to hover an element on the page.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when attempting to hover an element on the page.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return this;
        }

        public async Task<AsyncHandler> ClearFieldAsync() => await ClearFieldAsync(CurrentSelector, CurrentPage);
        public async Task<AsyncHandler> ClearFieldAsync(Page Page) => await ClearFieldAsync(CurrentSelector, Page);
        public async Task<AsyncHandler> ClearFieldAsync(Selectors Selector) => await ClearFieldAsync(Selector, CurrentPage);
        public async Task<AsyncHandler> ClearFieldAsync(Selectors Selector, Page Page)
        {
            if (Page == null)
            {
                Log.WL($"An error occurred when attempting to clear an element's value.  The page was NULL!", ConsoleColor.Red);

                return this;
            }

            if (Selector == null)
            {
                Log.WL($"An error occurred when attempting to clear an element's value.  The selector was NULL!", ConsoleColor.Red);

                return this;
            }

            try
            {
                await Page.FocusAsync(Selector);
                await Page.Keyboard.DownAsync("Control");
                await Page.Keyboard.PressAsync("A");
                await Page.Keyboard.UpAsync("Control");
                await Page.Keyboard.PressAsync("Backspace");
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, AsyncMethods.ClearFieldAsync, Selector, Page);

                if (DebugMode)
                    Log.WL($"A severe error occurred when attempting to clear an element's value.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when attempting to clear an element's value.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return this;
        }

        public AsyncHandler AddDialogEvent(Action<object, DialogEventArgs> Action) => AddDialogEvent(CurrentPage, Action);
        public AsyncHandler AddDialogEvent(Page Page, Action<object, DialogEventArgs> Action)
        {
            if (Page == null)
            {
                Log.WL($"An error occurred when attempting to add a dialog event.  The page was NULL!", ConsoleColor.Red);

                return this;
            }

            if (Action == null)
            {
                Log.WL($"An error occurred when attempting to add a dialog event.  The action was NULL!", ConsoleColor.Red);

                return this;
            }

            try
            {
                Page.Dialog += new EventHandler<DialogEventArgs>(Action);
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, AsyncMethods.AddDialogEvent, Page, Action);

                if (DebugMode)
                    Log.WL($"A severe error occurred when attempting to add a dialog event.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when attempting to add a dialog event.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return this;
        }
        #endregion

        #region Page Misc ASYNC
        public async Task<string> SolveImageCaptchaAsync() => await SolveImageCaptchaAsync(CurrentPage, CurrentSelector);
        public async Task<string> SolveImageCaptchaAsync(Page Page) => await SolveImageCaptchaAsync(Page, CurrentSelector);
        public async Task<string> SolveImageCaptchaAsync(Selectors CaptchaSelector) => await SolveImageCaptchaAsync(CurrentPage, CaptchaSelector);
        public async Task<string> SolveImageCaptchaAsync(Page Page, Selectors CaptchaSelector)
        {
            if (string.IsNullOrWhiteSpace(CaptchaKey))
            {
                Log.WL($"An error occurred when attempting to solve the image captcha.  The captcha key was NULL!  You can set the key when you build the AsyncHandler.", ConsoleColor.Red);

                return null;
            }

            if (Page == null)
            {
                Log.WL($"An error occurred when attempting to solve the image captcha.  The page was NULL!", ConsoleColor.Red);

                return null;
            }

            if (CaptchaSelector == null)
            {
                Log.WL($"An error occurred when attempting to solve the image captcha.  The selector was NULL!", ConsoleColor.Red);

                return null;
            }

            string Guid = System.Guid.NewGuid().ToString();

            try
            {
                if (!Directory.Exists("temp"))
                    Directory.CreateDirectory("temp");

                await CaptchaSelector.ScreenshotAsync(Page, "temp/" + Guid + ".png");

                Log.WL($"Please wait for the Image Captcha to solve.  This usually takes 10 seconds, but can take up to 30 seconds.", ConsoleColor.DarkYellow);

                _2CaptchaAPI._2Captcha Captcha = new _2CaptchaAPI._2Captcha(CaptchaKey);
                _2CaptchaAPI._2Captcha.Result Result = await Captcha.SolveImage(new FileStream("temp/" + Guid + ".png", FileMode.Open, FileAccess.Read, FileShare.Read), _2CaptchaAPI.Enums.FileType.Png);

                return Result.ResponseObject.ToString();
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, AsyncMethods.SolveCaptchaImageAsync, Page, CaptchaSelector);

                if (DebugMode)
                    Log.WL($"A severe error occurred when attempting to solve the image captcha.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when attempting to solve the image captcha.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return null;
        }

        public async Task<AsyncHandler> ScreenshotAsync(string SaveAs) => await ScreenshotAsync(CurrentPage, SaveAs);
        public async Task<AsyncHandler> ScreenshotAsync(Page Page, string SaveAs)
        {
            if (Page == null)
            {
                Log.WL($"An error occurred when attempting to screenshot the page.  The page was NULL!", ConsoleColor.Red);

                return this;
            }

            try
            {
                await Page.ScreenshotAsync(SaveAs);
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, AsyncMethods.ScreenshotAsync, Page, SaveAs);

                if (DebugMode)
                    Log.WL($"A severe error occurred when attempting to screenshot the page.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when attempting to screenshot the page.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return this;
        }

        public async Task<string> DownloadMediaAsync(string Url, string SaveAs)
        {
            try
            {
                using (WebClient Client = new WebClient())
                {
                    await Client.DownloadFileTaskAsync(new Uri(Url), SaveAs);
                }

                return SaveAs;
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, AsyncMethods.DownloadMediaAsync, Url, SaveAs);

                if (DebugMode)
                    Log.WL($"A severe error occurred when attempting to download media.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when attempting to download media.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return null;
        }

        public async Task PauseAsync() => await Task.Delay(-1);

        public async Task<string> RetrieveContentAsync() => await RetrieveContentAsync(CurrentPage);
        public async Task<string> RetrieveContentAsync(Page Page)
        {
            if (Page == null)
            {
                Log.WL($"An error occurred when attempting to retrieve the page's content.  The page was NULL!", ConsoleColor.Red);

                return null;
            }

            try
            {
                return await Page.GetContentAsync();
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, AsyncMethods.RetrieveContentAsync, Page);

                if (DebugMode)
                    Log.WL($"A severe error occurred when attempting to hover an element on the page.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when attempting to hover an element on the page.  Message: " + e.Message, ConsoleColor.DarkRed);

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
                ExceptionThrown?.Invoke(e, AsyncMethods.CloseBrowserAsync);

                if (DebugMode)
                    Log.WL($"A severe error occurred when attempting to close the browser.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when attempting to close the browser.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            Dispose();
        }
        #endregion

        public Handler ToHandler() => new Handler(Fetcher, Browser, Context, DefaultTimeout, Proxy, Authentication, CaptchaKey, Width, Height);

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
            private string CaptchaKey;
            private Browser Browser;
            private BrowserContext Context;
            private int Width;
            private int Height;

            public Builder(int DefaultTimeout = 30000, Product Browser = Product.Chrome)
            {
                Fetcher = new BrowserFetcher(new BrowserFetcherOptions { Product = Browser });

                this.DefaultTimeout = DefaultTimeout;
            }

            public Builder SetBrowserSize(int Width, int Height)
            {
                this.Width = Width;
                this.Height = Height;

                return this;
            }

            public Builder SetBrowserProxy(ProxyRotation Proxies)
            {
                if (Proxies == null || Proxies.Count == 0)
                    return this;

                Proxy = Proxies.Next();
                Authentication = Proxies[Proxy];

                return this;
            }

            public Builder AllowCaptchas(string _2CaptchaKey)
            {
                CaptchaKey = _2CaptchaKey;

                return this;
            }

            public async Task<Builder> CreateBrowserAsync(bool Headless = false, bool Incognito = false, Action<object, DownloadProgressChangedEventArgs> DownloadProgress = null)
            {
                try
                {
                    LaunchOptions Options = new LaunchOptions
                    {
                        Headless = Headless,
                        Args = !string.IsNullOrWhiteSpace(Proxy) ? new string[1] { "--proxy-server=" + Proxy } : new string[0]
                    };

                    if (DownloadProgress != null)
                        Fetcher.DownloadProgressChanged += (Object, Args) => { DownloadProgress.Invoke(Object, Args); };

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
                        Log.WL($"A severe error occurred when attempting to create a new browser.  Stacktrace: " + e, ConsoleColor.DarkRed);
                    else
                        Log.WL($"A severe error occurred when attempting to create a new browser.  Message: " + e.Message, ConsoleColor.DarkRed);
                }

                return this;
            }

            public Handler Build() => new Handler(Fetcher, Browser, Context, DefaultTimeout, Proxy, Authentication, CaptchaKey, Width, Height);
            public AsyncHandler BuildAsync() => new AsyncHandler(Fetcher, Browser, Context, DefaultTimeout, Proxy, Authentication, CaptchaKey, Width, Height);
        }
    }

    public sealed class Handler : IDisposable
    {
        public delegate void ExceptionThrownEvent(Exception Exception, Methods Method, params object[] MethodParameters);
        public event ExceptionThrownEvent ExceptionThrown;

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
        public string CaptchaKey { get; }
        public int Width { get; }
        public int Height { get; }

        public Handler(BrowserFetcher Fetcher, Browser Browser, BrowserContext Context, int DefaultTimeout, string Proxy, string Authentication, string CaptchaKey, int Width, int Height)
        {
            this.Fetcher = Fetcher;
            this.Browser = Browser;
            this.Context = Context;
            this.Proxy = Proxy;
            this.Authentication = Authentication;
            this.CaptchaKey = CaptchaKey;
            this.Width = Width;
            this.Height = Height;

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

            try
            {
                Pages.Add(Location, WaitUntil, Authentication, Width, Height).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, Methods.MakeNewPage, Location, WaitUntil);

                if (DebugMode)
                    Log.WL($"A severe error occurred when attempting to open a new page.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when attempting to open a new page.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

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

                Pages.Add(Page, Width, Height).GetAwaiter().GetResult();

                if (CurrentPage == null)
                    CurrentPage = Pages.Last();
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, Methods.MakeNewPage);

                if (DebugMode)
                    Log.WL($"A severe error occurred when attempting to open a new page.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when attempting to open a new page.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return this;
        }

        public Handler ClosePage() => ClosePage(CurrentPage);
        public Handler ClosePage(Page Page)
        {
            if (Page == null)
            {
                Log.WL($"An error occurred when attempting to close page.  The page was NULL!", ConsoleColor.Red);

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
                Log.WL($"An error occurred when attempting to close pages.  The pages were NULL, or there were no pages in the array!", ConsoleColor.Red);

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
                Log.WL($"An error occurred when trying to go to the URL.  The page was NULL!", ConsoleColor.Red);

                return this;
            }

            try
            {
                _ = Page.GoToAsync(Url, DefaultTimeout, new WaitUntilNavigation[1] { WaitUntil }).Result;
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, Methods.GoToUrl, Url, Page, WaitUntil);

                if (DebugMode)
                    Log.WL($"A severe error occurred when trying to go to the URL.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when trying to go to the URL.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return this;
        }
        public Handler BackOnePage(WaitUntilNavigation WaitUntil = WaitUntilNavigation.DOMContentLoaded) => BackOnePage(CurrentPage, WaitUntil);
        public Handler BackOnePage(Page Page, WaitUntilNavigation WaitUntil = WaitUntilNavigation.DOMContentLoaded)
        {
            if (Page == null)
            {
                Log.WL($"An error occurred when trying to go back a page.  The page was NULL!", ConsoleColor.Red);

                return this;
            }

            try
            {
                _ = Page.GoBackAsync(new NavigationOptions { Timeout = DefaultTimeout, WaitUntil = new WaitUntilNavigation[1] { WaitUntil } }).Result;
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, Methods.BackOnePage, Page, WaitUntil);

                if (DebugMode)
                    Log.WL($"A severe error occurred when trying to go back a page.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when trying to go back a page.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return this;
        }
        public Handler ForwardOnePage(WaitUntilNavigation WaitUntil = WaitUntilNavigation.DOMContentLoaded) => ForwardOnePage(CurrentPage, WaitUntil);
        public Handler ForwardOnePage(Page Page, WaitUntilNavigation WaitUntil = WaitUntilNavigation.DOMContentLoaded)
        {
            if (Page == null)
            {
                Log.WL($"An error occurred when trying to go forward a page.  The page was NULL!", ConsoleColor.Red);

                return this;
            }

            try
            {
                _ = Page.GoForwardAsync(new NavigationOptions { Timeout = DefaultTimeout, WaitUntil = new WaitUntilNavigation[1] { WaitUntil } }).Result;
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, Methods.ForwardOnePage, Page, WaitUntil);

                if (DebugMode)
                    Log.WL($"A severe error occurred when trying to go forward a page.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when trying to go forward a page.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return this;
        }
        #endregion

        #region Page Waiting
        public Handler Wait(int Milliseconds)
        {
            try
            {
                Task.Delay(Milliseconds).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, Methods.Wait, Milliseconds);

                if (DebugMode)
                    Log.WL($"A severe error occurred when delaying the task.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when delaying the task.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return this;
        }
        public Handler WaitForDOMLoad() => WaitForDOMLoad(CurrentPage);
        public Handler WaitForDOMLoad(Page Page)
        {
            if (Page == null)
            {
                Log.WL($"An error occurred when waiting for a page (DOM).  The page was NULL!", ConsoleColor.Red);

                return this;
            }

            try
            {
                _ = Page.WaitForNavigationAsync(new NavigationOptions { Timeout = DefaultTimeout, WaitUntil = new WaitUntilNavigation[1] { WaitUntilNavigation.DOMContentLoaded } }).Result;
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, Methods.WaitForDOMLoad, Page);

                if (DebugMode)
                    Log.WL($"A severe error occurred when waiting for a page (DOM).  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when waiting for a page (DOM).  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return this;
        }
        public Handler WaitForFullLoad() => WaitForFullLoad(CurrentPage);
        public Handler WaitForFullLoad(Page Page)
        {
            if (Page == null)
            {
                Log.WL($"An error occurred when waiting for a page (Load).  The page was NULL!", ConsoleColor.Red);

                return this;
            }

            try
            {
                _ = Page.WaitForNavigationAsync(new NavigationOptions { Timeout = DefaultTimeout, WaitUntil = new WaitUntilNavigation[1] { WaitUntilNavigation.Load } }).Result;
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, Methods.WaitForFullLoad, Page);

                if (DebugMode)
                    Log.WL($"A severe error occurred when waiting for a page (Load).  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when waiting for a page (Load).  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return this;
        }
        public Handler WaitForNetwork0Load() => WaitForNetwork0Load(CurrentPage);
        public Handler WaitForNetwork0Load(Page Page)
        {
            if (Page == null)
            {
                Log.WL($"An error occurred when waiting for a page (Net0).  The page was NULL!", ConsoleColor.Red);

                return this;
            }

            try
            {
                _ = Page.WaitForNavigationAsync(new NavigationOptions { Timeout = DefaultTimeout, WaitUntil = new WaitUntilNavigation[1] { WaitUntilNavigation.Networkidle0 } }).Result;
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, Methods.WaitForNetwork0Load, Page);

                if (DebugMode)
                    Log.WL($"A severe error occurred when waiting for a page (Net0).  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when waiting for a page (Net0).  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return this;
        }
        public Handler WaitForNetwork2Load() => WaitForNetwork2Load(CurrentPage);
        public Handler WaitForNetwork2Load(Page Page)
        {
            if (Page == null)
            {
                Log.WL($"An error occurred when waiting for a page (Net2).  The page was NULL!", ConsoleColor.Red);

                return this;
            }

            try
            {
                _ = Page.WaitForNavigationAsync(new NavigationOptions { Timeout = DefaultTimeout, WaitUntil = new WaitUntilNavigation[1] { WaitUntilNavigation.Networkidle2 } }).Result;
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, Methods.WaitForNetwork2Load, Page);

                if (DebugMode)
                    Log.WL($"A severe error occurred when waiting for a page (Net2).  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when waiting for a page (Net2).  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return this;
        }
        public Handler WaitForSelector(Selectors Selector) => WaitForSelector(Selector, CurrentPage);
        public Handler WaitForSelector(Selectors Selector, Page Page)
        {
            if (Page == null)
            {
                Log.WL($"An error occurred when waiting for a selector.  The page was NULL!", ConsoleColor.Red);

                return this;
            }

            try
            {
                _ = Page.WaitForSelectorAsync(Selector, new WaitForSelectorOptions { Timeout = DefaultTimeout }).Result;
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, Methods.WaitForSelector, Selector, Page);

                if (DebugMode)
                    Log.WL($"A severe error occurred when waiting for a selector.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when waiting for a selector.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return this;
        }
        public Handler WaitForFunction(string Function) => WaitForFunction(Function, CurrentPage);
        public Handler WaitForFunction(string Function, Page Page)
        {
            if (Page == null)
            {
                Log.WL($"An error occurred when waiting for a function.  The page was NULL!", ConsoleColor.Red);

                return this;
            }

            try
            {
                _ = Page.WaitForFunctionAsync(Function, new WaitForFunctionOptions { Timeout = DefaultTimeout }).Result;
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, Methods.WaitForFunction, Function, Page);

                if (DebugMode)
                    Log.WL($"A severe error occurred when waiting for a function.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when waiting for a function.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return this;
        }
        public Handler WaitForTimeout(int MillisecondDuration) => WaitForTimeout(MillisecondDuration, CurrentPage);
        public Handler WaitForTimeout(int MillisecondDuration, Page Page)
        {
            if (Page == null)
            {
                Log.WL($"An error occurred when waiting for timeout.  The page was NULL!", ConsoleColor.Red);

                return this;
            }

            try
            {
                Page.WaitForTimeoutAsync(MillisecondDuration).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, Methods.WaitForTimeout, MillisecondDuration, Page);

                if (DebugMode)
                    Log.WL($"A severe error occurred when waiting for timeout.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when waiting for timeout.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return this;
        }
        public Handler WaitForExpression(string Expression) => WaitForExpression(Expression, CurrentPage);
        public Handler WaitForExpression(string Expression, Page Page)
        {
            if (Page == null)
            {
                Log.WL($"An error occurred when waiting for an expression.  The page was NULL!", ConsoleColor.Red);

                return this;
            }

            try
            {
                _ = Page.WaitForExpressionAsync(Expression, new WaitForFunctionOptions { Timeout = DefaultTimeout }).Result;
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, Methods.WaitForExpression, Expression, Page);

                if (DebugMode)
                    Log.WL($"A severe error occurred when waiting for an expression.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when waiting for an expression.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return this;
        }
        #endregion

        #region Page Input
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
                Log.WL($"An error occurred when attempting to click the page.  The page was NULL!", ConsoleColor.Red);

                return this;
            }

            if (Selector == null)
            {
                Log.WL($"An error occurred when attempting to click the page.  The selector was NULL!", ConsoleColor.Red);

                return this;
            }

            try
            {
                Page.ClickAsync(Selector, new ClickOptions { Button = Button, ClickCount = ClickAmount, Delay = Delay }).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, Methods.ClickPage, Selector, Button, Delay, ClickAmount, Page);

                if (DebugMode)
                    Log.WL($"A severe error occurred when attempting to click the page.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when attempting to click the page.  Message: " + e.Message, ConsoleColor.DarkRed);
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
                Log.WL($"An error occurred when attempting to type on the page.  The page was NULL!", ConsoleColor.Red);

                return this;
            }

            if (Selector == null)
            {
                Log.WL($"An error occurred when attempting to type on the page.  The selector was NULL!", ConsoleColor.Red);

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
                ExceptionThrown?.Invoke(e, Methods.Type, Selector, Message, Delay, Page);

                if (DebugMode)
                    Log.WL($"A severe error occurred when attempting to type on the page.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when attempting to type on the page.  Message: " + e.Message, ConsoleColor.DarkRed);
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
                Log.WL($"An error occurred when attempting to hover an element on the page.  The page was NULL!", ConsoleColor.Red);

                return this;
            }

            if (Selector == null)
            {
                Log.WL($"An error occurred when attempting to hover an element on the page.  The selector was NULL!", ConsoleColor.Red);

                return this;
            }

            try
            {
                Page.HoverAsync(Selector).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, Methods.Hover, Selector, Page);

                if (DebugMode)
                    Log.WL($"A severe error occurred when attempting to hover an element on the page.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when attempting to hover an element on the page.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return this;
        }

        public Handler ClearField() => ClearField(CurrentSelector, CurrentPage);
        public Handler ClearField(Page Page) => ClearField(CurrentSelector, Page);
        public Handler ClearField(Selectors Selector) => ClearField(Selector, CurrentPage);
        public Handler ClearField(Selectors Selector, Page Page)
        {
            if (Page == null)
            {
                Log.WL($"An error occurred when attempting to clear an element's value.  The page was NULL!", ConsoleColor.Red);

                return this;
            }

            if (Selector == null)
            {
                Log.WL($"An error occurred when attempting to clear an element's value.  The selector was NULL!", ConsoleColor.Red);

                return this;
            }

            try
            {
                Page.FocusAsync(Selector).GetAwaiter().GetResult();
                Page.Keyboard.DownAsync("Control").GetAwaiter().GetResult();
                Page.Keyboard.PressAsync("A").GetAwaiter().GetResult();
                Page.Keyboard.UpAsync("Control").GetAwaiter().GetResult();
                Page.Keyboard.PressAsync("Backspace").GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, Methods.ClearField, Selector, Page);

                if (DebugMode)
                    Log.WL($"A severe error occurred when attempting to clear an element's value.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when attempting to clear an element's value.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return this;
        }

        public Handler AddDialogEvent(Action<object, DialogEventArgs> Action) => AddDialogEvent(CurrentPage, Action);
        public Handler AddDialogEvent(Page Page, Action<object, DialogEventArgs> Action)
        {
            if (Page == null)
            {
                Log.WL($"An error occurred when attempting to add a dialog event.  The page was NULL!", ConsoleColor.Red);

                return this;
            }

            if (Action == null)
            {
                Log.WL($"An error occurred when attempting to add a dialog event.  The action was NULL!", ConsoleColor.Red);

                return this;
            }

            try
            {
                Page.Dialog += new EventHandler<DialogEventArgs>(Action);
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, Methods.AddDialogEvent, Page, Action);

                if (DebugMode)
                    Log.WL($"A severe error occurred when attempting to add a dialog event.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when attempting to add a dialog event.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return this;
        }
        #endregion

        #region Page Misc
        public string SolveImageCaptcha() => SolveImageCaptcha(CurrentPage, CurrentSelector);
        public string SolveImageCaptcha(Page Page) => SolveImageCaptcha(Page, CurrentSelector);
        public string SolveImageCaptcha(Selectors CaptchaSelector) => SolveImageCaptcha(CurrentPage, CaptchaSelector);
        public string SolveImageCaptcha(Page Page, Selectors CaptchaSelector)
        {
            Stopwatch Watch = new Stopwatch();

            Watch.Start();

            if (string.IsNullOrWhiteSpace(CaptchaKey))
            {
                Log.WL($"An error occurred when attempting to solve the image captcha.  The captcha key was NULL!  You can set the key when you build the AsyncHandler.", ConsoleColor.Red);

                return null;
            }

            if (Page == null)
            {
                Log.WL($"An error occurred when attempting to solve the image captcha.  The page was NULL!", ConsoleColor.Red);

                return null;
            }

            if (CaptchaSelector == null)
            {
                Log.WL($"An error occurred when attempting to solve the image captcha.  The selector was NULL!", ConsoleColor.Red);

                return null;
            }

            try
            {
                if (!Directory.Exists("temp"))
                    Directory.CreateDirectory("temp");

                string Guid = System.Guid.NewGuid().ToString();

                CaptchaSelector.Screenshot(Page, "temp/" + Guid + ".png");

                Log.WL($"Please wait for the Image Captcha to solve.  This usually takes 10 seconds, but can take up to 30 seconds.", ConsoleColor.Yellow);

                _2CaptchaAPI._2Captcha Captcha = new _2CaptchaAPI._2Captcha(CaptchaKey);
                _2CaptchaAPI._2Captcha.Result Result = Captcha.SolveImage(new FileStream("temp/" + Guid + ".png", FileMode.Open, FileAccess.Read, FileShare.Read), _2CaptchaAPI.Enums.FileType.Png).Result;

                Log.W($"Image Captcha solved!", ConsoleColor.DarkGreen);

                Watch.Stop();

                if (DebugMode)
                    Log.WL(" Solved in " + (Watch.ElapsedMilliseconds / 1000.000).ToString("#,##0.0##") + "s.", ConsoleColor.DarkGreen);
                else
                    Log.NL();

                return Result.ResponseObject.ToString();
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, Methods.SolveImageCaptcha, Page, CaptchaSelector);

                if (DebugMode)
                    Log.WL($"A severe error occurred when attempting to solve the image captcha.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when attempting to solve the image captcha.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return null;
        }

        public Handler Screenshot(string SaveAs) => Screenshot(CurrentPage, SaveAs);
        public Handler Screenshot(Page Page, string SaveAs)
        {
            if (Page == null)
            {
                Log.WL($"An error occurred when attempting to screenshot the page.  The page was NULL!", ConsoleColor.Red);

                return this;
            }

            try
            {
                Page.ScreenshotAsync(SaveAs).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, Methods.SolveImageCaptcha, Page, SaveAs);

                if (DebugMode)
                    Log.WL($"A severe error occurred when attempting to screenshot the page.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when attempting to screenshot the page.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return this;
        }

        public string DownloadMedia(string Url, string SaveAs)
        {
            try
            {
                using (WebClient Client = new WebClient())
                {
                    Client.DownloadFile(new Uri(Url), SaveAs);
                }

                return SaveAs;
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, Methods.DownloadMedia, Url, SaveAs);

                if (DebugMode)
                    Log.WL($"A severe error occurred when attempting to download media.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when attempting to download media.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            return null;
        }

        public void Pause() => Task.Delay(-1).GetAwaiter().GetResult();

        public string RetrieveContent() => RetrieveContent(CurrentPage);
        public string RetrieveContent(Page Page)
        {
            if (Page == null)
            {
                Log.WL($"An error occurred when attempting to retrieve the page's content.  The page was NULL!", ConsoleColor.Red);

                return null;
            }

            try
            {
                return Page.GetContentAsync().Result;
            }
            catch (Exception e)
            {
                ExceptionThrown?.Invoke(e, Methods.RetrieveContent, Page);

                if (DebugMode)
                    Log.WL($"A severe error occurred when attempting to hover an element on the page.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when attempting to hover an element on the page.  Message: " + e.Message, ConsoleColor.DarkRed);

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
                ExceptionThrown?.Invoke(e, Methods.CloseBrowser);

                if (DebugMode)
                    Log.WL($"A severe error occurred when attempting to close the browser.  Stacktrace: " + e, ConsoleColor.DarkRed);
                else
                    Log.WL($"A severe error occurred when attempting to close the browser.  Message: " + e.Message, ConsoleColor.DarkRed);
            }

            Dispose();
        }
        #endregion

        public AsyncHandler ToAsyncHandler() => new AsyncHandler(Fetcher, Browser, Context, DefaultTimeout, Proxy, Authentication, CaptchaKey, Width, Height);

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
            private string CaptchaKey;
            private int Width;
            private int Height;

            public Builder(int DefaultTimeout = 30000, Product Browser = Product.Chrome)
            {
                Fetcher = new BrowserFetcher(new BrowserFetcherOptions { Product = Browser });

                this.DefaultTimeout = DefaultTimeout;
            }

            public Builder SetBrowserSize(int Width, int Height)
            {
                this.Width = Width;
                this.Height = Height;

                return this;
            }

            public Builder SetBrowserProxy(ProxyRotation Proxies)
            {
                if (Proxies == null || Proxies.Count == 0)
                    return this;

                Proxy = Proxies.Next();
                Authentication = Proxies[Proxy];

                return this;
            }

            public Builder AllowCaptchas(string _2CaptchaKey)
            {
                CaptchaKey = _2CaptchaKey;

                return this;
            }

            public Builder CreateBrowser(bool Headless = false, bool Incognito = false, Action<object, DownloadProgressChangedEventArgs> DownloadProgress = null)
            {
                try
                {
                    LaunchOptions Options = new LaunchOptions
                    {
                        Headless = Headless,
                        Args = !string.IsNullOrWhiteSpace(Proxy) ? new string[1] { "--proxy-server=" + Proxy } : new string[0]
                    };

                    if (DownloadProgress != null)
                        Fetcher.DownloadProgressChanged += (Object, Args) => { DownloadProgress.Invoke(Object, Args); };

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
                        Log.WL($"A severe error occurred when attempting to create a new browser.  Stacktrace: " + e, ConsoleColor.DarkRed);
                    else
                        Log.WL($"A severe error occurred when attempting to create a new browser.  Message: " + e.Message, ConsoleColor.DarkRed);
                }

                return this;
            }

            public Handler Build() => new Handler(Fetcher, Browser, Context, DefaultTimeout, Proxy, Authentication, CaptchaKey, Width, Height);
            public AsyncHandler BuildAsync() => new AsyncHandler(Fetcher, Browser, Context, DefaultTimeout, Proxy, Authentication, CaptchaKey, Width, Height);
        }
    }
}
