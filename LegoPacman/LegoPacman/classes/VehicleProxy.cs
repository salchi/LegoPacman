using MonoBrickFirmware.Display;
using MonoBrickFirmware.Movement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LegoPacman.classes
{
    class VehicleProxy
    {
        private Vehicle vehicle;
        private SensorProxy sensorProxy;

        public VehicleProxy(SensorProxy sensorProxy, MotorPort left, MotorPort right)
        {
            this.sensorProxy = sensorProxy;
            vehicle = new Vehicle(left, right);
        }

        public void Brake()
        {
            vehicle.Brake();
        }

        public void BrakeDelayed(int delayInMillis)
        {
            Thread.Sleep(delayInMillis);
            Brake();
        }

        public void ForwardByDegrees(sbyte speed, uint degrees, bool brakeOnFinish = true)
        {
            LcdConsole.WriteLine("moving forward: speed {0} deg {1} brake {2}", speed, degrees, brakeOnFinish);
            LegoUtils.WaitOnHandle(vehicle.Backward(speed, degrees, brakeOnFinish));
        }

        public void BackwardByDegrees(sbyte speed, uint degrees, bool brakeOnFinish = true)
        {
            LcdConsole.WriteLine("moving backward: speed {0} deg {1} brake {2}", speed, degrees, brakeOnFinish);
            LegoUtils.WaitOnHandle(vehicle.Forward(speed, degrees, brakeOnFinish));
        }


        public void BreakWhen<T>(Func<T> valueSource, Predicate<T> brakeCondition)
        {
            var value = valueSource();
            while (!brakeCondition(value))
            {
                value = valueSource();
            }

            vehicle.Brake();
        }

        public void MoveForward(sbyte speed)
        {
            vehicle.Backward(speed);
        }

        public void MoveBackward(sbyte speed)
        {
            vehicle.Forward(speed);
        }

        public void MoveForwardWhile<T>(Func<T> valueSource, Predicate<T> brakeCondition)
        {
            MoveForward(Velocity.Medium);
            BreakWhen(valueSource, LegoUtils.Negate(brakeCondition));
        }

        public void MoveBackwardWhile<T>(Func<T> valueSource, Predicate<T> brakeCondition)
        {
            MoveBackward(Velocity.Medium);
            BreakWhen(valueSource, LegoUtils.Negate(brakeCondition));
        }

        public void MoveForwardUntil<T>(Func<T> valueSource, Predicate<T> brakeCondition)
        {
            MoveForward(Velocity.Medium);
            BreakWhen(valueSource, brakeCondition);
        }

        public void MoveBackwardUntil<T>(Func<T> valueSource, Predicate<T> brakeCondition)
        {
            MoveBackward(Velocity.Medium);
            BreakWhen(valueSource, brakeCondition);
        }

        public void RotateLeftWhile<T>(Func<T> valueSource, Predicate<T> brakeCondition)
        {
            vehicle.SpinLeft(Velocity.Medium);
            BreakWhen(valueSource, LegoUtils.Negate(brakeCondition));
        }

        public void RotateRightWhile<T>(Func<T> valueSource, Predicate<T> brakeCondition)
        {
            vehicle.SpinRight(Velocity.Medium);
            BreakWhen(valueSource, LegoUtils.Negate(brakeCondition));
        }

        public void RotateLeftUntil<T>(Func<T> valueSource, Predicate<T> brakeCondition)
        {
            vehicle.SpinLeft(Velocity.Medium);
            BreakWhen(valueSource, brakeCondition);
        }

        public void RotateRightUntil<T>(Func<T> valueSource, Predicate<T> brakeCondition)
        {
            vehicle.SpinRight(Velocity.Medium);
            BreakWhen(valueSource, brakeCondition);
        }

        private const int BoundStopSpinning = 2;
        public bool NeedToStopSpinning(RotationDirection direction, int currentAngle, int targetAngle)
        {
            return LegoMath.AbsDelta(currentAngle, targetAngle) <= BoundStopSpinning;
        }


        private const int BoundReduceSpeed = 10;
        public sbyte GetRotatingSpeed(int delta)
        {
            return Convert.ToSByte((delta <= BoundReduceSpeed) ? Velocity.Lowest : Velocity.Highest);
        }

        public void SetRotatingSpeed(int delta, RotationDirection direction)
        {
            if (delta <= BoundReduceSpeed)
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

        public void RotateLeft(int degrees)
        {
            Rotate(degrees, RotationDirection.Left);
        }

        public void RotateRight(int degrees)
        {
            Rotate(degrees, RotationDirection.Right);
        }

        public void Rotate(int degrees, RotationDirection direction)
        {
            sensorProxy.ResetGyro();
            var currentAngle = sensorProxy.ReadGyro(direction);

            int targetAngle;
            if (direction == RotationDirection.Left)
            {
                targetAngle = degrees;
                vehicle.SpinLeft(GetRotatingSpeed(LegoMath.AbsDelta(currentAngle, targetAngle)));
            }
            else
            {
                targetAngle = 360 - degrees;
                vehicle.SpinRight(GetRotatingSpeed(LegoMath.AbsDelta(currentAngle, targetAngle)));
            }

            while (!NeedToStopSpinning(direction, currentAngle, targetAngle))
            {
                currentAngle = sensorProxy.ReadGyro(direction);
                SetRotatingSpeed(LegoMath.AbsDelta(currentAngle, targetAngle), direction);
            }

            vehicle.Brake();
        }

        public void TurnLeftForward(sbyte speed, sbyte turnPercentage, uint degrees, bool brake = true)
        {
            LegoUtils.WaitOnHandle(vehicle.TurnRightReverse(speed, turnPercentage, degrees, brake));
        }

        public void TurnMotorsOff()
        {
            vehicle.Off();
        }
    }
}
