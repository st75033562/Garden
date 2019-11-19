using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Robomation;

namespace RobotSimulation
{
    public class Robot : MonoBehaviour, IRobot
    {
        public const float PhysicalLength = 3.9f / 100.0f;

        public SteeringSystem steeringSystem;

        public FloorSensor leftFloorSensor;
        public FloorSensor rightFloorSensor;

        public ProximitySensor leftProximitySensor;
        public ProximitySensor rightProximitySensor;

        public LightSensor lightSensor;

        public Led leftLed;
        public Led rightLed;

        public Buzzer buzzer;

        void Awake()
        {
            robotIndex = -1;
        }

        void Start()
        {
            var collider = GetComponent<BoxCollider>();
            worldToPhysicalRatio = collider.size.z / PhysicalLength;

            leftProximitySensor.worldToPhysicalRatio = worldToPhysicalRatio;
            rightProximitySensor.worldToPhysicalRatio = worldToPhysicalRatio;
        }
        
        public IFloor floor
        {
            set
            {
                leftFloorSensor.floor = value;
                rightFloorSensor.floor = value;
            }
        }

        public IProximityModel proximityModel
        {
            set
            {
                leftProximitySensor.proximityCurve = value;
                rightProximitySensor.proximityCurve = value;
            }
        }

        public NormalNoise floorSensorNoise
        {
            set
            {
                leftFloorSensor.noise = value;
                rightFloorSensor.noise = value;
            }
        }

        public NormalNoise proximitySensorNoise
        {
            set
            {
                leftProximitySensor.noise = value;
                rightProximitySensor.noise = value;
            }
        }

        public float wheelBalanceValue
        {
            set { steeringSystem.balanceValue = value; }
        }

        public int robotIndex
        {
            get;
            internal set;
        }

        /// <summary>
        /// world size to physical size ratio
        /// <para>sizes are in meters</para>
        /// </summary>
        public float worldToPhysicalRatio
        {
            get;
            private set;
        }

        #region IRobot

        public string getName()
        {
            // TODO: temporary
            return "Hamster";
        }

        public RobotType type
        {
            get { return RobotType.Hamster; }
        }

        public int read(int deviceId)
        {
            return read(deviceId, 0);
        }

        public int read(int deviceId, int index)
        {
            return (int)readFloat(deviceId, index);
        }

        public float readFloat(int deviceId)
        {
            return readFloat(deviceId, 0);
        }

        public float readFloat(int deviceId, int index)
        {
            switch (deviceId)
            {
            case Hamster.LEFT_WHEEL:
                return steeringSystem.GetWheelValue(0);

            case Hamster.RIGHT_WHEEL:
                return steeringSystem.GetWheelValue(1);

            case Hamster.LEFT_FLOOR:
                return leftFloorSensor.value;

            case Hamster.RIGHT_FLOOR:
                return rightFloorSensor.value;

            case Hamster.LEFT_PROXIMITY:
                return leftProximitySensor.value;

            case Hamster.RIGHT_PROXIMITY:
                return rightProximitySensor.value;

            case Hamster.BUZZER:
                return buzzer.frequency;

            case Hamster.NOTE:
                return buzzer.note;

            case Hamster.LEFT_LED:
                return (float)leftLed.color;

            case Hamster.RIGHT_LED:
                return (float)rightLed.color;

            case Hamster.LEFT_WHEEL_SPEED:
                return steeringSystem.GetWheelGroundSpeed(0);

            case Hamster.RIGHT_WHEEL_SPEED:
                return steeringSystem.GetWheelGroundSpeed(1);

            case Hamster.LIGHT:
                return lightSensor.value;

            default:
                return 0.0f;
            }
        }

        public bool write(int deviceId, int data)
        {
            return write(deviceId, 0, data);
        }

        public bool write(int deviceId, int index, int data)
        {
            return writeFloat(deviceId, index, (float)data);
        }

        public bool writeFloat(int deviceId, float data)
        {
            return writeFloat(deviceId, 0, data);
        }

        public bool writeFloat(int deviceId, int index, float data)
        {
            switch (deviceId)
            {
            case Hamster.LEFT_WHEEL:
                steeringSystem.SetWheelValue(0, data);
                return true;

            case Hamster.RIGHT_WHEEL:
                steeringSystem.SetWheelValue(1, data);
                return true;

            case Hamster.BUZZER:
                buzzer.frequency = data;
                return true;

            case Hamster.NOTE:
                buzzer.note = (int)data;
                return true;

            case Hamster.LEFT_LED:
                leftLed.color = (LedColor)data;
                return true;

            case Hamster.RIGHT_LED:
                rightLed.color = (LedColor)data;
                return true;

            default:
                return false;
            }
        }

        public void resetDevices()
        {
            steeringSystem.SetWheelValue(0, 0);
            steeringSystem.SetWheelValue(1, 0);
            buzzer.frequency = 0;
            buzzer.note = 0;
            leftLed.color = LedColor.Off;
            rightLed.color = LedColor.Off;
        }

        #endregion IRobot
    }
}
