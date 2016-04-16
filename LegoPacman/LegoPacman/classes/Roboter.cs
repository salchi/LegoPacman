using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoBrickFirmware.Sensors;
using MonoBrickFirmware.Movement;
using MonoBrickFirmware.Display;
using System.Threading;

namespace LegoPacman.classes
{
    enum RotationDirection
    {
        Left,Right
    }

    class Roboter
    {
        private const SensorPort PORT_GYRO = SensorPort.In2;
        private const SensorPort PORT_INFRARED = SensorPort.In3;
        private const MotorPort PORT_MOTOR_LEFT = MotorPort.OutA;
        private const MotorPort PORT_MOTOR_RIGHT = MotorPort.OutD;

        private const int BOUND_REDUCE_SPEED = 5;

        private EV3GyroSensor gyroSensor;
        private EV3IRSensor infraredSensor;
        private Vehicle vehicle;

        public Roboter()
        {
            gyroSensor = new EV3GyroSensor(PORT_GYRO, GyroMode.Angle);
            gyroSensor.Reset();
            infraredSensor = new EV3IRSensor(PORT_INFRARED, IRMode.Proximity);
            vehicle = new Vehicle(PORT_MOTOR_LEFT, PORT_MOTOR_RIGHT);
        }

        public void Rotate(int degrees)
        {
            degrees %= 360;
            degrees = (degrees < 0) ? degrees + 360 : degrees;

            LcdConsole.WriteLine("degrees: {0}", degrees);

            var currentAngle = gyroSensor.Read();
            var targetAngle = GetTargetAngle(currentAngle, degrees);
            var delta = targetAngle - currentAngle;

            if (delta <= 180 && delta > 0)
            {
                LcdConsole.WriteLine("rotating left... delta {0}", delta);
                DoRotate(targetAngle, RotationDirection.Left);
            }
            else if(delta < 0 && delta >= -180)
            {
                LcdConsole.WriteLine("rotating right... delta {0}", delta);
                DoRotate(targetAngle, RotationDirection.Right);
            }
        }

        // in cm
        private const int FAST_THRESHOLD_IN_CM = 20;
        private const int IR_TO_FRONT_IN_CM = 15;
        public void MoveToFence()
        {
            int initialDistance = infraredSensor.ReadDistance();

            if (initialDistance >= (FAST_THRESHOLD_IN_CM + IR_TO_FRONT_IN_CM))
            {

            }
        }

        private const int SLEEP_TIME_IN_MS = 50;
        private const int CM_PER_TIME = 3;
        public void MoveForwardByCm(int cm, bool brakeOnFinish = true)
        {
            vehicle.Forward(100);

            for (int i = 0, n = cm / CM_PER_TIME; i < n; i++)
            {
                Thread.Sleep(SLEEP_TIME_IN_MS);
            }

            if (brakeOnFinish)
            {
                vehicle.Brake();
            }
        }

        private int GetTargetAngle(int startAngle, int degreesToRotate)
        {
            return (startAngle + degreesToRotate) % 360;
        }

        private sbyte GetRotatingSpeed(int delta)
        {
            return Convert.ToSByte((delta > BOUND_REDUCE_SPEED) ? 100 : 10);
        }

        private void DoRotate(int targetAngle, RotationDirection direction)
        {
            var currentAngle = gyroSensor.Read();
            var speed = GetRotatingSpeed(Math.Abs(targetAngle - currentAngle));

            LcdConsole.WriteLine("currentAngle: {0}; speed:{1}",currentAngle, speed);
            switch (direction)
            {
                case RotationDirection.Left:
                    vehicle.SpinLeft(speed);
                    break;
                case RotationDirection.Right:
                    vehicle.SpinRight(speed);
                    break;
            }

            while (currentAngle != targetAngle)
            {
                currentAngle = gyroSensor.Read();
            }

            vehicle.Brake();
        }
    }
}
