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
        Left, Right
    }

    static class Velocity
    {
        public const int Highest = 100;
        public const int High = 75;
        public const int Medium = 50;
        public const int Low = 35;
        public const int Lowest = 10;
    }

    class Roboter
    {
        private const SensorPort PortGyro = SensorPort.In2;
        private const SensorPort PortUltrasonic = SensorPort.In3;
        private const SensorPort PortColor = SensorPort.In4;
        private const MotorPort PortMotorLeft = MotorPort.OutD;
        private const MotorPort PortMotorRight = MotorPort.OutA;

        private EV3GyroSensor gyroSensor = new EV3GyroSensor(PortGyro, GyroMode.Angle);
        private EV3UltrasonicSensor ultrasonicSensor = new EV3UltrasonicSensor(PortUltrasonic, UltraSonicMode.Centimeter);
        
        private ColorReader colorReader = new ColorReader(PortColor);
        private ColorAnalyzer colorAnalyzer = new ColorAnalyzer(new List<KnownColor>() { KnownColor.Fence_temp, KnownColor.Blue });

        private Vehicle vehicle = new Vehicle(PortMotorLeft, PortMotorRight);

        private void ForwardByDegrees(sbyte speed, uint degrees, bool brakeOnFinish = true)
        {
            LcdConsole.WriteLine("moving forward: speed {0} deg {1} brake {2}", speed, degrees, brakeOnFinish);
            LegoUtils.WaitOnHandle(vehicle.Backward(speed, degrees, brakeOnFinish));
        }

        private void BackwardByDegrees(sbyte speed, uint degrees, bool brakeOnFinish = true)
        {
            LcdConsole.WriteLine("moving backward: speed {0} deg {1} brake {2}", speed, degrees, brakeOnFinish);
            LegoUtils.WaitOnHandle(vehicle.Forward(speed, degrees, brakeOnFinish));
        }

        private void MoveForward(sbyte speed)
        {
            vehicle.Backward(speed);
        }

        private void MoveBackward(sbyte speed)
        {
            vehicle.Forward(speed);
        }

        private const int MaxTries = 100;
        // the ultrasonic sensor seemingly returns the distance in millimeters
        private const double MmToCmFactor = 0.1d;
        private double ReadDistanceInCm()
        {
            var val = ultrasonicSensor.Read();
            var tries = 0;
            while (val == 0 && tries < MaxTries)
            {
                val = ultrasonicSensor.Read();
                tries++;
                Thread.Sleep(20);
            }

            LcdConsole.WriteLine("read dist {0}, {1} tries", val * MmToCmFactor);

            return val * MmToCmFactor;
        }

        private void HandleReadColor()
        {
            var lastRead = colorAnalyzer.Analyze(colorReader.LastRead);

            if (lastRead == KnownColor.Blue)
            {
                LcdConsole.WriteLine("got Blue!!");
                RotateLeft(90);
                MoveForwardByCm(20);
            }
            else if (lastRead == KnownColor.Invalid)
            {
                LcdConsole.WriteLine("got invalid color!!");
                RotateRight(10);
                MoveForwardByCm(5);
                RotateLeft(ReadGyro(RotationDirection.Right));
                FollowFence();
            }
        }

        public void FollowFence()
        {
            MoveForward(Velocity.Medium);
            colorReader.TryRead();

            while (colorAnalyzer.Analyze(colorReader.LastRead) == KnownColor.Fence_temp)
            {
                colorReader.TryRead();                
            }

            vehicle.Brake();
            HandleReadColor();
        }

        // in cm
        private const int IrSensorFrontCenterDifference = 3;
        private const int TurningBuffer = 3;
        private const int AngleToFence = 45;
        public void MoveToFence()
        {
            var distance = LegoMath.DoubleToInt(ReadDistanceInCm());
            LegoUtils.PrintAndWait(2, "initial distance: {0}", distance);

            int distanceToFence = distance - IrSensorFrontCenterDifference - TurningBuffer;
            LcdConsole.WriteLine("fence drive distance: {0}", distanceToFence);

            RotateRight(90);
            MoveForwardByCm(distanceToFence, false);

            LegoUtils.WaitOnHandle(vehicle.TurnRightReverse(Velocity.Medium, 100, LegoMath.CmToEngineDegrees(IrSensorFrontCenterDifference + TurningBuffer), true));

            RotateLeft(AngleToFence);
            LegoUtils.PrintAndWait(3, "finished moveToFence");
        }

        // in cm
        private const int SlowThresholdDistance = 3;
        private const double FastMomentumFactor = .85d;
        private const double SlowMomentumFactor = .90d;
        private const int FastBrakeAngle = 15;
        private const int SlowBrakeDistance = 2;
        public void MoveForwardByCm(int cm, bool brakeOnFinish = true)
        {
            if (cm > SlowThresholdDistance)
            {
                uint cmCalcDeg = LegoMath.CmToEngineDegrees(cm - SlowThresholdDistance);
  
                uint fastDegrees = (uint)Math.Round((cmCalcDeg * FastMomentumFactor) - FastBrakeAngle);
                uint slowDegrees = LegoMath.CmToEngineDegrees(SlowThresholdDistance) - SlowBrakeDistance;

                LcdConsole.WriteLine("fastdeg: {0} slowdeg:{1}", fastDegrees, slowDegrees);

                ForwardByDegrees(Velocity.Highest, fastDegrees, false);
                ForwardByDegrees(Velocity.Lowest, slowDegrees);
            }
            else
            {
                uint slowDegrees = (uint)Math.Round(LegoMath.CmToEngineDegrees(cm) * SlowMomentumFactor) - SlowBrakeDistance;
                LcdConsole.WriteLine("slow deg: {0}", slowDegrees);
                ForwardByDegrees(Velocity.Lowest, slowDegrees);
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

        private const int BoundStopSpinning = 2;
        private bool NeedToStopSpinning(RotationDirection direction, int currentAngle, int targetAngle)
        {
            return LegoMath.AbsDelta(currentAngle, targetAngle) <= BoundStopSpinning;
        }


        private const int BoundReduceSpeed = 10;
        private sbyte GetRotatingSpeed(int delta)
        {
            return Convert.ToSByte((delta <= BoundReduceSpeed) ? Velocity.Lowest : Velocity.Highest);
        }

        private void SetRotatingSpeed(int delta, RotationDirection direction)
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
            gyroSensor.Reset();
            var currentAngle = ReadGyro(direction);

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
                currentAngle = ReadGyro(direction);
                SetRotatingSpeed(LegoMath.AbsDelta(currentAngle, targetAngle), direction);
            }

            vehicle.Brake();
        }
    }
}
