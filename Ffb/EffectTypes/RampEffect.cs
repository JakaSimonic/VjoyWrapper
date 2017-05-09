using System.Collections.Generic;

namespace Ffb
{
    internal class RampEffect : IEffectType
    {
        private ICalculationProvider _calculationProvider;

        public RampEffect(ICalculationProvider calculationProvider)
        {
            _calculationProvider = calculationProvider;
        }

        public List<double> GetForce(JOYSTICK_INPUT joystickInput, Dictionary<string, object> structDictonary, double elapsedTime)
        {
            SET_EFFECT eff = (SET_EFFECT)structDictonary["SET_EFFECT"];
            RAMP rmp = (RAMP)structDictonary["RAMP"];
            ENVELOPE env = (ENVELOPE)structDictonary["ENVELOPE"];

            List<double> forces = new List<double>();

            double end = rmp.end;
            double start = rmp.start;
            double duration = eff.duration;

            double slope = (end - start) / duration;
            double magnitude = start + slope * elapsedTime;
            magnitude = _calculationProvider.ApplyGain(magnitude, eff.gain);

            double envelope = _calculationProvider.ApplyGain(_calculationProvider.GetEnvelope(env, elapsedTime, eff.duration), eff.gain);

            List<double> directions = _calculationProvider.GetDirection(eff);
            foreach (var direction in directions)
            {
                forces.Add(magnitude * envelope * direction);
            }

            return forces;
        }
    }
}