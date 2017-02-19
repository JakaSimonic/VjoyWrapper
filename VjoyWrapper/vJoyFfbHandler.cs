using Ffb;
using System;
using System.Collections.Generic;
using System.Linq;
using vJoyInterfaceWrap;

namespace VjoyWrapper
{
    internal class vJoyFfbHandler
    {
        private vJoy _joystick;
        private FfbController _ffbController;

        private Dictionary<FFBOP, EFFECT_OPERATION> EffectOperationMapper;
        private Dictionary<FFBEType, EFFECT_TYPE> EffectTypeMapper;
        private Dictionary<FFBPType, Tuple<string, Func<object, object>>> FfbReportMapper;
        private Dictionary<FFB_CTRL, PID_CONTROL> PidControlMapper;
        private ILogger _logger;

        public vJoyFfbHandler(vJoy joystick, IReportDescriptorProperties reportDescriptorProperties, ILogger logger)
        {
            _logger = logger;
            _joystick = joystick;
            FfbControllerFactory factory = new FfbControllerFactory(reportDescriptorProperties);

            _ffbController = factory.Create();

            EffectOperationMapper = new Dictionary<FFBOP, EFFECT_OPERATION>() {
              {FFBOP.EFF_START, EFFECT_OPERATION.START},
              {FFBOP.EFF_SOLO , EFFECT_OPERATION.SOLO},
              {FFBOP.EFF_STOP , EFFECT_OPERATION.STOP}
            };

            EffectTypeMapper = new Dictionary<FFBEType, EFFECT_TYPE>() {
                {FFBEType.ET_CONST,EFFECT_TYPE.CONSTANT},
                {FFBEType.ET_RAMP,EFFECT_TYPE.RAMP},
                {FFBEType.ET_SQR,EFFECT_TYPE.SQUARE},
                {FFBEType.ET_SINE,EFFECT_TYPE.SINE},
                {FFBEType.ET_TRNGL,EFFECT_TYPE.TRIANGLE},
                {FFBEType.ET_STUP,EFFECT_TYPE.SAWTOOTH_UP},
                {FFBEType.ET_STDN,EFFECT_TYPE.SAWTOOTH_DOWN},
                {FFBEType.ET_SPRNG,EFFECT_TYPE.SPRING},
                {FFBEType.ET_DMPR,EFFECT_TYPE.DAMPER},
                {FFBEType.ET_INRT,EFFECT_TYPE.INERTIA},
                {FFBEType.ET_FRCTN,EFFECT_TYPE.FRICTION},
                {FFBEType.ET_CSTM,EFFECT_TYPE.CUSTOM}
                };

            FfbReportMapper = new Dictionary<FFBPType, Tuple<string, Func<object, object>>>() {
                {FFBPType.PT_EFFREP,new Tuple<string, Func<object, object>>("SetEffect",SetEffectMapper)},
                { FFBPType.PT_ENVREP,new Tuple<string, Func<object, object>>("SetEnvelope",EnvelopeMapper)},
                { FFBPType.PT_CONDREP,new Tuple<string, Func<object, object>>("SetCondition",ConditionMapper)},
                { FFBPType.PT_PRIDREP,new Tuple<string, Func<object, object>>("SetPeriodic",PeriodMapper)},
                { FFBPType.PT_CONSTREP,new Tuple<string, Func<object, object>>("SetConstantForce",ConstantMapper)},
                { FFBPType.PT_RAMPREP,new Tuple<string, Func<object, object>>("SetRampForce",RampMapper)},
                { FFBPType.PT_EFOPREP,new Tuple<string, Func<object, object>>("EffectOperation",OperationMapper)},
                { FFBPType.PT_GAINREP,new Tuple<string, Func<object, object>>("DeviceGain",DeviceGainMapper)},
                { FFBPType.PT_CTRLREP,new Tuple<string, Func<object, object>>("PIDDeviceControl",PidDeviceControlMapper)},
                { FFBPType.PT_BLKFRREP,new Tuple<string, Func<object, object>>("PIDBlockFree",BlockFreeMapper)},
                { FFBPType.PT_NEWEFREP,new Tuple<string, Func<object, object>>("CreateNewEffect",CreateNewEffectMapper)},
                { FFBPType.PT_BLKLDREP,new Tuple<string, Func<object, object>>("BlockLoad",PidBlockLoadMapper)},
                { FFBPType.PT_POOLREP,new Tuple<string, Func<object, object>>("PIDState",PidStateMapper)}
                //{ FFBPType.PT_CSTMREP,new Tuple<string, Func<object, object>>("CustomForceData",gggg)},
                //{ FFBPType.PT_SETCREP,new Tuple<string, Func<object, object>>("SetCustomForce",gggg)}
        };

            PidControlMapper = new Dictionary<FFB_CTRL, PID_CONTROL>() {
            {FFB_CTRL.CTRL_ENACT ,PID_CONTROL.ENABLE_ACTUATORS},
            {FFB_CTRL.CTRL_DISACT,PID_CONTROL.DISABLE_ACTUATORS },
            {FFB_CTRL.CTRL_STOPALL,PID_CONTROL.STOP_ALL_EFFECTS },
            {FFB_CTRL.CTRL_DEVRST,PID_CONTROL.RESET },
            {FFB_CTRL.CTRL_DEVPAUSE,PID_CONTROL.PAUSE },
            {FFB_CTRL.CTRL_DEVCONT,PID_CONTROL.CONTINUE },
        };
        }

        public List<double> GetFfbForce(Ffb.JOYSTICK_INPUT joystickInput)
        {
            return _ffbController.GetForces(joystickInput);
        }

        public void HandleFfbSetPacket(object packet)
        {
            IntPtr _packet = (IntPtr)packet;
            FFBPType type = new FFBPType();
            uint status = 0;
            int dataSize = 0;
            byte[] data = new byte[256];
            uint ioctrl = 0;

            status = _joystick.Ffb_h_Type(_packet, ref type);
            status = _joystick.Ffb_h_Packet(_packet, ref ioctrl, ref dataSize, ref data);

            byte[] ffbData = new byte[dataSize];
            Array.Copy(data, 0, ffbData, 0, dataSize);

            Tuple<string, Func<object, object>> reportHandler = FfbReportMapper[type];

            string reportName = reportHandler.Item1;

            Console.WriteLine(reportName);
            Console.WriteLine(string.Join(" ", ffbData.Select(b => b.ToString())));

            Func<object, object> mapHandler = reportHandler.Item2;

            if (type > FFBPType.PT_NEWEFREP)
            {
                try
                {
                    object getObject = _ffbController.Get[reportName]();
                    byte[] packetData = (byte[])mapHandler(getObject);
                }
                catch (Exception e)
                {
                    _logger.WriteLog(e.ToString());
                }
            }
            else
            {
                try
                {
                    object mappedReport = mapHandler(packet);
                    _ffbController.SetReport[reportName](mappedReport);
                }
                catch (Exception e)
                {
                    _logger.WriteLog(e.ToString());
                }
            }
        }

        private object ConditionMapper(object packet)
        {
            IntPtr _packet = (IntPtr)packet;
            vJoy.FFB_EFF_COND ffbData = new vJoy.FFB_EFF_COND();
            _joystick.Ffb_h_Eff_Cond(_packet, ref ffbData);
            CONDITION o;
            o.effectBlockIndex = ffbData.EffectBlockIndex;
            o.deadBand = ffbData.DeadBand;
            o.cpOffset = ffbData.CenterPointOffset;
            o.negativeSaturation = ffbData.NegSatur;
            o.positiveSaturation = ffbData.PosSatur;
            o.negativeCoefficient = ffbData.NegCoeff;
            o.positiveCoefficient = ffbData.PosCoeff;
            return o;
        }

        private object ConstantMapper(object packet)
        {
            IntPtr _packet = (IntPtr)packet;
            vJoy.FFB_EFF_CONSTANT ffbData = new vJoy.FFB_EFF_CONSTANT();
            _joystick.Ffb_h_Eff_Constant(_packet, ref ffbData);
            CONSTANT o;
            o.effectBlockIndex = ffbData.EffectBlockIndex;
            o.magnitude = ffbData.Magnitude;
            return o;
        }

        private object EnvelopeMapper(object packet)
        {
            IntPtr _packet = (IntPtr)packet;
            vJoy.FFB_EFF_ENVLP ffbData = new vJoy.FFB_EFF_ENVLP();
            _joystick.Ffb_h_Eff_Envlp(_packet, ref ffbData);
            ENVELOPE o;
            o.effectBlockIndex = ffbData.EffectBlockIndex;
            o.attackLevel = ffbData.AttackLevel;
            o.attackTime = ffbData.AttackTime;
            o.fadeLevel = ffbData.FadeLevel;
            o.fadeTime = ffbData.FadeTime;
            return o;
        }

        private object OperationMapper(object packet)
        {
            IntPtr _packet = (IntPtr)packet;
            vJoy.FFB_EFF_OP ffbData = new vJoy.FFB_EFF_OP();
            _joystick.Ffb_h_EffOp(_packet, ref ffbData);
            OPERATION o;
            o.effectBlockIndex = ffbData.EffectBlockIndex;
            o.effectOp = EffectOperationMapper[ffbData.EffectOp];
            o.loopCount = ffbData.LoopCount;
            return o;
        }

        private object PeriodMapper(object packet)
        {
            IntPtr _packet = (IntPtr)packet;
            vJoy.FFB_EFF_PERIOD ffbData = new vJoy.FFB_EFF_PERIOD();
            _joystick.Ffb_h_Eff_Period(_packet, ref ffbData);
            PERIOD o;
            o.effectBlockIndex = ffbData.EffectBlockIndex;
            o.magnitude = ffbData.Magnitude;
            o.offset = ffbData.Offset;
            o.period = ffbData.Period;
            o.phase = ffbData.Phase;
            return o;
        }

        private object RampMapper(object packet)
        {
            IntPtr _packet = (IntPtr)packet;
            vJoy.FFB_EFF_RAMP ffbData = new vJoy.FFB_EFF_RAMP();
            _joystick.Ffb_h_Eff_Ramp(_packet, ref ffbData);
            RAMP o;
            o.effectBlockIndex = ffbData.EffectBlockIndex;
            o.end = ffbData.End;
            o.start = ffbData.Start;
            return o;
        }

        private object SetEffectMapper(object packet)
        {
            IntPtr _packet = (IntPtr)packet;
            vJoy.FFB_EFF_REPORT ffbData = new vJoy.FFB_EFF_REPORT();
            _joystick.Ffb_h_Eff_Report(_packet, ref ffbData);
            SET_EFFECT o;
            o.effectBlockIndex = ffbData.EffectBlockIndex;
            o.duration = ffbData.Duration;
            o.gain = ffbData.Gain;
            o.samplePeriod = ffbData.SamplePrd;
            o.trigerButton = ffbData.TrigerBtn;
            o.trigerRepeat = ffbData.TrigerRpt;
            o.directionX = ffbData.DirX;
            o.directionY = ffbData.DirY;
            o.direction = ffbData.Direction;
            o.polar = ffbData.Polar;
            o.effetType = EffectTypeMapper[ffbData.EffectType];
            o.startDelay = 0;
            return o;
        }

        private object PidDeviceControlMapper(object packet)
        {
            IntPtr _packet = (IntPtr)packet;
            FFB_CTRL ffbData = new FFB_CTRL();
            _joystick.Ffb_h_DevCtrl(_packet, ref ffbData);
            PID_DEVICE_CONTROL o;
            o.pidControl = PidControlMapper[ffbData];
            return o;
        }

        private object DeviceGainMapper(object packet)
        {
            IntPtr _packet = (IntPtr)packet;
            byte ffbData = 0;
            _joystick.Ffb_h_DevGain(_packet, ref ffbData);
            DEVICE_GAIN o;
            o.deviceGain = ffbData;
            return o;
        }

        private object BlockFreeMapper(object packet)
        {
            IntPtr _packet = (IntPtr)packet;
            int dataSize = 0;
            uint ioctrl = 0;

            byte[] ffbData = new byte[8];
            _joystick.Ffb_h_Packet(_packet, ref ioctrl, ref dataSize, ref ffbData);
            PID_BLOCK_FREE o;
            o.effectBlockIndex = ffbData[1];
            return o;
        }

        private object CreateNewEffectMapper(object packet)
        {
            IntPtr _packet = (IntPtr)packet;
            int dataSize = 0;
            uint ioctrl = 0;

            byte[] ffbData = new byte[8];
            _joystick.Ffb_h_Packet(_packet, ref ioctrl, ref dataSize, ref ffbData);

            CREATE_NEW_EFFECT o;
            o.effetType = EffectTypeMapper[(FFBEType)ffbData[1]];
            return o;
        }

        private object PidBlockLoadMapper(object ffbData)
        {
            PID_BLOCK_LOAD _ffbData = (PID_BLOCK_LOAD)ffbData;
            byte[] o = new byte[4];
            o[0] = (byte)_ffbData.effectBlockIndex;   // 1..40
            o[1] = (byte)_ffbData.loadStatus; // 1=Success,2=Full,3=Error
            o[2] = 0xFF; // =0 or 0xFFFF?
            o[3] = 0xFF; // =0 or 0xFFFF?
            return o;
        }

        private object PidStateMapper(object ffbData)
        {
            PID_STATE _ffbData = (PID_STATE)ffbData;
            byte[] o = new byte[4];
            o[0] = 2;   // 1..40
            o[1] = (byte)_ffbData.status; // 1=Success,2=Full,3=Error
            o[2] = 0; // =0 or 0xFFFF?
            return o;
        }
    }
}