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

        public SensorProxy SensorProxy { get; }
        public VehicleProxy VehicleProxy { get; }
        private ColorAnalyzer colorAnalyzer;

        public Roboter()
        {
            SensorProxy = new SensorProxy(gyroPort: PortGyro, ultrasonicPort: PortUltrasonic, colorPort: PortColor);
            VehicleProxy = new VehicleProxy(SensorProxy, left: PortMotorLeft, right: PortMotorRight);
            colorAnalyzer = new ColorAnalyzer(new List<KnownColor>() { KnownColor.Fence_temp, KnownColor.Blue });
        }

        private void HandleReadColor()
        {
            var lastRead = colorAnalyzer.Analyze(SensorProxy.ColorReader.ReadColor());

            if (lastRead == KnownColor.Blue)
            {
                LcdConsole.WriteLine("got Blue");
                VehicleProxy.RotateLeft(90);
                MoveForwardByCm(10);
            }
            else if (lastRead == KnownColor.Invalid)
            {
                LcdConsole.WriteLine("got invalid color");
                VehicleProxy.RotateRight(10);
                MoveForwardByCm(5);
                VehicleProxy.RotateLeft(SensorProxy.ReadGyro(RotationDirection.Right));
                LegoUtils.PrintAndWait(2, "rotation done");
                FollowFence();
            }
        }

        public void FollowFence()
        {
            VehicleProxy.MoveForwardWhile(() => SensorProxy.ColorReader.ReadColor(), color => colorAnalyzer.Analyze(color) == KnownColor.Fence_temp);

            //HandleReadColor();
        }

        // in cm
        public void MoveToFence()
        {
            const int IrSensorFrontCenterDifference = 3;
            const int TurningBuffer = 10;
            const int CurveDistance = IrSensorFrontCenterDifference + TurningBuffer + 9;

            var distance = SensorProxy.ReadDistanceInCm();
            //LegoUtils.PrintAndWait(2, "initial distance: {0}", distance);

            double distanceToFence = distance - IrSensorFrontCenterDifference - TurningBuffer;
            //LcdConsole.WriteLine("fence drive distance: {0}", distanceToFence);

            VehicleProxy.RotateRight(90);
            MoveForwardByCm(distanceToFence, false);

            LegoUtils.PrintAndWait(2, "moved by cm");

            VehicleProxy.TurnLeftForward(Velocity.Medium, 100, LegoMath.CmToEngineDegrees(CurveDistance));
            LegoUtils.PrintAndWait(10, "turned left forward");

            MoveForwardByCm(3);

            LegoUtils.PrintAndWait(10, "moved forward again");

            var endDeg = SensorProxy.ReadGyro(RotationDirection.Left);
            LegoUtils.PrintAndWait(1, "rotleft deg: {0}", endDeg);
            VehicleProxy.RotateLeft(endDeg);
            LegoUtils.PrintAndWait(10, "finished moveToFence");
        }

        enum Direction
        {
            Forward, Backward
        }

        // in cm
        public void MoveForwardByCm(double cm, bool brakeOnFinish = true)
        {
            MoveByCm(cm, Direction.Forward, brakeOnFinish);
        }

        public void MoveBackwardByCm(double cm, bool brakeOnFinish = true)
        {
            MoveByCm(cm, Direction.Backward, brakeOnFinish);
        }

        private void MoveByCm(double cm, Direction direction, bool brakeOnFinish = true)
        {
            const int SlowThresholdDistance = 3;
            const double FastMomentumFactor = .85d;
            const double SlowMomentumFactor = .90d;
            const int FastBrakeAngle = 15;
            const int SlowBrakeDistance = 2;

            if (cm > SlowThresholdDistance)
            {
                uint cmCalcDeg = LegoMath.CmToEngineDegrees(cm - SlowThresholdDistance);

                uint fastDegrees = (uint)Math.Round((cmCalcDeg * FastMomentumFactor) - FastBrakeAngle);
                uint slowDegrees = LegoMath.CmToEngineDegrees(SlowThresholdDistance) - SlowBrakeDistance;

                LcdConsole.WriteLine("fastdeg: {0} slowdeg:{1}", fastDegrees, slowDegrees);

                MoveByDegrees(Velocity.Highest, fastDegrees, direction, false);
                MoveByDegrees(Velocity.Low, slowDegrees, direction, brakeOnFinish);
            }
            else
            {
                uint slowDegrees = (uint)Math.Round(LegoMath.CmToEngineDegrees(cm) * SlowMomentumFactor) - SlowBrakeDistance;
                LcdConsole.WriteLine("slow deg: {0}", slowDegrees);
                MoveByDegrees(Velocity.Low, slowDegrees, direction, brakeOnFinish);
            }
        }

        private void MoveByDegrees(sbyte speed, uint degrees, Direction direction, bool brakeOnFinish = true)
        {
            if (direction == Direction.Forward)
            {
                VehicleProxy.ForwardByDegrees(speed, degrees, brakeOnFinish);
            }
            else
            {
                VehicleProxy.BackwardByDegrees(speed, degrees, brakeOnFinish);
            }
        }

        public void TurnMotorsOff()
        {
            VehicleProxy.TurnMotorsOff();
        }
    }
}