﻿using System;
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
            var lastRead = colorAnalyzer.Analyze(SensorProxy.ColorReader.LastRead);

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
            SensorProxy.ColorReader.TryRead();

            VehicleProxy.MoveForwardWhile(() =>
            {
                SensorProxy.ColorReader.TryRead();
                return SensorProxy.ColorReader.LastRead;
            }, color => colorAnalyzer.Analyze(color) == KnownColor.Fence_temp);

            //HandleReadColor();
        }

        // in cm
        public void MoveToFence()
        {
            const int IrSensorFrontCenterDifference = 3;
            const int TurningBuffer = 9;
            const int CurveDistance = IrSensorFrontCenterDifference + TurningBuffer + 9;

            var distance = LegoMath.DoubleToInt(SensorProxy.ReadDistanceInCm());
            //LegoUtils.PrintAndWait(2, "initial distance: {0}", distance);

            int distanceToFence = distance - IrSensorFrontCenterDifference - TurningBuffer;
            //LcdConsole.WriteLine("fence drive distance: {0}", distanceToFence);

            VehicleProxy.RotateRight(90);
            MoveForwardByCm(distanceToFence, false);

            VehicleProxy.TurnLeftForward(Velocity.Medium, 100, LegoMath.CmToEngineDegrees(CurveDistance));

            MoveForwardByCm(1);

            VehicleProxy.RotateLeft(SensorProxy.ReadGyro(RotationDirection.Left));
            LegoUtils.PrintAndWait(3, "finished moveToFence");
        }

        // in cm
        public void MoveForwardByCm(int cm, bool brakeOnFinish = true)
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

                VehicleProxy.ForwardByDegrees(Velocity.Highest, fastDegrees, false);
                VehicleProxy.ForwardByDegrees(Velocity.Lowest, slowDegrees);
            }
            else
            {
                uint slowDegrees = (uint)Math.Round(LegoMath.CmToEngineDegrees(cm) * SlowMomentumFactor) - SlowBrakeDistance;
                LcdConsole.WriteLine("slow deg: {0}", slowDegrees);
                VehicleProxy.ForwardByDegrees(Velocity.Lowest, slowDegrees);
            }
        }

        public void TurnMotorsOff()
        {
            VehicleProxy.TurnMotorsOff();
        }
    }
}