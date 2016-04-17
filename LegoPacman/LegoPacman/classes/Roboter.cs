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
        private const MotorPort PORT_MOTOR_LEFT = MotorPort.OutD;
        private const MotorPort PORT_MOTOR_RIGHT = MotorPort.OutA;
 
        private const int BOUND_REDUCE_SPEED = 10;
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

        private const int CORRECTION = 3;
        public void AlignAlongRightSide()
        {
            LcdConsole.WriteLine("starting align");
            var distance = infraredSensor.ReadDistance();
            LcdConsole.WriteLine("initial distance: {0}", distance);

            Rotate(3, RotationDirection.Right);

            var tempDistance = infraredSensor.ReadDistance();
            LcdConsole.WriteLine("second distance: {0}", tempDistance);

            var rotationDirection = RotationDirection.Right;
            if (tempDistance > distance)
            {
                vehicle.SpinLeft(SPEED_LOW);
                rotationDirection = RotationDirection.Left;
            }
            else
            {
                vehicle.SpinRight(SPEED_LOW);
            }

            var distanceDelta = 0;
            var oldDistance = infraredSensor.ReadDistance();
            int newDistance;

            while (distanceDelta <= 0)
            {
                newDistance = infraredSensor.ReadDistance();
                distanceDelta = newDistance - oldDistance;
                LcdConsole.WriteLine("old: {0} new: {1} delta: {2}", oldDistance, newDistance, distanceDelta);
                oldDistance = newDistance;
            }

            vehicle.Brake();
            LcdConsole.WriteLine("align finished");
            Rotate(CORRECTION, (rotationDirection == RotationDirection.Left) ? RotationDirection.Right : RotationDirection.Left);
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
            var distance = infraredSensor.ReadDistance();
            LcdConsole.WriteLine("initial distance: {0}", distance);

            if (distance >= (FAST_DISTANCE_IN_CM + IR_TO_FRONT_IN_CM))
            {
                LegoUtils.PrintAndWait(3, "fast, distance = {0}", distance - IR_TO_FRONT_IN_CM - SLOW_DISTANCE_IN_CM);
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

        private void ForwardByDegrees(sbyte speed, uint degrees, bool brakeOnFinish = true)
        {
            LcdConsole.WriteLine("speed: {0} deg: {1}");
            WaitHandle handle = vehicle.Backward(speed, degrees, brakeOnFinish);
            LcdConsole.WriteLine("made handle, started");
            handle.WaitOne();
            LcdConsole.WriteLine("handle waited");
        }

        private const int SLOW_THRESHOLD_IN_CM = 1;
        public void MoveForwardByCm(int cm, bool brakeOnFinish = true)
        {
            if (cm > 2)
            {
                uint fastDegrees = LegoUtils.CmToEngineDegrees(cm - SLOW_THRESHOLD_IN_CM);
                uint slowDegrees = LegoUtils.CmToEngineDegrees(SLOW_THRESHOLD_IN_CM);

                ForwardByDegrees(SPEED_MAX, fastDegrees);
                ForwardByDegrees(SPEED_LOW, slowDegrees);
            }
            else
            {
                uint slowDegrees = LegoUtils.CmToEngineDegrees(cm);
                LcdConsole.WriteLine("slow deg: {0}", slowDegrees);
                ForwardByDegrees(SPEED_LOW, slowDegrees);
            }
        }

        private int ReadGyro(RotationDirection direction)
        {
            if (direction == RotationDirection.Left)
            {
                return Math.Abs(gyroSensor.Read());
            }
            else
            {
                return 360 - gyroSensor.Read();
            }
        }

        private bool NeedToStopSpinning(RotationDirection direction, int currentAngle, int targetAngle)
        {
            return getAbsDelta(currentAngle, targetAngle) <= BOUND_STOP_SPINNING;
        }

        private int getAbsDelta(int currentAngle, int targetAngle)
        {
            return Math.Abs(currentAngle - targetAngle);
        }

        private sbyte GetRotatingSpeed(int delta)
        {
            return (delta <= BOUND_REDUCE_SPEED) ? Convert.ToSByte(SPEED_LOW) : Convert.ToSByte(SPEED_MAX);
        }

        private void SetRotatingSpeed(int delta, RotationDirection direction)
        {
            if (delta <= BOUND_REDUCE_SPEED)
            {
                if (direction == RotationDirection.Left)
                {
                    vehicle.SpinLeft(GetRotatingSpeed(delta));
                }
                else
                {
                    vehicle.SpinRight(GetRotatingSpeed(delta));
                }
            }
        }

        public void Rotate(int degrees, RotationDirection direction)
        {
            gyroSensor.Reset();

            var currentAngle = ReadGyro(direction);

            int targetAngle;
            if (direction == RotationDirection.Left)
            {
                targetAngle = degrees;
                vehicle.SpinLeft(GetRotatingSpeed(getAbsDelta(currentAngle, targetAngle)));
            }
            else
            {
                targetAngle = 360 - degrees;
                vehicle.SpinRight(GetRotatingSpeed(getAbsDelta(currentAngle, targetAngle)));
            }

            while (!NeedToStopSpinning(direction, currentAngle, targetAngle))
            {
                currentAngle = ReadGyro(direction);
                SetRotatingSpeed(getAbsDelta(currentAngle, targetAngle), direction);
            }
            vehicle.Brake();
        }
    }
}
