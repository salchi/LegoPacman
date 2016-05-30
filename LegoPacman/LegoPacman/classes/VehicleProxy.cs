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
        public Vehicle Vehicle { get; }
        private SensorProxy sensorProxy;

        private const int AngleReduceSpeed = 10;

        public VehicleProxy(SensorProxy sensorProxy, MotorPort left, MotorPort right)
        {
            this.sensorProxy = sensorProxy;
            Vehicle = new Vehicle(left, right);
        }

        public void Brake()
        {
            Vehicle.Brake();
        }

        public void ForwardByDegrees(sbyte speed, uint degrees, bool brakeOnFinish = true)
        {
            LcdConsole.WriteLine("mov forward speed{0} deg{1} brake{2}", speed, degrees, brakeOnFinish);
            LegoUtils.WaitOnHandle(Vehicle.Forward(speed, degrees, brakeOnFinish));
        }

        public void BackwardByDegrees(sbyte speed, uint degrees, bool brakeOnFinish = true)
        {
            LcdConsole.WriteLine("mov backward speed{0} deg{1} brake{2}", speed, degrees, brakeOnFinish);
            LegoUtils.WaitOnHandle(Vehicle.Backward(speed, degrees, brakeOnFinish));
        }

        public void BreakWhen<T>(Func<T> valueSource, Predicate<T> brakeCondition)
        {
            var value = valueSource();
            while (!brakeCondition(value))
            {
                value = valueSource();
            }

            Vehicle.Brake();
        }

        public void MoveForward(sbyte speed)
        {
            Vehicle.Forward(speed);
        }

        public void MoveBackward(sbyte speed)
        {
            Vehicle.Backward(speed);
        }

        public void MoveForwardWhile<T>(Func<T> valueSource, Predicate<T> brakeCondition)
        {
            if (!brakeCondition(valueSource()))
            {
                return;
            }

            MoveForward(Velocity.Medium);
            BreakWhen(valueSource, LegoUtils.Negate(brakeCondition));
        }

        public void MoveBackwardWhile<T>(Func<T> valueSource, Predicate<T> brakeCondition)
        {
            if (!brakeCondition(valueSource()))
            {
                return;
            }

            MoveBackward(Velocity.Medium);
            BreakWhen(valueSource, LegoUtils.Negate(brakeCondition));
        }

        public void MoveForwardUntil<T>(Func<T> valueSource, Predicate<T> brakeCondition)
        {
            if (brakeCondition(valueSource()))
            {
                return;
            }

            MoveForward(Velocity.Medium);
            BreakWhen(valueSource, brakeCondition);
        }

        public void MoveBackwardUntil<T>(Func<T> valueSource, Predicate<T> brakeCondition)
        {
            if (brakeCondition(valueSource()))
            {
                return;
            }

            MoveBackward(Velocity.Medium);
            BreakWhen(valueSource, brakeCondition);
        }

        public void RotateLeftWhile<T>(Func<T> valueSource, Predicate<T> brakeCondition)
        {
            if (!brakeCondition(valueSource()))
            {
                return;
            }

            Vehicle.SpinLeft(Velocity.Medium);
            BreakWhen(valueSource, LegoUtils.Negate(brakeCondition));
        }

        public void RotateRightWhile<T>(Func<T> valueSource, Predicate<T> brakeCondition)
        {
            if (!brakeCondition(valueSource()))
            {
                return;
            }

            Vehicle.SpinRight(Velocity.Medium);
            BreakWhen(valueSource, LegoUtils.Negate(brakeCondition));
        }

        public void RotateLeftUntil<T>(Func<T> valueSource, Predicate<T> brakeCondition)
        {
            if (brakeCondition(valueSource()))
            {
                return;
            }

            Vehicle.SpinLeft(Velocity.Medium);
            BreakWhen(valueSource, brakeCondition);
        }

        public void RotateRightUntil<T>(Func<T> valueSource, Predicate<T> brakeCondition)
        {
            if (brakeCondition(valueSource()))
            {
                return;
            }

            Vehicle.SpinRight(Velocity.Medium);
            BreakWhen(valueSource, brakeCondition);
        }

        public sbyte GetRotatingSpeed(int delta)
        {
            return Convert.ToSByte((delta <= AngleReduceSpeed) ? Velocity.Lowest : Velocity.Highest);
        }

        public void RotateLeft(int degrees)
        {
            try
            {
                Rotate(degrees, RotationDirection.Left);
            }
            catch (Exception e)
            { 
                LegoUtils.PrintLongString(e.Message);
            }
        }

        public void RotateRight(int degrees)
        {
            try
            {
                Rotate(degrees, RotationDirection.Right);
            }
            catch (Exception e)
            {
                LegoUtils.PrintLongString(e.Message);
            }
        }

        public void Rotate(int degrees, RotationDirection direction)
        {
            if (degrees == 0)
            {
                return;
            }

            const int AngleStopSpinning = 3;

            sensorProxy.ResetGyro();
            var currentAngle = sensorProxy.ReadGyro(direction);

            int targetAngle;
            if (direction == RotationDirection.Left)
            {
                targetAngle = degrees;
                Vehicle.SpinLeft(GetRotatingSpeed(LegoMath.AbsDelta(currentAngle, targetAngle)));
            }
            else
            {
                targetAngle = 360 - degrees;
                Vehicle.SpinRight(GetRotatingSpeed(LegoMath.AbsDelta(currentAngle, targetAngle)));
            }

            while (LegoMath.AbsDelta(currentAngle, targetAngle) > AngleReduceSpeed)
            {
                currentAngle = sensorProxy.ReadGyro(direction);
            }

            Vehicle.Brake();

            if (direction == RotationDirection.Left)
            {
                Vehicle.SpinLeft(Velocity.Lowest);
            }
            else
            {
                Vehicle.SpinRight(Velocity.Lowest);
            }

            while (LegoMath.AbsDelta(currentAngle, targetAngle) > AngleStopSpinning)
            {
                currentAngle = sensorProxy.ReadGyro(direction);
            }

            Vehicle.Brake();
        }

        public void TurnLeftForward(sbyte speed, sbyte turnPercentage, uint degrees, bool brake = true)
        {
            LegoUtils.WaitOnHandle(Vehicle.TurnRightReverse(speed, turnPercentage, degrees, brake));
        }

        public void TurnRightForward(sbyte speed, sbyte turnPercentage, uint degrees, bool brake = true)
        {
            LegoUtils.WaitOnHandle(Vehicle.TurnLeftReverse(speed, turnPercentage, degrees, brake));
        }

        public void TurnMotorsOff()
        {
            Vehicle.Off();
        }
    }
}
