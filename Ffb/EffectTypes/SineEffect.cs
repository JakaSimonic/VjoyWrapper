using System;
using System.Collections.Generic;

namespace Ffb
{
    internal class SineEffect : IEffectType
    {
        private readonly ICalculationProvider _calculationProvider;
        private readonly IReportDescriptorProperties _reportDescriptorProperties;

        public SineEffect(ICalculationProvider calculationProvider, IReportDescriptorProperties reportDescriptorProperties)
        {
            _calculationProvider = calculationProvider;
            _reportDescriptorProperties = reportDescriptorProperties;
        }

        public List<double> GetForce(JOYSTICK_INPUT joystickInput, Dictionary<string, object> structDictonary, double elapsedTime)
        {
            SET_EFFECT eff = (SET_EFFECT)structDictonary["SET_EFFECT"];
            PERIOD periodic = (PERIOD)structDictonary["PERIOD"];
            ENVELOPE env = (ENVELOPE)structDictonary["ENVELOPE"];

            List<double> forces = new List<double>();
            double offset = periodic.offset;
            double magnitude = periodic.magnitude;
            double phase = periodic.phase;
            double period = periodic.period;

            magnitude = _calculationProvider.ApplyGain(magnitude, eff.gain);

            double angle = ((elapsedTime / period) + (phase / _reportDescriptorProperties.MAX_PHASE) * period) * 2 * Math.PI;
            double sine = Math.Sin(angle);
            double tempforce = sine * magnitude;
            tempforce += offset;

            double envelope = _calculationProvider.ApplyGain(_calculationProvider.GetEnvelope(env, elapsedTime, eff.duration), eff.gain);

            List<double> directions = _calculationProvider.GetDirection(eff);
            foreach (var direction in directions)
            {
                forces.Add(tempforce * envelope * direction);
            }
            return forces;
        }
    }
}