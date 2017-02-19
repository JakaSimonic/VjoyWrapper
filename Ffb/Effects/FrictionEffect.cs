using System.Collections.Generic;
using System.Linq;

namespace Ffb
{
    internal class FrictionEffect : IEffect
    {
        private readonly ICalculationProvider _calculationProvider;

        private List<double> previousAxesPositions;

        public FrictionEffect(ICalculationProvider calculationProvider)
        {
            _calculationProvider = calculationProvider;
        }

        public List<double> GetForce(JOYSTICK_INPUT joystickInput, Dictionary<string, object> structDictonary, double elapsedTime)
        {
            SET_EFFECT eff = (SET_EFFECT)structDictonary["SET_EFFECT"];
            List<CONDITION> cond = (List<CONDITION>)structDictonary["CONDITION"];
            ENVELOPE env = (ENVELOPE)structDictonary["ENVELOPE"];

            List<double> forces = joystickInput.axesPositions.Select(x => 0d).ToList();

            if (previousAxesPositions != null)
            {
                var axisSpeed = joystickInput.axesPositions.Zip(previousAxesPositions, (u, v) => u - v).ToList();

                forces = _calculationProvider.GetCondition(cond, axisSpeed);
                forces = forces.Select(x => -x).ToList();
            }

            previousAxesPositions = joystickInput.axesPositions;

            return forces;
        }
    }
}