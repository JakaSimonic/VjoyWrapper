using System.Collections.Generic;

namespace Ffb
{
    internal interface ICalculationProvider
    {
        double GetEnvelope(ENVELOPE envParms, double elapsedTime, double duration);

        List<double> GetDirection(SET_EFFECT effectParms);

        List<double> GetCondition(List<CONDITION> condInput, List<double> joystickInput);
        double ApplyGain(double value, double gain);
    }
}