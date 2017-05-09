using System.Collections.Generic;
using System.Linq;
using TinyIoC;
namespace Ffb
{
    internal class EffectsContainer
    {
        private List<IEffect> _effects;
        private double _deviceGain;
        private Dictionary<EFFECT_TYPE, IEffectType> efectsDict;
        private IReportDescriptorProperties _reportDescriptorProperties;

        public EffectsContainer(IReportDescriptorProperties reportDescriptorProperties)
        {
            _effects = new List<IEffect>();

            ICalculationProvider calculationProvider = TinyIoCContainer.Current.Resolve<ICalculationProvider>();
            _reportDescriptorProperties = reportDescriptorProperties;

            efectsDict = new Dictionary<EFFECT_TYPE, IEffectType>()
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
                {EFFECT_TYPE.CUSTOM, new CustomEffect(calculationProvider)}
              };
        }

        public int GetFirstFree()
        {
            return _effects.GetFirstFreeSlot();
        }

        public void FreeEffect(int effectBlockIndex)
        {
            _effects.InsertEffect(effectBlockIndex, null);
        }

        public void FreeAllEffects()
        {
            _effects = _effects.Select(x => (IEffect)null).ToList();
        }

        public void StopAllEffects()
        {
            foreach (var effect in _effects)
            {
                effect?.Stop();
            }
        }

        public void StartEffect(int effectBlockIndex)
        {
            IEffect effect = _effects.GetEffect(effectBlockIndex);
            effect.Start();
            _effects.InsertEffect(effectBlockIndex, effect);
        }

        public void StartAllEffects()
        {
            foreach (var effect in _effects)
            {
                effect?.Start();
            }
        }

        public void StopEffect(int effectBlockIndex)
        {
            IEffect effect = _effects.GetEffect(effectBlockIndex);
            effect.Stop();
            _effects.InsertEffect(effectBlockIndex, effect);
        }

        public void ContinueAllEffects()
        {
            foreach (var effect in _effects)
            {
                effect?.Continue();
            }
        }

        public void ContinueEffect(int effectBlockIndex)
        {
            IEffect effect = _effects.GetEffect(effectBlockIndex);
            effect.Continue();
            _effects.InsertEffect(effectBlockIndex, effect);
        }

        public List<double> GetForce(JOYSTICK_INPUT joystickInput)
        {
            ManageTriggerEffects(joystickInput.pressedButtonOffsets);

            List<double> forcesSum = joystickInput.axesPositions.Select(x => 0d).ToList();

            foreach (var effect in _effects.ToList())
            {
                if (effect != null)
                {
                    List<double> forces = effect.GetForce(joystickInput);
                    forcesSum = forcesSum.Zip(forces, (u, v) => u + v).ToList();
                }
            }
            forcesSum = forcesSum.Select(x => x * _deviceGain).ToList();
            return forcesSum;
        }

        private void ManageTriggerEffects(List<int> pressedButtons)
        {
            foreach (var effect in _effects.ToList())
            {
                if (effect != null)
                {
                    int triggerButton = ((SET_EFFECT)effect.GetParameter("SET_EFFECT")).trigerButton;
                    if(pressedButtons.Contains(triggerButton))
                    {
                        effect.TriggerButtonPressed();                        
                    }
                    else
                    {
                        effect.TriggerButtonReleased();
                    }
                }
            }
        }

        public void CreateNewEffect(int effectBlockIndex, string parmName, object parameter)
        {
            EFFECT_TYPE effetType = ((CREATE_NEW_EFFECT)parameter).effetType;
            IEffect effect = new Effect(efectsDict[effetType], _reportDescriptorProperties);
            _effects.InsertEffect(effectBlockIndex, effect);
        }

        public void SetParameter(int effectBlockIndex, string parmName, object parameter)
        {
            IEffect effect = _effects.GetEffect(effectBlockIndex);
            effect.SetParameter(parmName, parameter);
            _effects.InsertEffect(effectBlockIndex, effect);
        }

        public void SetDuration(int effectBlockIndex, long loopCount)
        {
            IEffect effect = _effects.GetEffect(effectBlockIndex);
            SET_EFFECT setEffect = (SET_EFFECT)effect.GetParameter("SET_EFFECT");

            if (loopCount == _reportDescriptorProperties.MAX_LOOP)
            {
                ENVELOPE envelope = (ENVELOPE)effect.GetParameter("ENVELOPE");

                setEffect.duration = _reportDescriptorProperties.DURATION_INFINITE;
                envelope.fadeTime = 0d;
            }
            else
            {
                setEffect.duration *= loopCount;
            }
        }

        public void SetDeviceGain(double deviceGain)
        {
            _deviceGain = deviceGain;
        }
    }
}