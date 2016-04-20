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
        private const SensorPort PORT_ULTRASONIC = SensorPort.In3;
        private const SensorPort PORT_COLOR = SensorPort.In4;
        private const MotorPort PORT_MOTOR_LEFT = MotorPort.OutD;
        private const MotorPort PORT_MOTOR_RIGHT = MotorPort.OutA;
 
        private const int BOUND_REDUCE_SPEED = 10;
        private const int BOUND_STOP_SPINNING = 2;
        private const int SPEED_MAX = 100;
        private const int SPEED_INTERMEDIATE = 50;
        private const int SPEED_LOW = 15;

        private EV3GyroSensor gyroSensor;
        private EV3UltrasonicSensor ultrasonicSensor;
        private Vehicle vehicle;
        private ColorReader colorReader;
        private ColorAnalyzer colorAnalyzer;

        public Roboter()
        {
            gyroSensor = new EV3GyroSensor(PORT_GYRO, GyroMode.Angle);
            gyroSensor.Reset();
            ultrasonicSensor = new EV3UltrasonicSensor(PORT_ULTRASONIC, UltraSonicMode.Centimeter);
            vehicle = new Vehicle(PORT_MOTOR_LEFT, PORT_MOTOR_RIGHT);
            colorReader = new ColorReader(PORT_COLOR);
            colorAnalyzer = new ColorAnalyzer();
            colorAnalyzer.ValidColors.AddRange(new List<KnownColor>() { KnownColor.Fence_temp, KnownColor.Blue });
        }

        private void HandleReadColor()
        {
            var lastRead = colorAnalyzer.Analyze(colorReader.LastRead);

            if (lastRead == KnownColor.Blue)
            {
                LcdConsole.WriteLine("Blue found!!");
                Rotate(90, RotationDirection.Left);
                MoveForwardByCm(20);
            }
            else if (lastRead == KnownColor.Invalid)
            {
                LcdConsole.WriteLine("got invalid color!!");
                vehicle.TurnRightForward(SPEED_LOW, 50, 10, true);
            }
        }

        public void FollowFence()
        {
            vehicle.Backward(SPEED_INTERMEDIATE);
            colorReader.TryRead();

            while (colorAnalyzer.Analyze(colorReader.LastRead) == KnownColor.Fence_temp)
            {
                colorReader.TryRead();                
            }
            vehicle.Brake();
            HandleReadColor();
        }

        private const int MAX_TRIES = 100;
        private double readDistanceInCm()
        {
            var val = ultrasonicSensor.Read();
            var tries = 0;
            while (val == 0 && tries < MAX_TRIES)
            {
                val = ultrasonicSensor.Read();
                tries++;
                Thread.Sleep(20);
            }
            return val / 10;
        }

        // in cm
        private const int IR_TO_FRONT_CENTER_DIFFERENCE_IN_CM = 3;
        private const int TURNING_BUFFER_IN_CM = 3;
        private const int ANGLE_TO_FENCE = 45;
        public void MoveToFence()
        {
            var distance = LegoUtils.DoubleToInt(readDistanceInCm());
            LegoUtils.PrintAndWait(2, "initial distance: {0}", distance);

            int distanceToFence = distance - IR_TO_FRONT_CENTER_DIFFERENCE_IN_CM - TURNING_BUFFER_IN_CM;
            LcdConsole.WriteLine("fence drice distance: {0}", distanceToFence);

            Rotate(90, RotationDirection.Right);
            MoveForwardByCm(distanceToFence, false);
            //Rotate(90 - ANGLE_TO_FENCE, RotationDirection.Left);
            //MoveForwardByCm(IR_TO_FRONT_CENTER_DIFFERENCE_IN_CM + TURNING_BUFFER_IN_CM);

            WaitHandle handle = vehicle.TurnLeftForward(SPEED_INTERMEDIATE, 50, LegoUtils.CmToEngineDegrees(IR_TO_FRONT_CENTER_DIFFERENCE_IN_CM + TURNING_BUFFER_IN_CM), true);
            handle.WaitOne();

            Rotate(ANGLE_TO_FENCE, RotationDirection.Left);
            LegoUtils.PrintAndWait(3, "finished moveToFence");
        }

        private void ForwardByDegrees(sbyte speed, uint degrees, bool brakeOnFinish = true)
        {
            LcdConsole.WriteLine("moving forward: speed {0} deg {1} brake {2}", speed, degrees, brakeOnFinish);
            WaitHandle handle = vehicle.Backward(speed, degrees, brakeOnFinish);
            handle.WaitOne();
        }

        private const int SLOW_THRESHOLD_IN_CM = 3;
        private const double FAST_MOMENTUM_FACTOR = .85d;
        private const double SLOW_MOMENTUM_FACTOR = .90d;
        private const int FAST_BRAKE_ANGLE = 15;
        private const int SLOW_BRAKE_ANGLE = 2;
        public void MoveForwardByCm(int cm, bool brakeOnFinish = true)
        {
            if (cm > SLOW_THRESHOLD_IN_CM)
            {
                uint cmCalcDeg = LegoUtils.CmToEngineDegrees(cm - SLOW_THRESHOLD_IN_CM);
                LcdConsole.WriteLine("cmcalcdeg {0}", cmCalcDeg);
                uint fastDegrees = (uint)Math.Round((cmCalcDeg * FAST_MOMENTUM_FACTOR) - FAST_BRAKE_ANGLE);
                uint slowDegrees = LegoUtils.CmToEngineDegrees(SLOW_THRESHOLD_IN_CM) - SLOW_BRAKE_ANGLE;

                LcdConsole.WriteLine("fastdeg: {0} slowdeg:{1}", fastDegrees, slowDegrees);

                ForwardByDegrees(SPEED_MAX, fastDegrees, false);
                ForwardByDegrees(SPEED_LOW, slowDegrees);
            }
            else
            {
                uint slowDegrees = (uint)Math.Round(LegoUtils.CmToEngineDegrees(cm) * SLOW_MOMENTUM_FACTOR) - SLOW_BRAKE_ANGLE;
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

        private int getAbsDelta(int number1, int number2)
        {
            return Math.Abs(number1 - number2);
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
