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
            LcdConsole.WriteLine("up: moveForwardWhile");
            LcdConsole.WriteLine("left: MoveToFence");
            LcdConsole.WriteLine("right: moveForwardUntil");
            LcdConsole.WriteLine("down: read color");
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
                LcdConsole.WriteLine("starting moveForwardWhile");
                RoboTest.MoveForwardWhile();
                LcdConsole.WriteLine("moveForwardWhile done");
            };

            buttonEvents.LeftPressed += () =>
            {
                LcdConsole.WriteLine("starting MoveToFence");
                RoboTest.MoveToFence();
                LcdConsole.WriteLine("MoveToFence done");
            };

            buttonEvents.RightPressed += () =>
            {
                LcdConsole.WriteLine("starting moveForwardUntil");
                RoboTest.MoveForwardUntil();
                LcdConsole.WriteLine("moveForwardUntil done");
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
