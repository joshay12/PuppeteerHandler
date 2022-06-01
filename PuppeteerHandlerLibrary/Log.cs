using System;
using System.Collections.Generic;

namespace PuppeteerHandler
{
    internal class ChatColor
    {
        internal string Code { get { return "{" + Color.ToString() + "}"; } }
        internal ConsoleColor Color { get; }

        public static implicit operator ChatColor(ConsoleColor Color) => new ChatColor(Color);
        public static implicit operator ConsoleColor(ChatColor Color) => Color.Color;

        internal static readonly ChatColor[] AllColors = new ChatColor[16] { Black, Blue, Cyan, DarkBlue, DarkCyan, DarkGray, DarkGreen, DarkMagenta, DarkRed, DarkYellow, Gray, Green, Magenta, Red, White, Yellow };

        internal static ChatColor Black = ConsoleColor.Black;
        internal static ChatColor Blue = ConsoleColor.Blue;
        internal static ChatColor Cyan = ConsoleColor.Cyan;
        internal static ChatColor DarkBlue = ConsoleColor.DarkBlue;
        internal static ChatColor DarkCyan = ConsoleColor.DarkCyan;
        internal static ChatColor DarkGray = ConsoleColor.DarkGray;
        internal static ChatColor DarkGreen = ConsoleColor.DarkGreen;
        internal static ChatColor DarkMagenta = ConsoleColor.DarkMagenta;
        internal static ChatColor DarkRed = ConsoleColor.DarkRed;
        internal static ChatColor DarkYellow = ConsoleColor.DarkYellow;
        internal static ChatColor Gray = ConsoleColor.Gray;
        internal static ChatColor Green = ConsoleColor.Green;
        internal static ChatColor Magenta = ConsoleColor.Magenta;
        internal static ChatColor Red = ConsoleColor.Red;
        internal static ChatColor White = ConsoleColor.White;
        internal static ChatColor Yellow = ConsoleColor.Yellow;

        private ChatColor(ConsoleColor Color) => this.Color = Color;

        public override string ToString() => Color.ToString();
    }

    internal static class Log
    {
        private static KeyValuePair<string, ChatColor>[] CheckMessage(object Message)
        {
            if (Message == null)
                return new KeyValuePair<string, ChatColor>[0];

            string Msg = Message.ToString();

            List<KeyValuePair<string, ChatColor>> Output = new List<KeyValuePair<string, ChatColor>>();

            string Current = "";

            for (int i = 0; i < Msg.Length; i++)
            {
                char C = Msg[i];

                if (C == '{')
                {
                    ChatColor Chosen = null;

                    foreach (ChatColor Color in ChatColor.AllColors)
                    {
                        bool Broke = false;

                        int ii = 0;

                        for (; ii < Color.Code.Length; ii++)
                        {
                            if (Color.Code[ii] != Msg[i + ii])
                            {
                                Broke = true;

                                break;
                            }
                        }

                        if (!Broke)
                        {
                            i += ii;

                            Chosen = Color;

                            break;
                        }
                    }

                    if (Chosen != null)
                    {
                        Output.Add(new KeyValuePair<string, ChatColor>(Current, Chosen));

                        continue;
                    }
                }

                Current += C;
            }

            if (Current.Length > 0)
                Output.Add(new KeyValuePair<string, ChatColor>(Current, null));

            return Output.ToArray();
        }

        public static void WL(object Message)
        {
            KeyValuePair<string, ChatColor>[] Results = CheckMessage(Message);

            foreach (KeyValuePair<string, ChatColor> Item in Results)
            {
                Console.WriteLine(Item.Key);
                Console.ForegroundColor = Item.Value;
            }
        }
    }
}
