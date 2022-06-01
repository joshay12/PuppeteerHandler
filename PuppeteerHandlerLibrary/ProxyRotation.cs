using System.Collections.Generic;
using System.Linq;

namespace PuppeteerHandler
{
    public class ProxyRotation
    {
        private readonly Dictionary<string, string> Proxies;

        public int Count { get { return Proxies.Count; } }
        public string this[int Index] { get { return Proxies.ElementAt(Index).Key; } }
        public string this[string Proxy] { get { return Proxies[Proxy]; } }
        public string Current { get { return this[CurrentIndex]; } }
        public int CurrentIndex { get; private set; }

        public ProxyRotation()
        {
            Proxies = new Dictionary<string, string>();
            CurrentIndex = 0;
        }

        public void AddProxy(string IP, int Port) => AddProxy(IP + ":" + Port);
        public void AddProxy(string Proxy) => Proxies.Add(Proxy, null);
        public void AddProxyUserAuth(string IP, int Port, string Username, string Password) => AddProxyUserAuth(IP + ":" + Port + ":" + Username + ":" + Password);
        public void AddProxyUserAuth(string Proxy)
        {
            if (Proxy == null)
            {
                Log.WL($"{ ChatColor.Red }There was an issue adding your User Authenticated Proxy.  The proxy was NULL.");

                return;
            }

            string[] Info = Proxy.Split(':');

            if (Info.Length != 4)
            {
                Log.WL($"{ ChatColor.Red }There was an issue adding your User Authenticated Proxy.  The proxy length had { Info.Length } specifications, when it need 4.");

                return;
            }

            Proxies.Add(Info[0] + ":" + Info[1], Info[2] + ":" + Info[3]);
        }

        public void ClearProxies() => Proxies.Clear();

        public string Next()
        {
            CurrentIndex++;

            if (CurrentIndex >= Count)
                CurrentIndex = 0;

            return Proxies.ElementAt(CurrentIndex).Key;
        }

        public override string ToString()
        {
            string Output = "Rotational Proxies {\n";

            foreach (KeyValuePair<string, string> Item in Proxies)
            {
                bool User = Item.Value != null;

                Output += "  " + Item.Key;

                if (User)
                    Output += " {\n    " + string.Join("\n    ", Item.Value.Split(':')) + "\n  },\n";
                else
                    Output += ",\n";
            }

            if (Output.EndsWith(",\n"))
                Output = Output.Substring(0, Output.Length - 2) + "\n";

            return Output + "}";
        }
    }
}
