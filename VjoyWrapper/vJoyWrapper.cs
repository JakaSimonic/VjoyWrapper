using Ffb;
using System;
using System.Collections.Generic;
using vJoyInterfaceWrap;

namespace VjoyWrapper
{
    public class vJoyFunctions
    {
        private vJoy _joystick;
        private uint _id = 1;
        private vJoyFfbHandler _ffbHandler;
        private ILogger _logger;
        public vJoy.JoystickState zeroJoystickState { get; private set; }

        public vJoyFunctions(vJoy jostick)
        {
            _logger = new Logger();

            _joystick = jostick;
            IReportDescriptorProperties reportDescriptorProperties = new vJoyReportDescriptorProperties();
            _ffbHandler = new vJoyFfbHandler(_joystick, reportDescriptorProperties, _logger);
            _joystick.FfbRegisterGenCB(OnFfbArrival, _id);

            zeroJoystickState = GetZeroJoystickInput();
        }

        public vJoyFunctions(vJoy jostick, int id, ILogger customLogger)
        {
            _id = (uint)id;
            _logger = customLogger != null ? customLogger : new Logger();

            _joystick = jostick;
            IReportDescriptorProperties reportDescriptorProperties = new vJoyReportDescriptorProperties();
            _ffbHandler = new vJoyFfbHandler(_joystick, reportDescriptorProperties, _logger);
            _joystick.FfbRegisterGenCB(OnFfbArrival, _id);

            zeroJoystickState = GetZeroJoystickInput();
        }

        public bool Init()
        {
            return _joystick.AcquireVJD(_id);
        }

        public void TestVjoy()
        {
            // Create one joystick object and a position structure.

            // Device ID can only be in the range 1-16
            if (_id <= 0 || _id > 16)
            {
                _logger.WriteLog($"Illegal device ID {_id}\nExit!");
                return;
            }

            // Get the driver attributes (Vendor ID, Product ID, Version Number)
            if (!_joystick.vJoyEnabled())
            {
                _logger.WriteLog($"vJoy driver not enabled: Failed Getting vJoy attributes.\n");
                return;
            }
            else
                _logger.WriteLog($"Vendor: {_joystick.GetvJoyManufacturerString()}\nProduct :{_joystick.GetvJoyProductString()}\nVersion Number:{_joystick.GetvJoySerialNumberString()}\n");

            // Get the state of the requested device
            VjdStat status = _joystick.GetVJDStatus(_id);
            switch (status)
            {
                case VjdStat.VJD_STAT_OWN:
                    _logger.WriteLog($"vJoy Device {_id} is already owned by this feeder");
                    break;

                case VjdStat.VJD_STAT_FREE:
                    _logger.WriteLog($"vJoy Device {_id} is free");
                    break;

                case VjdStat.VJD_STAT_BUSY:
                    _logger.WriteLog($"vJoy Device {_id} is already owned by another feeder\nCannot continue");
                    return;

                case VjdStat.VJD_STAT_MISS:
                    _logger.WriteLog($"vJoy Device {_id} is not installed or disabled\nCannot continue");
                    return;

                default:
                    _logger.WriteLog($"vJoy Device {_id} general error\nCannot continue");
                    return;
            };

            // Check which axes are supported
            bool AxisX = _joystick.GetVJDAxisExist(_id, HID_USAGES.HID_USAGE_X);
            bool AxisY = _joystick.GetVJDAxisExist(_id, HID_USAGES.HID_USAGE_Y);
            bool AxisZ = _joystick.GetVJDAxisExist(_id, HID_USAGES.HID_USAGE_Z);
            bool AxisRX = _joystick.GetVJDAxisExist(_id, HID_USAGES.HID_USAGE_RX);
            bool AxisRZ = _joystick.GetVJDAxisExist(_id, HID_USAGES.HID_USAGE_RZ);
            bool AxisRY = _joystick.GetVJDAxisExist(_id, HID_USAGES.HID_USAGE_RY);
            // Get the number of buttons and POV Hat switchessupported by this vJoy device
            int nButtons = _joystick.GetVJDButtonNumber(_id);
            int ContPovNumber = _joystick.GetVJDContPovNumber(_id);
            int DiscPovNumber = _joystick.GetVJDDiscPovNumber(_id);

            // Print results
            _logger.WriteLog($"\nvJoy Device {_id} capabilities:");
            _logger.WriteLog($"Numner of buttons\t\t{nButtons}");
            _logger.WriteLog($"Numner of Continuous POVs\t{ContPovNumber}");
            _logger.WriteLog($"Numner of Descrete POVs\t\t{DiscPovNumber}");
            _logger.WriteLog($"Axis X\t\t{AxisX.ToString()}");
            _logger.WriteLog($"Axis Y\t\t{AxisY.ToString()}");
            _logger.WriteLog($"Axis Z\t\t{AxisZ.ToString()}");
            _logger.WriteLog($"Axis Rx\t\t{AxisRX.ToString()}");
            _logger.WriteLog($"Axis Ry\t\t{AxisRY.ToString()}");
            _logger.WriteLog($"Axis Rz\t\t{AxisRZ.ToString()}");

            // Test if DLL matches the driver
            UInt32 DllVer = 0, DrvVer = 0;
            bool match = _joystick.DriverMatch(ref DllVer, ref DrvVer);
            if (match)
                _logger.WriteLog($"Version of Driver Matches DLL Version ({DllVer:X})");
            else
                _logger.WriteLog($"Version of Driver ({DrvVer:X}) does NOT match DLL Version ({DllVer:X})");

            // Acquire the target
            if ((status == VjdStat.VJD_STAT_OWN) || ((status == VjdStat.VJD_STAT_FREE) && (!_joystick.AcquireVJD(_id))))
            {
                _logger.WriteLog($"Failed to acquire vJoy device number {_id}");
                return;
            }
            else
                _logger.WriteLog($"Acquired: vJoy device number {_id}");

            _logger.WriteLog($"\npress enter to stat feeding");
        }

        private void OnFfbArrival(IntPtr packet, object userData)
        {
            _ffbHandler.HandleFfbSetPacket(packet);
        }

        public List<double> GetFfbForce(List<double> input)
        {
            JOYSTICK_INPUT joystickInput = new JOYSTICK_INPUT();
            joystickInput.axesPositions = input;
            return _ffbHandler.GetFfbForce(joystickInput);
        }

        public void SetJoystickInput(vJoy.JoystickState _iReport)
        {
            if (!_joystick.UpdateVJD(_id, ref _iReport))
            {
                _logger.WriteLog($"Feeding vJoy device number {_id} failed - try to enable device then press enter");
                Console.ReadKey(true);
                _joystick.AcquireVJD(_id);
            }
        }

        public void Clear()
        {
            _joystick.RelinquishVJD(_id);
        }


        private vJoy.JoystickState GetZeroJoystickInput()
        {
            vJoy.JoystickState jS = new vJoy.JoystickState();
            jS.Aileron = 0;
            jS.AxisVBRX = 0;
            jS.AxisVBRY = 0;
            jS.AxisVBRZ = 0;
            jS.AxisVX = 0;
            jS.AxisVY = 0;
            jS.AxisVZ = 0;
            jS.AxisX = 0;
            jS.AxisXRot = 0;
            jS.AxisY = 0;
            jS.AxisYRot = 0;
            jS.AxisZ = 0;
            jS.AxisZRot = 0;
            jS.bDevice = 0;
            jS.bHats = 0;
            jS.bHatsEx1 = 0;
            jS.bHatsEx2 = 0;
            jS.bHatsEx3 = 0;
            jS.Buttons = 0;
            jS.ButtonsEx1 = 0;
            jS.ButtonsEx2 = 0;
            jS.ButtonsEx3 = 0;
            jS.Dial = 0;
            jS.Rudder = 0;
            jS.Slider = 0;
            jS.Throttle = 0;
            jS.Wheel = 0;

            return jS;
        }
    }
}