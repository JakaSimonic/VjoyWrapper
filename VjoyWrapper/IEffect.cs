using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotoJoy
{
    public interface IEffect
    {
        double[] CalculatedForce();
        void Start();
        void Pause();
        void Continue();
    }
}
