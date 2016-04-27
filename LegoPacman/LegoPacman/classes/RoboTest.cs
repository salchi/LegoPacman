using MonoBrickFirmware.Display;
using MonoBrickFirmware.Movement;
using MonoBrickFirmware.Sensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LegoPacman.classes
{
    static class RoboTest
    {
        private static Roboter roboter = new Roboter();

        public static void FollowFence()
        {
            roboter.FollowFence();
        }

        public static void Rotation(int start, int end, int step)
        {
            for (int i = start; i <= end; i += step)
            {
                roboter.VehicleProxy.RotateLeft(i);
                roboter.VehicleProxy.RotateRight(i);
            }
        }

        public static void TestUltrasonic()
        {
            var sensor = new EV3UltrasonicSensor(SensorPort.In3, UltraSonicMode.Centimeter);
            var val = sensor.Read();
            while (val >= 1)
            {
                LcdConsole.WriteLine("distance: {0}", val);
                val = sensor.Read();
            }
        }

        public static void ForwardBackward()
        {
            LcdConsole.WriteLine("forward with speed 20"); // b
            roboter.VehicleProxy.MoveForward(20);
            Thread.Sleep(2000);

            LcdConsole.WriteLine("forward 360 deg"); // f
            LegoUtils.WaitOnHandle(roboter.VehicleProxy.Vehicle.Forward(Velocity.Medium, 360, true));
            Thread.Sleep(2000);

            LcdConsole.WriteLine("backward 360 deg"); //  f
            LegoUtils.WaitOnHandle(roboter.VehicleProxy.Vehicle.Backward(Velocity.Medium, 360, true));
            Thread.Sleep(2000);

            LcdConsole.WriteLine("backward with speed 20"); // b
            roboter.VehicleProxy.MoveBackward(20);
            Thread.Sleep(2000);

            roboter.TurnMotorsOff();
        }

        public static void MoveForwardByCm(int distance)
        {
            for (int i = 5; i <= 50; i+=5)
            {
                LegoUtils.PrintAndWait(2, "distance: {0}", i);
                roboter.MoveForwardByCm(i);
                roboter.MoveBackwardByCm(i);
            }
        }

        public static void MoveToFence()
        {
            roboter.MoveToFence();
        }

        public static void TurnOffMotors()
        {
            roboter.TurnMotorsOff();
        }
    }
}
