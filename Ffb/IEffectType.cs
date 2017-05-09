using System.Collections.Generic;

namespace Ffb
{
    internal interface IEffectType
    {
        List<double> GetForce(JOYSTICK_INPUT joystickInput, Dictionary<string, object> structDictonary, double elapsedTime);
    }
}