using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoBrickFirmware.Sensors;
using MonoBrickFirmware.Movement;
using MonoBrickFirmware.Display;

namespace LegoPacman.classes
{
    enum RotationDirection
    {
        Left,Right
    }

    class Roboter
    {
        private const SensorPort PORT_GYRO = SensorPort.In2;
        private const MotorPort PORT_MOTOR_LEFT = MotorPort.OutA;
        private const MotorPort PORT_MOTOR_RIGHT = MotorPort.OutD;

        private const int BOUND_REDUCE_SPEED = 5;

        private EV3GyroSensor gyroSensor;
        private Vehicle vehicle;

        public Roboter()
        {
            gyroSensor = new EV3GyroSensor(PORT_GYRO, GyroMode.Angle);
            gyroSensor.Reset();
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
