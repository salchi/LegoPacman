using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoBrickFirmware.Sensors;
using MonoBrickFirmware.Movement;
using MonoBrickFirmware.Display;
using System.Threading;
using LegoTest2;

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
        private const int BOUND_STOP_SPINNING = 2;
        private const int SPEED_MAX = 100;
        private const int SPEED_INTERMEDIATE = 50;
        private const int SPEED_LOW = 15;

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

        public void AlignAlongRightSide()
        {
            LegoUtils.PrintAndWait(3, "starting align");
            int distance = infraredSensor.ReadDistance();
            LcdConsole.WriteLine("initial distance: {0}", distance);

            Rotate(3, RotationDirection.Right);

            int tempDistance = infraredSensor.ReadDistance();
            LcdConsole.WriteLine("second distance: {0}", tempDistance);
            if (tempDistance > distance)
            {
                vehicle.SpinLeft(SPEED_LOW);
            }
            else
            {
                vehicle.SpinRight(SPEED_LOW);
            }

            int distanceDelta = 0;
            int oldDistance = infraredSensor.ReadDistance();
            int newDistance;

            while (distanceDelta <= 0)
            {
                newDistance = infraredSensor.ReadDistance();
                distanceDelta = newDistance - oldDistance;
                LegoUtils.PrintAndWait(2, "old: {0} new: {1} delta: {2}", oldDistance, newDistance, distanceDelta);
                oldDistance = newDistance;
            }

            LegoUtils.PrintAndWait(3, "align finished");
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
            LegoUtils.PrintAndWait(3, "starting align");
            int distance = infraredSensor.ReadDistance();
            LcdConsole.WriteLine("initial distance: {0}", distance);

            if (distance >= (FAST_DISTANCE_IN_CM + IR_TO_FRONT_IN_CM))
            {
                LegoUtils.PrintAndWait(3, "fast, distance = {0}", distance - IR_TO_FRONT_IN_CM - SLOW_DISTANCE_IN_CM));
                Rotate(90, RotationDirection.Left);
                MoveForwardByCm(distance - IR_TO_FRONT_IN_CM - SLOW_DISTANCE_IN_CM);
                Rotate(90 - ANGLE_TO_FENCE, RotationDirection.Left);
            }
            else
            {
                LegoUtils.PrintAndWait(3, "slow");
                Rotate(ANGLE_TO_FENCE, RotationDirection.Right);
            }

            distance = infraredSensor.ReadDistance();
            while (distance > TARGET_FENCE_DISTANCE)
            {
                vehicle.Forward(SPEED_INTERMEDIATE);
            }

            Rotate(ANGLE_TO_FENCE, RotationDirection.Left);
            LegoUtils.PrintAndWait(3, "finished moveToFence");
        }

        private const int SLEEP_TIME_IN_MS = 50;
        private const int CM_PER_TIME = 3;
        public void MoveForwardByCm(int cm, bool brakeOnFinish = true)
        {
            LegoUtils.PrintAndWait(3, "movecm: {0}", cm);
            vehicle.Forward(SPEED_MAX);

            for (int i = 0; i < cm; i += CM_PER_TIME)
            {
                LcdConsole.WriteLine("step: {0}", i);
                Thread.Sleep(SLEEP_TIME_IN_MS);
            }

            if (brakeOnFinish)
            {
                vehicle.Brake();
            }
            LegoUtils.PrintAndWait(3, "finished movecm");
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

        public void Rotate(int degrees, RotationDirection direction)
        {
            degrees %= 360;
            degrees = (direction == RotationDirection.Right) ? -degrees : degrees;

            LcdConsole.WriteLine("degrees: {0}", degrees);

            var currentAngle = ReadGyro();
            var targetAngle = GetTargetAngle(currentAngle, degrees);
            LcdConsole.WriteLine("target {0}", targetAngle);

            DoRotate(targetAngle, direction);
        }

        private int ReadGyro()
        {
            var angle = gyroSensor.Read();
            return (angle > 0) ? 360 + angle : Math.Abs(angle); 
        }

        private void Spin(sbyte speed, RotationDirection direction)
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

        private int GetDelta(int number1, int number2)
        {
            return Math.Abs(Math.Abs(number1) - Math.Abs(number2));
        }

        private void DoRotate(int targetAngle, RotationDirection direction)
        {
            var currentAngle = ReadGyro();
            var delta = GetDelta(targetAngle, currentAngle);

            LcdConsole.WriteLine("curr: {0}; delta:{1}",currentAngle, delta);
            Spin(GetRotatingSpeed(delta), direction);

            System.Threading.Thread.Sleep(4000);

            while (delta > BOUND_STOP_SPINNING)
            {
                currentAngle = ReadGyro();
                if (delta <= BOUND_REDUCE_SPEED)
                {
                    Spin(GetRotatingSpeed(delta), direction);
                }
            }

            vehicle.Brake();
        }
    }
}
