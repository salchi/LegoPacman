using MonoBrickFirmware.Display;
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

        public static void MoveForwardByCm(int distance)
        {
            roboter.MoveForwardByCm(distance);
        }

        public static void AlignAlongRightSide()
        {
            roboter.AlignAlongRightSide();
        }

        public static void MoveToFence()
        {
            roboter.MoveToFence();
        }
    }
}
