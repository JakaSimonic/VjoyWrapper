using System;


namespace Ffb
{
    public interface IReportDescriptorProperties
    {
        double MAX_GAIN { get;  }
        double ENVELOPE_MAX { get;  }
        double DIRECTION_MAX { get;  }
        double MAX_PHASE { get; }
        int FREE_ALL_EFFECTS { get; }
        int MAX_LOOP { get; }
        int DOWNLOAD_FORCE_SAMPLE_AXES { get; }
        int MAX_DEVICE_GAIN { get; }
        long DURATION_INFINITE { get; }
        int MAX_RAM_POOL { get; }
    }
}
