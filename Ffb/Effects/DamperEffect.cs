using System;
using System.Collections.Generic;
using System.Linq;

namespace Ffb
{
    internal class DamperEffect : IEffect
    {
        public Dictionary<string, object> structDictonary = new Dictionary<string, object>();

        private readonly ICalculationProvider _calculationProvider;

        private List<double> previousAxesPositions;

        public DamperEffect(ICalculationProvider calculationProvider)
        {
            _calculationProvider = calculationProvider;
        }

        public List<double> GetForce(JOYSTICK_INPUT joystickInput, Dictionary<string, object> structDictonary, double elapsedTime)
        {
            ENVELOPE env = (ENVELOPE)structDictonary["ENVELPOPE"];
            SET_EFFECT eff = (SET_EFFECT)structDictonary["SET_EFFECT"];
            List<CONDITION> cond = (List<CONDITION>)structDictonary["CONDITION"];

            List<double> forces = joystickInput.axesPositions.Select(x => 0d).ToList();

            if (previousAxesPositions != null)
            {
                var axisSpeed = joystickInput.axesPositions.Zip(previousAxesPositions, (u, v) => u - v).ToList();
                forces = _calculationProvider.GetCondition(cond, axisSpeed);
            }

            previousAxesPositions = joystickInput.axesPositions;

            return forces;
        }
    }
}