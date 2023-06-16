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
        static void Main(string[] args)
        {
            ConsoleSc.PressAnyKeyToContinue();

            _ = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(5000);
                    ConsoleSc.WriteLine(DateTime.Now.ToString());
                }
            });


            DateTime qwq = ConsoleSc.ReadForDateTime("Input a date time");
            ConsoleSc.WriteLine($"DateTime: {qwq}");

            long num = ConsoleSc.ReadForLong("Input a long integer");
            ConsoleSc.WriteLine($"Number: {num}");

            DayOfWeek day = ConsoleSc.Select<DayOfWeek>("What day is it today?");
            if (day == DayOfWeek.Sunday ||
                day == DayOfWeek.Saturday)
                ConsoleSc.WriteLine("Have a good time~");
            else
                ConsoleSc.WriteLine("It's time to work");
        }
    }

    enum DayOfWeek
    {
        Sunday,
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday,
        Saturday,
    }
}
