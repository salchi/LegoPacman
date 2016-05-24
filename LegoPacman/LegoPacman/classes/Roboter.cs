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
        //C
        private const SensorPort PortGyro = SensorPort.In2;
        private const SensorPort PortUltrasonic = SensorPort.In3;
        private const SensorPort PortColor = SensorPort.In4;

        private const MotorPort PortMotorLeft = MotorPort.OutD;
        private const MotorPort PortMotorRight = MotorPort.OutA;
        private const MotorPort PortMotorPrincess = MotorPort.OutB;
        private const MotorPort PortMotor = MotorPort.OutC;

        public SensorProxy SensorProxy { get; }
        public VehicleProxy VehicleProxy { get; }
        private ColorAnalyzer colorAnalyzer;

        public Roboter()
        {
            SensorProxy = new SensorProxy(gyroPort: PortGyro, ultrasonicPort: PortUltrasonic, colorPort: PortColor);
            VehicleProxy = new VehicleProxy(SensorProxy, left: PortMotorLeft, right: PortMotorRight);
            colorAnalyzer = new ColorAnalyzer(new List<KnownColor>() { KnownColor.Blue, KnownColor.Red, KnownColor.White, KnownColor.Yellow, KnownColor.FenceLight, KnownColor.FenceDark, KnownColor.Invalid });
        }

        private void HandleReadColor()
        {
            var lastRead = colorAnalyzer.Analyze(SensorProxy.ColorReader.ReadColor());

            if (lastRead == KnownColor.Blue)
            {
                LcdConsole.WriteLine("got Blue");
                RightTurnAction();
            }
            else if (lastRead == KnownColor.Red)
            {
                LcdConsole.WriteLine("got Red");
                LeftTurnAction();
            }
            else if (lastRead == KnownColor.White)
            {
                LcdConsole.WriteLine("got White");
                StraightAheadAction();
            }
            else if (lastRead == KnownColor.Yellow)
            {
                LcdConsole.WriteLine("got Yellow");
                TurnBackAction();
            }
            else if (lastRead == KnownColor.Invalid)
            {
                LcdConsole.WriteLine("got invalid color");
                VehicleProxy.RotateRight(10);
                MoveForwardByCm(5);
                VehicleProxy.RotateLeft(SensorProxy.ReadGyro(RotationDirection.Right) - 1);
            }

            FollowFence();
        }

        public void Grab()
        {
            var m = new Motor(PortMotorPrincess);
            m.SetSpeed(10);
            Thread.Sleep(1000);
            m.Brake();
        }

        public void Collect()
        {
            var m = new Motor(PortMotor);
            m.SetSpeed(10);
            Thread.Sleep(1000);
            m.Brake();
        }

        public void FollowFence()
        {
            VehicleProxy.MoveForwardUntil(
                SensorProxy.ColorReader.ReadColor,
                color => {
                    var kc = colorAnalyzer.Analyze(color);
                    return KnownColor.IsActionColor(kc) || kc.Equals(KnownColor.Invalid);
                    }
                );

            HandleReadColor();
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
            const int SlowThresholdDistance = 4;
            const double FastMomentumFactor = .85d;
            const double SlowMomentumFactor = .90d;
            const int FastBrakeAngle = 15;
            const int SlowBrakeDistance = 1;

            var degrees = (cm >= 10) ? LegoMath.CmToEngineDegrees(cm) * 8 / 7 : LegoMath.CmToEngineDegrees(cm);
            MoveByDegrees(Velocity.Low, degrees, direction, brakeOnFinish);

            /*if (cm > SlowThresholdDistance)
            {
                uint cmCalcDeg = LegoMath.CmToEngineDegrees(cm - SlowThresholdDistance);

                uint fastDegrees = (uint)Math.Round((cmCalcDeg * FastMomentumFactor) - FastBrakeAngle);
                uint slowDegrees = LegoMath.CmToEngineDegrees(SlowThresholdDistance) - SlowBrakeDistance;

                LcdConsole.WriteLine("fastdeg:{0} slowdeg:{1}", fastDegrees, slowDegrees);

                MoveByDegrees(Velocity.Medium, fastDegrees, direction, false);
                MoveByDegrees(Velocity.Low, slowDegrees, direction, brakeOnFinish);
            }
            else
            {
                uint slowDegrees = (uint)Math.Max(Math.Round(LegoMath.CmToEngineDegrees(cm) * SlowMomentumFactor) - SlowBrakeDistance, 0);
                LcdConsole.WriteLine("slow deg: {0}", slowDegrees);
                if (slowDegrees != 0)
                {
                    MoveByDegrees(Velocity.Low, slowDegrees, direction, brakeOnFinish);
                }
            }*/
        }

        private void MoveByDegrees(sbyte speed, uint degrees, Direction direction, bool brakeOnFinish = true)
        {
            if (degrees > 0)
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
        }

        const int VehicleLength = 25;
        const int VehicleWidth = 16;
        const int FenceLength = 33;
        const int ColorWidth = 3;
        const int FenceWidth = 3;
        const int ColorSensorToCenter = 10;
        const int ColorSensorFrontToBack = 5;
        const int CrossingWidth = FenceLength - FenceWidth;
        public void RightTurnAction()
        {
            VehicleProxy.RotateRight(92);
            MoveForwardByCm(VehicleLength/2 + ColorSensorToCenter);
        }

        public void LeftTurnAction()
        {
            MoveForwardByCm(ColorSensorFrontToBack);
            var hyp = Math.Sqrt(Math.Pow(FenceLength, 2) + Math.Pow(FenceLength * 1 / 6, 2));
            var alpha = Math.Asin(FenceLength / hyp)/(2*Math.PI) * 360 - 8;
            LcdConsole.WriteLine("hyp {0}", hyp);
            LcdConsole.WriteLine("alpha {0}", alpha);
            VehicleProxy.RotateLeft((int)alpha);
            MoveForwardByCm(hyp);
            VehicleProxy.RotateLeft(90-(int)alpha);
        }

        public void StraightAheadAction()
        {
            MoveForwardByCm(CrossingWidth + ColorWidth);
        }

        public void TurnBackAction()
        {
            var c = CrossingWidth - VehicleWidth/2;
            var cPow = Math.Pow(c/2, 2);
            var hyp = Math.Sqrt(cPow + cPow);

            VehicleProxy.RotateLeft(45);
            MoveForwardByCm(hyp);
            VehicleProxy.RotateLeft(90);
            MoveForwardByCm(hyp);
            VehicleProxy.RotateLeft(45);
        }

        public void TurnMotorsOff()
        {
            VehicleProxy.TurnMotorsOff();
        }
    }
}