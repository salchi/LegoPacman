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
            var v = new Vehicle(MotorPort.OutD, MotorPort.OutA);

            LcdConsole.WriteLine("isLeftReversed {0}  isRightReversed {1}", v.ReverseLeft, v.ReverseRight);

            LcdConsole.WriteLine("forward with speed 20");
            v.Forward(20);
            Thread.Sleep(2000);

            LcdConsole.WriteLine("forward for 400 degrees");
            var handle = v.Forward(20, 400, true);
            handle.WaitOne();

            LcdConsole.WriteLine("backward with speed 20");
            v.Backward(20);
            Thread.Sleep(2000);

            LcdConsole.WriteLine("backward for 400 degrees");
            handle = v.Backward(20,400,true);
            handle.WaitOne();
        }

        public static void MoveForwardByCm(int distance)
        {
            roboter.MoveForwardByCm(distance);
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
