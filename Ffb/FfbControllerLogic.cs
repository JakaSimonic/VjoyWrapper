using System;
using System.Collections.Generic;

namespace Ffb
{
    internal class FfbControllerLogic
    {
        private EffectsLogic _effectsLogic;
        private IReportDescriptorProperties _reportDescriptorProperties;

        private PID_BLOCK_LOAD pidBlockLoad;
        private PID_STATE pidState;

        public Action DisableActuators;
        public Action EnableActuators;

        public FfbControllerLogic(IReportDescriptorProperties reportDescriptorProperties, ICalculationProvider calculationProvider)
        {
            _effectsLogic = new EffectsLogic(reportDescriptorProperties, calculationProvider);
            _reportDescriptorProperties = reportDescriptorProperties;

            pidState.status = 30;
            pidState.effectPlayingAndEffectBlockIndex = 0;
        }

        public void CreateNewEffect(object parameter)
        {

            int index = _effectsLogic.GetFirstFree();
            pidBlockLoad.effectBlockIndex = index;
            if (pidBlockLoad.effectBlockIndex == -1)
            {                
                pidBlockLoad.loadStatus = LOAD_STATUS.ERROR;
            }
            else
            {
                _effectsLogic.CreateNewEffect(index, null, parameter);
                pidBlockLoad.loadStatus = LOAD_STATUS.SUCCESS;
            }
            pidBlockLoad.ramPoolAvailable = _reportDescriptorProperties.MAX_RAM_POOL;
        }

        public void SetEnvelopeParameter(object parameter)
        {
            _effectsLogic.SetParameter(((ENVELOPE)parameter).effectBlockIndex, "ENVELOPE", parameter);
        }

        public void SetConditionParameter(object parameter)
        {
            _effectsLogic.SetParameter(((CONDITION)parameter).effectBlockIndex, "CONDITION", parameter);
        }

        public void SetPeriodParameter(object parameter)
        {
            _effectsLogic.SetParameter(((PERIOD)parameter).effectBlockIndex, "PERIOD", parameter);
        }

        public void SetRampParameter(object parameter)
        {
            _effectsLogic.SetParameter(((RAMP)parameter).effectBlockIndex, "RAMP", parameter);
        }

        public void SetEffectParameter(object parameter)
        {
            _effectsLogic.SetParameter(((SET_EFFECT)parameter).effectBlockIndex, "SET_EFFECT", parameter);
        }

        public void SetConstantParameter(object parameter)
        {
            _effectsLogic.SetParameter(((CONSTANT)parameter).effectBlockIndex, "CONSTANT", parameter);
        }

        public void EffectsOperation(object parameter)
        {
            OPERATION operation = (OPERATION)parameter;
            switch (operation.effectOp)
            {
                case EFFECT_OPERATION.START:
                    if (operation.loopCount > 0)
                    {
                        _effectsLogic.SetDuration(operation.effectBlockIndex, operation.loopCount);
                    }
                    _effectsLogic.StartEffect(operation.effectBlockIndex);
                    break;

                case EFFECT_OPERATION.SOLO:
                    _effectsLogic.StopAllEffects();
                    _effectsLogic.StartEffect(operation.effectBlockIndex);
                    break;

                case EFFECT_OPERATION.STOP:
                    _effectsLogic.StopEffect(operation.effectBlockIndex);
                    break;
            }
        }

        public void PIDBlockFree(object parameter)
        {
            int effectBlockIndex = ((PID_BLOCK_FREE)parameter).effectBlockIndex;
            if (effectBlockIndex == _reportDescriptorProperties.FREE_ALL_EFFECTS)
            {
                _effectsLogic.FreeAllEffects();
            }
            else
            {
                _effectsLogic.FreeEffect(effectBlockIndex);
            }
        }

        public void PIDDeviceControl(object parameter)
        {
            PID_CONTROL pidControl = ((PID_DEVICE_CONTROL)parameter).pidControl;
            switch (pidControl)
            {
                case PID_CONTROL.ENABLE_ACTUATORS:
                    EnableActuators?.Invoke();
                    pidState.status = 30;
                    break;

                case PID_CONTROL.DISABLE_ACTUATORS:
                    DisableActuators?.Invoke();
                    pidState.status = 28;
                    break;

                case PID_CONTROL.STOP_ALL_EFFECTS:
                    _effectsLogic.StopAllEffects();
                    break;

                case PID_CONTROL.RESET:
                    _effectsLogic.StartAllEffects();
                    break;

                case PID_CONTROL.PAUSE:
                    _effectsLogic.StopAllEffects();
                    break;

                case PID_CONTROL.CONTINUE:
                    _effectsLogic.ContinueAllEffects();
                    break;
            }
        }

        public void DeviceGain(object parameter)
        {
            _effectsLogic.SetDeviceGain(((DEVICE_GAIN)parameter).deviceGain / _reportDescriptorProperties.MAX_DEVICE_GAIN);
        }

        public object GetBlockLoad()
        {
            return pidBlockLoad;
        }

        public object GetPidState()
        {
            return pidState;
        }

        public List<double> GetForces(JOYSTICK_INPUT joystickInput)
        {
            return _effectsLogic.GetForce(joystickInput);
        }

        public void NotImplemented(object x)
        {
            x = null;
            throw new NotImplementedException();
        }
    }
}