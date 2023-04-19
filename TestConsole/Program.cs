using System;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Tasks;
using EleCho.ConsoleUtilities;

namespace TestConsole
{
    class Program
    {
        static Program()
        {
            ConsoleSc.PromptForInput = ">>> ";
        }
        
        static void Main(string[] args)
        {
            ConsoleSc.PressAnyKeyToContinue();

            //_ = Task.Run(async () =>
            //{
            //    while (true)
            //    {
            //        await Task.Delay(1000);
            //        ConsoleSc.WriteLine(DateTime.Now.ToString());
            //    }
            //});

            while(true)
            {
                DateTime qwq = ConsoleSc.ReadForDateTime("Input a date time");
                ConsoleSc.WriteLine(qwq.ToString());
            }
        }
    }
}
