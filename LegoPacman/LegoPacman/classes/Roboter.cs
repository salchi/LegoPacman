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
        private const MotorPort PORT_MOTOR_LEFT = MotorPort.OutD;
        private const MotorPort PORT_MOTOR_RIGHT = MotorPort.OutA;

        private const int BOUND_REDUCE_SPEED = 10;
        private const int BOUND_STOP_SPIN = 1;

        private EV3GyroSensor gyroSensor;
        private Vehicle vehicle;

        public Roboter()
        {
            gyroSensor = new EV3GyroSensor(PORT_GYRO, GyroMode.Angle);
            gyroSensor.Reset();
            vehicle = new Vehicle(PORT_MOTOR_LEFT, PORT_MOTOR_RIGHT);
        }

        public void Rotate(int degrees, RotationDirection direction)
        {
            degrees %= 360;
            degrees = (direction == RotationDirection.Right) ? -degrees : degrees;

            var currentAngle = ReadGyro();
            var targetAngle = GetTargetAngle(currentAngle, degrees);

            DoRotate(targetAngle, direction);
        }

        private int GetTargetAngle(int startAngle, int degreesToRotate)
        {
            var targetAngle = (startAngle + degreesToRotate) % 360;
            return (targetAngle < 0) ? targetAngle + 360 : targetAngle;
        }

        private sbyte GetRotatingSpeed(int delta)
        {
            return Convert.ToSByte((delta > BOUND_REDUCE_SPEED) ? 100 : 10);
        }

        private int GetAbsDelta(int number1, int number2)
        {
            return Math.Abs(Math.Abs(number1) - Math.Abs(number2));
        }

        private void DoRotate(int targetAngle, RotationDirection direction)
        {
            var currentAngle = ReadGyro();
            var delta = GetAbsDelta(targetAngle, currentAngle);

            Spin(direction, GetRotatingSpeed(delta));
            
            while (delta > BOUND_STOP_SPIN)
            {
                currentAngle = ReadGyro();
                delta = GetAbsDelta(targetAngle, currentAngle);
                if (delta <= BOUND_REDUCE_SPEED)
                {
                    Spin(direction, GetRotatingSpeed(delta));
                }
            }

            vehicle.Brake();
        }

        private int ReadGyro()
        {
            var angle = gyroSensor.Read();
            return (angle > 0) ? 360 - angle : Math.Abs(angle);
        }

        private void Spin(RotationDirection direction, sbyte speed)
        {
            switch (direction)
            {
                case RotationDirection.Left:
                    vehicle.SpinLeft(speed);
                    break;
                case RotationDirection.Right:
                    vehicle.SpinRight(speed);
                    break;
            }
        }
    }
}
