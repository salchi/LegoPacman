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
            LcdConsole.WriteLine("right: FollowFence");
            LcdConsole.WriteLine("down: MoveToFence");

            buttonEvents.EnterPressed += () =>
            {
                LcdConsole.Clear();
                LcdConsole.WriteLine("up: Rotation");
                LcdConsole.WriteLine("left: MoveForwardByCm");
                LcdConsole.WriteLine("right: FollowFence");
                LcdConsole.WriteLine("down: MoveToFence");
            };

            buttonEvents.UpPressed += () =>
            {
                LegoUtils.PrintAndWait(2, "starting Rotation");
                RoboTest.Rotation(90, 360, 90);
                LegoUtils.PrintAndWait(2, "Rotation done");
            };

            buttonEvents.LeftPressed += () =>
            {
                LegoUtils.PrintAndWait(2, "starting MoveForwardByCm");
                LegoUtils.PrintAndWait(6, "1");
                RoboTest.MoveForwardByCm(1);
                LegoUtils.PrintAndWait(6, "2");
                RoboTest.MoveForwardByCm(2);
                LegoUtils.PrintAndWait(6, "3");
                RoboTest.MoveForwardByCm(3);
                LegoUtils.PrintAndWait(6, "5");
                RoboTest.MoveForwardByCm(5);
                LegoUtils.PrintAndWait(6, "10");
                RoboTest.MoveForwardByCm(10);
                LegoUtils.PrintAndWait(6, "20");
                RoboTest.MoveForwardByCm(20);
                LegoUtils.PrintAndWait(2, "MoveForwardByCm done");
            };

            buttonEvents.RightPressed += () =>
            {
                LegoUtils.PrintAndWait(2, "starting FollowFence");
                RoboTest.FollowFence();
                LegoUtils.PrintAndWait(2, "FollowFence done");
            };

            buttonEvents.DownPressed += () =>
            {
                LegoUtils.PrintAndWait(2, "starting MoveToFence");
                RoboTest.MoveToFence();
                LegoUtils.PrintAndWait(2, "MoveToFence done");
            };

            terminateProgram.WaitOne();
        }
    }
}
