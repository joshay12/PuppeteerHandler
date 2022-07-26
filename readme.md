# Puppeteer Handler Documentation

## Async and sync

In this API, you have a choice between keeping Puppeteer async or sync.  If you ever change your mind, or want to convert to async/sync mid-way through your program, you can always change it with `ToHandler()` or `ToAsyncHandler()`.

## Error Handling

All errors are handled for you and will be logged to the Console accordingly.  There is also a Debug mode that you can enable with `Handler.DebugMode = true;` or `AsyncHandler.DebugMode = true;`, allowing the Stacktrace of all handled errors to be printed.  If this is false, then it will only print the error message.  However, if you want to gather specific data on the errors that are handled, that can happen through the following.

```CSharp
NAME.ExceptionThrown += NAME_ExceptionThrown;
```

With a method to give access.

```CSharp
private static void NAME_ExceptionThrown(Exception Exception, Methods Method, params object[] MethodParameters) {

}
```

The `Exception` is the exception thrown.
The `Method` is the method the exception was thrown in.
The `MethodParameters` are the parameters that were provided to the method.

## Creating a Handler

```CSharp
Handler NAME = new Handler.Builder().CreateBrowser().Build();
```
OR
```CSharp
AsyncHandler NAME = (await new AsyncHandler.Builder().CreateBrowserAsync()).BuildAsync();
```

In the Builder constructor, you can specify the default timeout and the browser you wish to launch.  When creating the browser and page, you can specify if you want the browser to be headless or in incognito mode.  For example, like this:

```CSharp
//                   MilliSecond Timeout, Browser to Launch                 Headless, Incognito
Handler NAME = new Handler.Builder(10000, PuppeteerSharp.Product.Firefox).Build(true, true);
```

## Handling Selectors

```CSharp
Selectors Selector = new Selectors.Builder().AddClass("CLASS_NAME_WITHOUT_PERIOD").Build();
```
OR
```CSharp
Selectors Selector = new Selectors("ELEMENT_ID_WITHOUT_HASHTAG/POUND_SYMBOL");
```
OR if you just want to use normal selectors
```CSharp
Selectors Selector = Selectors.FromSelector("SELECTOR_INFORMATION");
```
Of course, you aren't limited to just one item in the builder.  You can add more classes, elements, attributes, etc. to find what you need.  Just add more data, like this.
```CSharp
new Selectors.Builder().AddClass("Class1").AddChildClass("Child").AddAttribute("value", "10").AddSiblingElement("div").NewSelector().AddClass("Class2").Build();

```
If printed to the Console, the output would be as follows...
> .Class1 > .Child[value=10] + div, .Class2

## Handling Proxies

```CSharp
ProxyRotation Proxies = new ProxyRotation();
```
Using the code above, you can create a rotation of proxies per browser that loads.  To add a proxy (IP Auth or User Auth), use the below example.
```CSharp
Proxies.AddProxy("IP_ADDRESS", PORT);
Proxies.AddProxy("IP_ADDRESS:PORT");  // Both give the same result.
```
Above is an example of IP Authenticated Proxies.  Below is an example of User Authenticated Proxies.
```CSharp
Proxies.AddProxyUserAuth("IP_ADDRESS", PORT, "USERNAME", "PASSWORD");
Proxies.AddProxyUserAuth("IP_ADDRESS:PORT:USERNAME:PASSWORD"); // Both give the same result.
```
After this, add it to the browser handler with the following code.
```CSharp
Handler NAME = new Handler.Builder().SetBrowserProxy(Proxies).CreateBrowser().Build();
```
OR
```CSharp
AsyncHandler NAME = (await new AsyncHandler.Builder().SetBrowserProxy(Proxies).CreateBrowserAsync()).BuildAsync();
```
Using this, it will automatically rotate the proxies for you.

## Handling Captchas

At the moment, only text captchas work.  You will need a 2Captcha account and the 2Captcha API Key.  Then, when creating your browser, insert your API Key like this.

```CSharp
Handler NAME = new Handler.Builder().AllowCaptchas("API_KEY").CreateBrowser().Build();
```
OR 
```CSharp
AsyncHandler NAME = (await new AsyncHandler.Builder().AllowCaptchas("API_KEY").CreateBrowserAsync()).BuildAsync();
```
Then, you can solve the text captchas with the following (will display Handler only; not AsyncHandler).
```CSharp
string Result = NAME.SolveImageCaptcha(new Selectors("SELECTOR_OF_CAPTCHA_IMAGE"));
```