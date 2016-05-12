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
        private static void PrintInfo()
        {
            LcdConsole.WriteLine("up: followFence");
            LcdConsole.WriteLine("left: moveByCm");
            LcdConsole.WriteLine("right: readColor");
            LcdConsole.WriteLine("down: getColor");
        }

        static void Main(string[] args)
        {
            var terminateProgram = new ManualResetEvent(false);
            var buttonEvents = new ButtonEvents();

            buttonEvents.EscapePressed += () =>
            {
                terminateProgram.Set();
            };

            PrintInfo();

            buttonEvents.EnterPressed += () =>
            {
                LcdConsole.Clear();
                PrintInfo();
            };

            buttonEvents.UpPressed += () =>
            {
                LcdConsole.WriteLine("starting followFence");
                RoboTest.FollowFence();
                LcdConsole.WriteLine("followFence done");
            };

            buttonEvents.LeftPressed += () =>
            {
                LcdConsole.WriteLine("starting moveByCm 3");
                RoboTest.MoveByCm(3);
                Thread.Sleep(2000);
                LcdConsole.WriteLine("starting moveByCm 27");
                RoboTest.MoveByCm(27);
                LcdConsole.WriteLine("moveByCm done");
            };

            buttonEvents.RightPressed += () =>
            {
                LcdConsole.WriteLine("starting readColor");
                RoboTest.ReadColor();
                LcdConsole.WriteLine("readColor done");
            };

            buttonEvents.DownPressed += () =>
            {
                RoboTest.GetColor();
            };

            terminateProgram.WaitOne();
            RoboTest.TurnOffMotors();
        }
    }
}
