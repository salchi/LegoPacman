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
            LcdConsole.WriteLine("up: straigth");
            LcdConsole.WriteLine("left: left");
            LcdConsole.WriteLine("right: rigth");
            LcdConsole.WriteLine("down: back");
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
                LcdConsole.WriteLine("starting straigth");
                RoboTest.Straigth();
                LcdConsole.WriteLine("straigth done");
            };

            buttonEvents.LeftPressed += () =>
            {
                LcdConsole.WriteLine("starting left");
                RoboTest.Left();
                LcdConsole.WriteLine("left done");
            };

            buttonEvents.RightPressed += () =>
            {
                LcdConsole.WriteLine("starting right");
                RoboTest.Right();
                LcdConsole.WriteLine("right done");
            };

            buttonEvents.DownPressed += () =>
            {
                LcdConsole.WriteLine("starting back");
                RoboTest.Back();
                LcdConsole.WriteLine("back done");
            };

            terminateProgram.WaitOne();
            RoboTest.TurnOffMotors();
        }
    }
}
