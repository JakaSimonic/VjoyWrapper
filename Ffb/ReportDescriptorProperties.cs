using System;

namespace Ffb
{
    internal class ReportDescriptorProperties : IReportDescriptorProperties
    {
        public double MAX_GAIN { get; private set; }
        public double ENVELOPE_MAX { get; private set; }
        public double DIRECTION_MAX { get; private set; }
        public double MAX_PHASE { get; private set; }
        public int FREE_ALL_EFFECTS { get; }
        public int MAX_LOOP { get; }
        public int DOWNLOAD_FORCE_SAMPLE_AXES { get; }
        public int MAX_DEVICE_GAIN { get; }
        public int MAX_RAM_POOL { get; }
        public long DURATION_INFINITE { get; }

        public ReportDescriptorProperties()
        {
            MAX_GAIN = 255d;
            ENVELOPE_MAX = 10000d;
            DIRECTION_MAX = 255d;
            MAX_PHASE = 35999d;
            FREE_ALL_EFFECTS = 0xFF;
            MAX_LOOP = 255;
            DOWNLOAD_FORCE_SAMPLE_AXES = 2;
            MAX_DEVICE_GAIN = 255;
            MAX_RAM_POOL = 65535;
            DURATION_INFINITE = -1;
        }
    }
}