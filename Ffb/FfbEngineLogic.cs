using System;
using System.Collections.Generic;
using TinyIoC;
namespace Ffb
{
    internal class FfbEngineLogic
    {
        private EffectsContainer _effectsContainer;
        private IReportDescriptorProperties _reportDescriptorProperties;

        private PID_BLOCK_LOAD pidBlockLoad;
        private PID_STATE pidState;

        public Action DisableActuators;
        public Action EnableActuators;

        public FfbEngineLogic(IReportDescriptorProperties reportDescriptorProperties)
        {
            _effectsContainer = new EffectsContainer(reportDescriptorProperties);
            _reportDescriptorProperties = reportDescriptorProperties;

            pidState.status = 30;
            pidState.effectPlayingAndEffectBlockIndex = 0;
        }

        public void CreateNewEffect(object parameter)
        {

            int index = _effectsContainer.GetFirstFree();
            pidBlockLoad.effectBlockIndex = index;
            if (pidBlockLoad.effectBlockIndex == -1)
            {                
                pidBlockLoad.loadStatus = LOAD_STATUS.ERROR;
            }
            else
            {
                _effectsContainer.CreateNewEffect(index, null, parameter);
                pidBlockLoad.loadStatus = LOAD_STATUS.SUCCESS;
            }
            pidBlockLoad.ramPoolAvailable = _reportDescriptorProperties.MAX_RAM_POOL;
        }

        public void SetEnvelopeParameter(object parameter)
        {
            _effectsContainer.SetParameter(((ENVELOPE)parameter).effectBlockIndex, "ENVELOPE", parameter);
        }

        internal void SetCustomForce(object parameter)
        {
            _effectsContainer.SetParameter(((CUSTOM_FORCE_PARAMETER)parameter).effectBlockIndex, "CUSTOM_FORCE_PARAMETER", parameter);
        }

        internal void CustomForceData(object parameter)
        {
            _effectsContainer.SetParameter(((CUSTOM_FORCE_DATA_REPORT)parameter).effectBlockIndex, "CUSTOM_FORCE_DATA_REPORT", parameter);
        }

        public void SetConditionParameter(object parameter)
        {
            _effectsContainer.SetParameter(((CONDITION)parameter).effectBlockIndex, "CONDITION", parameter);
        }

        public void SetPeriodParameter(object parameter)
        {
            _effectsContainer.SetParameter(((PERIOD)parameter).effectBlockIndex, "PERIOD", parameter);
        }

        public void SetRampParameter(object parameter)
        {
            _effectsContainer.SetParameter(((RAMP)parameter).effectBlockIndex, "RAMP", parameter);
        }

        public void SetEffectParameter(object parameter)
        {
            _effectsContainer.SetParameter(((SET_EFFECT)parameter).effectBlockIndex, "SET_EFFECT", parameter);
        }

        public void SetConstantParameter(object parameter)
        {
            _effectsContainer.SetParameter(((CONSTANT)parameter).effectBlockIndex, "CONSTANT", parameter);
        }

        public void EffectsOperation(object parameter)
        {
            OPERATION operation = (OPERATION)parameter;
            switch (operation.effectOp)
            {
                case EFFECT_OPERATION.START:
                    if (operation.loopCount > 0)
                    {
                        _effectsContainer.SetDuration(operation.effectBlockIndex, operation.loopCount);
                    }
                    _effectsContainer.StartEffect(operation.effectBlockIndex);
                    break;

                case EFFECT_OPERATION.SOLO:
                    _effectsContainer.StopAllEffects();
                    _effectsContainer.StartEffect(operation.effectBlockIndex);
                    break;

                case EFFECT_OPERATION.STOP:
                    _effectsContainer.StopEffect(operation.effectBlockIndex);
                    break;
            }
        }

        public void PIDBlockFree(object parameter)
        {
            int effectBlockIndex = ((PID_BLOCK_FREE)parameter).effectBlockIndex;
            if (effectBlockIndex == _reportDescriptorProperties.FREE_ALL_EFFECTS)
            {
                _effectsContainer.FreeAllEffects();
            }
            else
            {
                _effectsContainer.FreeEffect(effectBlockIndex);
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
                    _effectsContainer.StopAllEffects();
                    break;

                case PID_CONTROL.RESET:
                    _effectsContainer.StartAllEffects();
                    break;

                case PID_CONTROL.PAUSE:
                    _effectsContainer.StopAllEffects();
                    break;

                case PID_CONTROL.CONTINUE:
                    _effectsContainer.ContinueAllEffects();
                    break;
            }
        }

        public void DeviceGain(object parameter)
        {
            _effectsContainer.SetDeviceGain(((DEVICE_GAIN)parameter).deviceGain / _reportDescriptorProperties.MAX_DEVICE_GAIN);
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
            return _effectsContainer.GetForce(joystickInput);
        }
    }
}