using System;
using System.Collections.Generic;

namespace Ffb
{
    public class FfbController
    {
        private Dictionary<string, Action<object>> _dictSet = null;
        private Dictionary<string, Func<object>> _dictGet = null;

        public FfbController(Dictionary<string, Action<object>> dictSet, Dictionary<string, Func<object>> dictGet)
        {
            _dictSet = dictSet;
            _dictGet = dictGet;
        }

        public Func<JOYSTICK_INPUT,List<double>> GetForces;

        public Dictionary<string, Action<object>> SetReport
        {
            get
            {
                return _dictSet;
            }
            set
            {
                _dictSet = value;
            }
        }

        public Dictionary<string, Func<object>> Get
        {
            get
            {
                return _dictGet;
            }
            set
            {
                _dictGet = value;
            }
        }

    }
}