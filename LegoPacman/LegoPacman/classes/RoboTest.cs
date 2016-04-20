using MonoBrickFirmware.Display;
using MonoBrickFirmware.Movement;
using MonoBrickFirmware.Sensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                roboter.Rotate(i, RotationDirection.Left);
                roboter.Rotate(i, RotationDirection.Right);
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
            LcdConsole.WriteLine("reverseLEft {0}  reverseRight {1}", v.ReverseLeft, v.ReverseRight);
            LcdConsole.WriteLine("forward with speed");
            v.Forward(20);
            LcdConsole.WriteLine("forward with degrees");
            var handle = v.Forward(20, 400, true);
            handle.WaitOne();
            LcdConsole.WriteLine("backward with speed");
            v.Backward(20);
            LcdConsole.WriteLine("backward with degrees");
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
    }
}
