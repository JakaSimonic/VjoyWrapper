using System;
using System.Collections.Generic;

namespace Ffb
{
    public  class FfbControllerFactory
    {
        private ICalculationProvider _calculationProvider;
        private FfbControllerLogic _ffbOperation;
        private IReportDescriptorProperties _reportDescriptorProperties;

        public FfbControllerFactory(IReportDescriptorProperties reportDescriptorProperties)
        {
            _reportDescriptorProperties = reportDescriptorProperties;
            _calculationProvider = new CalculationProvider(reportDescriptorProperties);
            _ffbOperation = new FfbControllerLogic(reportDescriptorProperties, _calculationProvider);
        }

        public FfbController Create()
        {
            FfbController ffb = new FfbController(
            new Dictionary<string, Action<object>>()
              {
                {"SetEffect",  _ffbOperation.SetEffectParameter},
                {"SetEnvelope",  _ffbOperation.SetEnvelopeParameter},
                {"SetCondition",  _ffbOperation.SetConditionParameter},
                {"SetPeriodic",  _ffbOperation.SetPeriodParameter},
                {"SetConstantForce",  _ffbOperation.SetConstantParameter},
                {"SetRampForce",  _ffbOperation.SetRampParameter},
                {"EffectOperation", _ffbOperation.EffectsOperation },
                {"PIDBlockFree", _ffbOperation.PIDBlockFree },
                {"PIDDeviceControl", _ffbOperation.PIDDeviceControl },
                {"DeviceGain", _ffbOperation.DeviceGain },
                {"CreateNewEffect", _ffbOperation.CreateNewEffect },
                {"CustomForceData", _ffbOperation.NotImplemented},
                {"SetCustomForce", _ffbOperation.NotImplemented },
              },
            new Dictionary<string, Func<object>>()
              {
                {"BlockLoad", _ffbOperation.GetBlockLoad },
                {"PIDState", _ffbOperation.GetPidState}
              });

            ffb.GetForces = _ffbOperation.GetForces;

            return ffb;
        }
    }
}