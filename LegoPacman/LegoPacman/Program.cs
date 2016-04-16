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
            TestRotation(90, RotationDirection.Left);
            TestRotation(90, RotationDirection.Right);
            TestRotation(180, RotationDirection.Left);
            TestRotation(180, RotationDirection.Right);
            TestRotation(360, RotationDirection.Left);
            TestRotation(360, RotationDirection.Right);
            TestRotation(45, RotationDirection.Left);
            TestRotation(45, RotationDirection.Right);

            terminateProgram.WaitOne();
        }

        private static void TestRotation(int degrees, RotationDirection direction)
        {
            MonoBrickFirmware.Display.LcdConsole.WriteLine("{0} degrees", degrees);
            roboter.Rotate(degrees, direction);
            Thread.Sleep(1000);
        }
    }
}
