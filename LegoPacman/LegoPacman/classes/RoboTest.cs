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

        public static void GetColor()
        {
            roboter.SensorProxy.ColorReader.TryRead();
            var analyzer = new ColorAnalyzer(new List<KnownColor>() { KnownColor.Fence_temp, KnownColor.Blue, KnownColor.Red, KnownColor.White, KnownColor.Yellow });
            LcdConsole.WriteLine(analyzer.Analyze(roboter.SensorProxy.ColorReader.LastRead).Name);
        }

        public static void Rotation(int start, int end, int step)
        {
            for (int i = start; i <= end; i += step)
            {
                roboter.VehicleProxy.RotateLeft(i);
                roboter.VehicleProxy.RotateRight(i);
            }
        }

        public static void Rotate(int degrees, RotationDirection direction)
        {
            roboter.VehicleProxy.Rotate(degrees, direction);
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

        public static void ReadColor()
        {
            roboter.SensorProxy.ColorReader.TryRead();
            LcdConsole.WriteLine(RGBColorHelper.ToString(roboter.SensorProxy.ColorReader.LastRead));
        }

        public static void MoveTest(int distance)
        {
            LcdConsole.WriteLine("forward distance: {0}", distance);
            roboter.MoveForwardByCm(distance);
            Thread.Sleep(2000);

            LcdConsole.WriteLine("backward distance: {0}", distance);
            roboter.MoveBackwardByCm(distance);
        }

        public static void MoveToFence()
        {
            roboter.MoveToFence();
        }

        public static void TurnOffMotors()
        {
            roboter.TurnMotorsOff();
        }

        public static void MoveForwardWhile()
        {
            roboter.VehicleProxy.MoveForwardWhile(roboter.SensorProxy.ReadDistanceInCm, x => x > 10);
        }

        public static void MoveForwardUntil()
        {
            roboter.VehicleProxy.MoveForwardUntil(roboter.SensorProxy.ReadDistanceInCm, x => x < 10);
        }
    }
}
