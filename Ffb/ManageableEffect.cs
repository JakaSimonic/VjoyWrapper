using System;
using System.Collections.Generic;
using System.Linq;

namespace Ffb
{
    internal class ManageableEffect : IManageableEffect
    {
        private long ticksStart;
        private long pauseTime = 0;
        private bool paused = true;
        private IEffect _effect;
        private Dictionary<string, object> _structDictonary;
        private IReportDescriptorProperties _reportDescriptorProperties;

        public ManageableEffect(IEffect effect, IReportDescriptorProperties reportDescriptorProperties)
        {
            _effect = effect;
            _structDictonary = new Dictionary<string, object>();
            _reportDescriptorProperties = reportDescriptorProperties;
        }

        public void Start()
        {
            paused = false;
            ticksStart = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) + ((SET_EFFECT)_structDictonary["SET_EFFECT"]).startDelay;
            pauseTime = 0;
        }

        public void Stop()
        {
            paused = true;
            pauseTime = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
        }

        public void Continue()
        {
            paused = false;
            ticksStart += (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - pauseTime;
            pauseTime = 0;
        }

        public List<double> GetForce(JOYSTICK_INPUT joystickInput)
        {
            List<double> forces = joystickInput.axesPositions.Select(x => 0d).ToList();

            if (!paused)
            {
                double elapsedTime = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - ticksStart;

                SET_EFFECT setEffect = (SET_EFFECT)_structDictonary["SET_EFFECT"];

                double duration = setEffect.duration;

                #region TriggerNotImplemented

                //int? triggerButton = setEffect.trigerButton;
                //if (triggerButton != null)
                //{
                //    paused = true;
                //    foreach (var button in joystickInput.triggerButtonOffsets)
                //    {
                //        if (button == triggerButton)
                //        {
                //            throw new NotImplementedException();
                //        }
                //    }
                //}

                #endregion TriggerNotImplemented

                if (((elapsedTime < duration) && (elapsedTime > 0)) || (duration == _reportDescriptorProperties.DURATION_INFINITE))
                {
                    forces = _effect.GetForce(joystickInput, _structDictonary, elapsedTime);
                }
            }
            return forces;
        }

        public void SetParameter(string parmName, object parameter)
        {
            if (parmName == "CONDITION")
            {
                if (_structDictonary.ContainsKey(parmName))
                {
                    List<CONDITION> conditionList = (List<CONDITION>)_structDictonary[parmName];
                    conditionList.Add((CONDITION)parameter);
                    _structDictonary[parmName] = conditionList;
                }
                else
                {
                    List<CONDITION> conditionList = new List<CONDITION>();
                    conditionList.Add((CONDITION)parameter);
                    _structDictonary.Add(parmName, conditionList);
                }
            }
            else
            {
                if (_structDictonary.ContainsKey(parmName))
                {
                    _structDictonary[parmName] = parameter;
                }
                else
                {
                    _structDictonary.Add(parmName, parameter);
                }
            }
        }

        public object GetParameter(string parmName)
        {
            return _structDictonary[parmName];
        }
    }
}