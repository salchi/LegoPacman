using LegoPacman.classes;
using LegoTest2;
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

            roboter = new Roboter();


            EV3GyroSensor gs = new EV3GyroSensor(SensorPort.In2);

            gs.Reset();
            LegoUtils.PrintAndWait(3, "initial gs val {0}", gs.Read());

            int targetAngle = 90;

            int currentAngle = gs.Read();

            Vehicle vehicle = new Vehicle(MotorPort.OutD, MotorPort.OutA);
            vehicle.SpinLeft(100);

            while (currentAngle < targetAngle - 5)
            {
                currentAngle = gs.Read();
                LcdConsole.WriteLine("current: {0}", currentAngle);
            }

            vehicle.Brake();

            LcdConsole.WriteLine("turn finished");

            /*
            TestRotation(90, RotationDirection.Left);
            TestRotation(90, RotationDirection.Right);
            TestRotation(180, RotationDirection.Left);
            TestRotation(180, RotationDirection.Right);
            TestRotation(360, RotationDirection.Left);
            TestRotation(360, RotationDirection.Right);
            TestRotation(45, RotationDirection.Left);
            TestRotation(45, RotationDirection.Right);
            */
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
