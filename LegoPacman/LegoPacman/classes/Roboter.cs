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
        private const MotorPort PORT_MOTOR_LEFT = MotorPort.OutA;
        private const MotorPort PORT_MOTOR_RIGHT = MotorPort.OutD;
    
        private const int BOUND_REDUCE_SPEED = 5;
        private const int SPEED_MAX = 100;
        private const int SPEED_INTERMEDIATE = 50;
        private const int SPEED_LOW = 15;

        private EV3GyroSensor gyroSensor;
        private Vehicle vehicle;

        public Roboter()
        {
            gyroSensor = new EV3GyroSensor(PORT_GYRO, GyroMode.Angle);
            gyroSensor.Reset();
            infraredSensor = new EV3IRSensor(PORT_INFRARED, IRMode.Proximity);
            vehicle = new Vehicle(PORT_MOTOR_LEFT, PORT_MOTOR_RIGHT);
        }

        public void AlignAlongRightSide()
        {
            int distance = infraredSensor.ReadDistance();

            RotateRight(3);

            int angleDelta = 0;
            int oldDistance = infraredSensor.ReadDistance();
            int newDistance;

            if (infraredSensor.ReadDistance() > distance)
            {
                vehicle.SpinLeft(SPEED_LOW);
            }
            else
            {
                vehicle.SpinRight(SPEED_LOW);
            }

            while (angleDelta <= 0)
            {
                newDistance = infraredSensor.ReadDistance();
                angleDelta = newDistance - oldDistance;
                oldDistance = newDistance;
            }

            vehicle.Brake();
        }

        // in cm
        private const int FAST_DISTANCE_IN_CM = 20;
        private const int IR_TO_FRONT_IN_CM = 18;
        private const int SLOW_DISTANCE_IN_CM = 5;
        private const int ANGLE_TO_FENCE = 10;
        private const int TARGET_FENCE_DISTANCE = 2;
        public void MoveToFence()
        {
            int distance = infraredSensor.ReadDistance();

            if (distance >= (FAST_DISTANCE_IN_CM + IR_TO_FRONT_IN_CM))
            {
                RotateRight(90);
                MoveForwardByCm(distance - IR_TO_FRONT_IN_CM - SLOW_DISTANCE_IN_CM);
                RotateLeft(90 - ANGLE_TO_FENCE);
            }
            else
            {
                RotateRight(ANGLE_TO_FENCE);
            }

            distance = infraredSensor.ReadDistance();
            while (distance > TARGET_FENCE_DISTANCE)
            {
                vehicle.Forward(SPEED_INTERMEDIATE);
            }

            RotateLeft(ANGLE_TO_FENCE);
        }

        private const int SLEEP_TIME_IN_MS = 50;
        private const int CM_PER_TIME = 3;
        public void MoveForwardByCm(int cm, bool brakeOnFinish = true)
        {
            vehicle.Forward(SPEED_MAX);

            for (int i = 0; i < cm; i += CM_PER_TIME)
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
