using System.Collections.Generic;
using System.Linq;

namespace Ffb
{
    internal class EffectsLogic
    {
        private List<IManageableEffect> _effectsList;
        private double _deviceGain;
        private Dictionary<EFFECT_TYPE, IEffect> efectsDict;
        private IReportDescriptorProperties _reportDescriptorProperties;

        public EffectsLogic(IReportDescriptorProperties reportDescriptorProperties, ICalculationProvider calculationProvider)
        {
            _effectsList = new List<IManageableEffect>();
            _reportDescriptorProperties = reportDescriptorProperties;
            efectsDict = new Dictionary<EFFECT_TYPE, IEffect>()
              {
                {EFFECT_TYPE.CONSTANT, new ConstantEffect(calculationProvider) },
                {EFFECT_TYPE.RAMP,new RampEffect(calculationProvider)},
                {EFFECT_TYPE.SQUARE, new SquareEffect(calculationProvider,reportDescriptorProperties)},
                {EFFECT_TYPE.SINE, new SineEffect(calculationProvider,reportDescriptorProperties)},
                {EFFECT_TYPE.TRIANGLE,new TriangleEffect(calculationProvider,reportDescriptorProperties) },
                {EFFECT_TYPE.SAWTOOTH_UP, new SawtoothDownEffect(calculationProvider,reportDescriptorProperties) },
                {EFFECT_TYPE.SAWTOOTH_DOWN,new SawtoothUpEffect(calculationProvider,reportDescriptorProperties) },
                {EFFECT_TYPE.SPRING, new SpringEffect(calculationProvider) },
                {EFFECT_TYPE.DAMPER, new DamperEffect(calculationProvider)},
                {EFFECT_TYPE.INERTIA, new InertiaEffect(calculationProvider) },
                {EFFECT_TYPE.FRICTION, new FrictionEffect(calculationProvider) },
                {EFFECT_TYPE.CUSTOM, null}
              };
        }

        public int GetFirstFree()
        {
            return _effectsList.GetFirstFreeSlot();
        }

        public void FreeEffect(int effectBlockIndex)
        {
            _effectsList.InsertEffect(effectBlockIndex, null);
        }

        public void FreeAllEffects()
        {
            _effectsList = _effectsList.Select(x => (IManageableEffect)null).ToList();
        }

        public void StopAllEffects()
        {
            foreach (var manageadEffect in _effectsList)
            {
                manageadEffect?.Stop();
            }
        }

        public void StartEffect(int effectBlockIndex)
        {
            IManageableEffect manageadEffect = _effectsList.GetEffect(effectBlockIndex);
            manageadEffect.Start();
            _effectsList.InsertEffect(effectBlockIndex, manageadEffect);
        }

        public void StartAllEffects()
        {
            foreach (var manageadEffect in _effectsList)
            {
                manageadEffect?.Start();
            }
        }

        public void StopEffect(int effectBlockIndex)
        {
            IManageableEffect manageadEffect = _effectsList.GetEffect(effectBlockIndex);
            manageadEffect.Stop();
            _effectsList.InsertEffect(effectBlockIndex, manageadEffect);
        }

        public void ContinueAllEffects()
        {
            foreach (var manageadEffect in _effectsList)
            {
                manageadEffect?.Continue();
            }
        }

        public void ContinueEffect(int effectBlockIndex)
        {
            IManageableEffect manageadEffect = _effectsList.GetEffect(effectBlockIndex);
            manageadEffect.Continue();
            _effectsList.InsertEffect(effectBlockIndex, manageadEffect);
        }

        public List<double> GetForce(JOYSTICK_INPUT joystickInput)
        {
            List<double> forcesSum = joystickInput.axesPositions.Select(x => 0d).ToList();

            foreach (var manageadEffect in _effectsList.ToList())
            {
                if (manageadEffect != null)
                {
                    List<double> forces = manageadEffect.GetForce(joystickInput);
                    forcesSum = forcesSum.Zip(forces, (u, v) => u + v).ToList();
                }
            }
            forcesSum = forcesSum.Select(x => x * _deviceGain).ToList();
            return forcesSum;
        }

        public void CreateNewEffect(int effectBlockIndex, string parmName, object parameter)
        {
            EFFECT_TYPE effetType = ((CREATE_NEW_EFFECT)parameter).effetType;
            IManageableEffect manageadEffect = new ManageableEffect(efectsDict[effetType], _reportDescriptorProperties);
            _effectsList.InsertEffect(effectBlockIndex, manageadEffect);
        }

        public void SetParameter(int effectBlockIndex, string parmName, object parameter)
        {
            IManageableEffect manageadEffect = _effectsList.GetEffect(effectBlockIndex);
            manageadEffect.SetParameter(parmName, parameter);
            _effectsList.InsertEffect(effectBlockIndex, manageadEffect);
        }

        public void SetDuration(int effectBlockIndex, double loopCount)
        {
            IManageableEffect manageadEffect = _effectsList.GetEffect(effectBlockIndex);
            SET_EFFECT setEffect = (SET_EFFECT)manageadEffect.GetParameter("SET_EFFECT");

            if (loopCount == _reportDescriptorProperties.MAX_LOOP)
            {
                ENVELOPE envelope = (ENVELOPE)manageadEffect.GetParameter("ENVELOPE");

                setEffect.duration = _reportDescriptorProperties.DURATION_INFINITE;
                envelope.fadeTime = 0d;
                manageadEffect.SetParameter("ENVELOPE", envelope);
            }
            else
            {
                setEffect.duration *= loopCount;
            }
            manageadEffect.SetParameter("SET_EFFECT", setEffect);

            _effectsList.InsertEffect(effectBlockIndex, manageadEffect);
        }

        public void SetDeviceGain(double deviceGain)
        {
            _deviceGain = deviceGain;
        }
    }
}