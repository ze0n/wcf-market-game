using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client
{
    class ConsoleCommander
    {
        Dictionary<string, Func<string [], string>> Cmds = new Dictionary<string, Func<string[], string>>();

        public void AddCmd(string cmdName, Func<string[], string> cmdFunc)
        {
            Cmds.Add(cmdName, cmdFunc);
        }

        private void PrintPrompt()
        {
            Console.Write("# ");
        }

        private string[] ReadCmd()
        {
            return Console.ReadLine().Split(new []{' '});
        }

        public void StartLoop()
        {
            while (true)
            {
                PrintPrompt();
                var cs = ReadCmd();

                if (cs.Length == 0)
                    continue;

                if (cs[0].ToUpper() == "EXIT" || cs[0].ToUpper() == "QUIT" || cs[0].ToUpper() == "Q")
                    break;

                if (Cmds.ContainsKey(cs[0]))
                {
                    var s = Cmds[cs[0]].Invoke(cs);
                    Console.WriteLine();
                    Console.WriteLine(s);
                    Console.WriteLine();
                }
            }
        }
    }
}
