using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vJoyInterfaceWrap;

namespace VjoyWrapper.Extensions
{
    public static class VjoyStateExtensions
    {
        public static void SetButton(this vJoy.JoystickState jostickState, int buttonNumber)
        {
            if (buttonNumber < 33)
            {
                jostickState.Buttons |= (uint)(1 << buttonNumber);
            }
            else if (buttonNumber < 65)
            {
                jostickState.bHatsEx1 |= (uint)(1 << (buttonNumber % 33 + 1));
            }
            else if (buttonNumber < 97)
            {
                jostickState.bHatsEx2 |= (uint)(1 << (buttonNumber % 65 + 1));
            }
            else if (buttonNumber < 129)
            {
                jostickState.bHatsEx3 |= (uint)(1 << (buttonNumber % 97 + 1));
            }
        }

    }
}
