using System.Collections.Generic;

namespace Ffb
{
    internal interface IEffect
    {
        List<double> GetForce(JOYSTICK_INPUT joystickInput, Dictionary<string, object> structDictonary, double elapsedTime);
    }
}