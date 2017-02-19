using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ffb
{
    public interface IReportDescriptorProperties
    {
        double TO_RAD { get;   }
        double HALF_PI { get;  }
        double MAX_GAIN { get;  }
        double ENVELOPE_MAX { get;  }
        double DIRECTION_MAX { get;  }
        double MAX_PHASE { get; }
        int FREE_ALL_EFFECTS { get; }
        int MAX_LOOP { get; }
        int DOWNLOAD_FORCE_SAMPLE_AXES { get; }
        Type DOWNLOAD_FORCE_SAMPLE_TYPE { get; }
        int MAX_DEVICE_GAIN { get; }
        double DURATION_INFINITE { get; }
        int MAX_RAM_POOL { get; }
    }
}
