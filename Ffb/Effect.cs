using System;
using System.Collections.Generic;
using System.Linq;

namespace Ffb
{
    internal class Effect : IEffect
    {
        private long ticksStart;
        private long pauseTime = 0;
        private long lastUpdate;
        private bool paused = true;
        private bool buttonReleased = false;
        private IEffectType _effect;
        private Dictionary<string, object> _structDictonary;
        private IReportDescriptorProperties _reportDescriptorProperties;

        public Effect(IEffectType effect, IReportDescriptorProperties reportDescriptorProperties)
        {
            _effect = effect;
            _structDictonary = new Dictionary<string, object>();
            _reportDescriptorProperties = reportDescriptorProperties;
        }

        public void Start()
        {
            paused = false;
            long startDelay = ((SET_EFFECT)_structDictonary["SET_EFFECT"]).startDelay;
            ticksStart = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) + startDelay;
            lastUpdate = ticksStart;
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
                long elapsedTime = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - ticksStart;
                lastUpdate = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - lastUpdate;
                SET_EFFECT setEffect = (SET_EFFECT)_structDictonary["SET_EFFECT"];

                long duration = setEffect.duration;
                long samplePeriod = setEffect.samplePeriod;

                if (((elapsedTime < duration) && (elapsedTime > 0) && (lastUpdate > samplePeriod)) || (duration == _reportDescriptorProperties.DURATION_INFINITE))
                {
                    forces = _effect.GetForce(joystickInput, _structDictonary, elapsedTime);
                }
            }
            return forces;
        }

        public void SetParameter(string parmName, object parameter)
        {
            if (!parmName.Equals("CONDITION") && !parmName.Equals("CUSTOM_FORCE_DATA_REPORT"))
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
            else if (parmName == "CONDITION")
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
                    List<int> samples = ((CUSTOM_FORCE_DATA_REPORT)parameter).samples;
                    ((CUSTOM_FORCE_DATA_REPORT)_structDictonary[parmName]).samples.AddRange(samples);
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

        public void TriggerButtonPressed()
        {
            if (paused)
            {
                Start();
                buttonReleased = false;
                return;
            }

            if (!buttonReleased)
            {
                long elapsedTime = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - ticksStart;
                SET_EFFECT setEffect = (SET_EFFECT)_structDictonary["SET_EFFECT"];
                long duration = setEffect.duration;
                long trigerRepeat = setEffect.trigerRepeat;
                if ((duration + trigerRepeat) > elapsedTime)
                {
                    Start();
                }
            }
        }

        public void TriggerButtonReleased()
        {
            buttonReleased = true;
            paused = true;
        }
    }
}