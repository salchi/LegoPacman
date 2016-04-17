using LegoPacman.classes;
using MonoBrickFirmware.Display;
using MonoBrickFirmware.Movement;
using MonoBrickFirmware.Sensors;
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

        public static object LegoUtil { get; private set; }

        static void Main(string[] args)
        {
            var terminateProgram = new ManualResetEvent(false);
            var buttonEvents = new ButtonEvents();

            buttonEvents.EscapePressed += () =>
            {
                terminateProgram.Set();
            };

            LcdConsole.WriteLine("up: Rotation");
            LcdConsole.WriteLine("left: MoveForwardByCm");
            LcdConsole.WriteLine("right: AlignAlongRightSide");
            LcdConsole.WriteLine("down: MoveToFence");

            buttonEvents.UpPressed += () =>
            {
                RoboTest.Rotation(90, 360, 90);
            };
            buttonEvents.LeftPressed += () =>
            {
                RoboTest.MoveForwardByCm(10);
            };
            buttonEvents.RightPressed += () =>
            {
                RoboTest.AlignAlongRightSide();
            };
            buttonEvents.DownPressed += () =>
            {
                RoboTest.MoveToFence();
            };

            terminateProgram.WaitOne();
        }
    }
}
