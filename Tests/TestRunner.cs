using System;
using System.Configuration;

namespace Tests
{
    // Written by blokeley
    class NUnitConsoleRunner
    {
        [STAThread]
        static void Main(string[] args)
        {
            NUnit.ConsoleRunner.Runner.Main(args);

        }
    }
}