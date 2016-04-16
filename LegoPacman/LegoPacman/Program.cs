using LegoPacman.classes;
using MonoBrickFirmware.Movement;
using MonoBrickFirmware.UserInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LegoPacman
{
    class Program
    {
        private static Roboter roboter;

        static void Main(string[] args)
        {
            var terminateProgram = new ManualResetEvent(false);
            var buttonEvents = new ButtonEvents();

            buttonEvents.EscapePressed += () =>
            {
                terminateProgram.Set();
            };

            roboter = new Roboter();
            TestRotation(90);
            TestRotation(270);
            TestRotation(180);
            TestRotation(360);
            TestRotation(540);
            TestRotation(810);
            TestRotation(-90);
            TestRotation(-180);
            TestRotation(-270);
            TestRotation(-360);
            TestRotation(-810);
            TestRotation(-900);

            terminateProgram.WaitOne();
        }

        private static void TestRotation(int degrees)
        {
            MonoBrickFirmware.Display.LcdConsole.WriteLine("{0} degrees", degrees);
            roboter.Rotate(degrees);
            Thread.Sleep(1000);
        }
    }
}
