using System.Collections.Generic;

namespace Ffb
{
    internal interface IEffect
    {
        void Start();
        void Continue();
        void Stop();
        object GetParameter(string parmName);
        void SetParameter(string parmName, object parameter);
        List<double> GetForce(JOYSTICK_INPUT jostickInput);
        void TriggerButtonPressed();
        void TriggerButtonReleased();
    }
}