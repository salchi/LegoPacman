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
        private const MotorPort PortMotorCollector = MotorPort.OutC;

        public SensorProxy SensorProxy { get; }
        public VehicleProxy VehicleProxy { get; }
        public bool IsCancelled { get; set; }
        private ColorAnalyzer colorAnalyzer;

        public Roboter()
        {
            SensorProxy = new SensorProxy(gyroPort: PortGyro, ultrasonicPort: PortUltrasonic, colorPort: PortColor);
            VehicleProxy = new VehicleProxy(SensorProxy, left: PortMotorLeft, right: PortMotorRight);
            colorAnalyzer = new ColorAnalyzer(new List<KnownColor>() { KnownColor.Blue, KnownColor.Red, KnownColor.White, KnownColor.Yellow, KnownColor.FenceLight, KnownColor.FenceDark/*KnownColor.Fence_temp*/, KnownColor.Invalid });
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
                LcdConsole.WriteLine("got invalid");
                VehicleProxy.RotateRight(10);
                MoveForwardByCm(5);
                /*for (int i = 0; i < 2; i++)
                {
                    VehicleProxy.RotateLeft(5);
                    var color = SensorProxy.ColorReader.ReadColor();
                    var kc = colorAnalyzer.Analyze(color);
                    if (KnownColor.IsActionColor(kc) || KnownColor.IsFenceColor(kc))
                    {
                        VehicleProxy.RotateLeft(10-5*i);
                        break;
                    }
                }*/
                VehicleProxy.RotateLeft(10);
            }
        }

        private const int GrabSpeed = 10;
        private const int GrabTime = 1000;
        public void GrabPrincess()
        {
            var m = new Motor(PortMotorPrincess);
            m.SetSpeed(GrabSpeed);
            Thread.Sleep(GrabTime);
            m.Off();
        }

        private const int CollectSpeed = 10;
        private const int CollectTime = 1500;
        public void CloseCollectorArm()
        {
            var m = new Motor(PortMotorCollector);
            m.SetSpeed(CollectSpeed);
            Thread.Sleep(CollectTime);
            m.Off();
        }

        public void OpenCollectorArm()
        {
            var m = new Motor(PortMotorCollector);
            m.SetSpeed(-CollectSpeed);
            Thread.Sleep(CollectTime);
            m.Off();
        }

        private bool IsTimeElapsed(TimeSpan deltaTime)
        {
            return deltaTime.TotalSeconds > 270;
        }

        public void FollowFence(bool chainOperations = false)
        {
            int operationCounter = 0;
            DateTime startingTime = DateTime.Now;

            do
            {
                VehicleProxy.MoveForwardUntil(
                    SensorProxy.ColorReader.ReadColor,
                    color =>
                    {
                        var kc = colorAnalyzer.Analyze(color);
                        return KnownColor.IsActionColor(kc) || kc.Equals(KnownColor.Invalid) || IsCancelled || IsTimeElapsed(DateTime.Now - startingTime);
                    }
                );

                HandleReadColor();
                operationCounter++;

                if (operationCounter == 5)
                {
                    GrabPrincess();
                }
            } while (!(IsCancelled || chainOperations || IsTimeElapsed(DateTime.Now - startingTime)));

            VehicleProxy.TurnMotorsOff();
            CloseCollectorArm();
        }

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
            var degrees = (cm >= 10) ? LegoMath.CmToEngineDegrees(cm) * 8 / 7 : LegoMath.CmToEngineDegrees(cm);
            MoveByDegrees(Velocity.Low, degrees, direction, brakeOnFinish);
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
            else if (degrees < 0)
            {
                if (direction == Direction.Forward)
                {
                    VehicleProxy.BackwardByDegrees(speed, degrees, brakeOnFinish);
                }
                else
                {
                    VehicleProxy.ForwardByDegrees(speed, degrees, brakeOnFinish);
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
            var alpha = Math.Asin(FenceLength / hyp) / (2 * Math.PI) * 360 - 8;
            LcdConsole.WriteLine("hyp {0}", hyp);
            LcdConsole.WriteLine("alpha {0}", alpha);
            VehicleProxy.RotateLeft((int)alpha);
            MoveForwardByCm(hyp);
            VehicleProxy.RotateLeft((90 - (int)alpha)+3);
        }

        public void StraightAheadAction()
        {
            MoveForwardByCm(CrossingWidth + ColorWidth * 1.5);
        }

        public void TurnBackAction()
        {
            /*
            VehicleProxy.RotateLeft(45);
            MoveForwardByCm(8.5);
            VehicleProxy.RotateLeft(90);
            MoveForwardByCm(8.5);
            VehicleProxy.RotateLeft(47);*/

            VehicleProxy.RotateLeft(24);
            MoveForwardByCm(7.2);

            VehicleProxy.RotateLeft(85);
            MoveForwardByCm(6);

            VehicleProxy.RotateLeft(35);
            MoveForwardByCm(5.2);

            VehicleProxy.RotateLeft(38);

            MoveForwardByCm((VehicleLength/2)+ColorSensorToCenter);
        }

        public void TurnMotorsOff()
        {
            VehicleProxy.TurnMotorsOff();
        }
    }
}