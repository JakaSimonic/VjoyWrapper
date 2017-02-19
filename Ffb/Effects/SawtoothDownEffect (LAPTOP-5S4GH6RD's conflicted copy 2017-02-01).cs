using Ffb.CalculationProvider;
using System;
using System.Collections.Generic;

namespace Ffb.Effects
{
    public class SawtoothDownEffect : IEffect
    {
        public SET_EFFECT eff;
        public PERIOD periodic;
        public ENVELOPE env;

        private readonly ICalculationProvider _calcProvider;
        private readonly IReportDescriptorProperties _reportDescriptorProperties;

        private long ticksStart;
        private long pauseTime = 0;
        private bool paused = true;

        public SawtoothDownEffect(ICalculationProvider calcProvider, IReportDescriptorProperties reportDescriptorProperties)
        {
            _calcProvider = calcProvider;
            _reportDescriptorProperties = reportDescriptorProperties;
        }

        public List<double> CalculatedForce(EFFECT_PARMS effectParms)
        {
            List<double> forces = new List<double>();
            double offset = periodic.offset;
            double magnitude = periodic.magnitude;
            double phase = periodic.phase;
            double period = periodic.period;

            magnitude = _calcProvider.ApplyGain(magnitude, eff.gain);

            double max = offset + magnitude;
            double min = offset - magnitude;
            double phasetime = (phase * period) / _reportDescriptorProperties.MAX_PHASE;
            double time = effectParms.elapsedTime + phasetime;
            double reminder = time % period;
            double slope = (max - min) / period;
            double tempforce = 0;
            tempforce = slope * reminder;
            tempforce += min;

            double envelope = _calcProvider.ApplyGain(_calcProvider.GetEnvelope(env, effectParms.elapsedTime, eff.duration), eff.gain);

            List<double> directions = _calcProvider.GetDirection(eff);
            foreach (var direction in directions)
            {
                forces.Add(tempforce * envelope * direction);
            }
            return forces;
        }

        public void Start()
        {
            ticksStart = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
            pauseTime = 0;
            paused = false;
        }

        public void Pause()
        {
            paused = true;
            pauseTime = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
        }

        public void Continue()
        {
            ticksStart += (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - pauseTime;
        }
    }
}