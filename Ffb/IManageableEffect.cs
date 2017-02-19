using System.Collections.Generic;

namespace Ffb
{
    internal interface IManageableEffect
    {
        void Start();
        void Continue();
        void Stop();
        object GetParameter(string parmName);
        void SetParameter(string parmName, object parameter);
        List<double> GetForce(JOYSTICK_INPUT jostickInput);
    }
}