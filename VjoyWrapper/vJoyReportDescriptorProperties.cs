using System;
using Ffb;
namespace VjoyWrapper
{
    internal class vJoyReportDescriptorProperties : IReportDescriptorProperties
    {
        public double TO_RAD { get; private set; }
        public double HALF_PI { get; private set; }
        public double MAX_GAIN { get; private set; }
        public double ENVELOPE_MAX { get; private set; }
        public double DIRECTION_MAX { get; private set; }
        public double MAX_PHASE { get; private set; }
        public int FREE_ALL_EFFECTS { get; }
        public int MAX_LOOP { get; }
        public int DOWNLOAD_FORCE_SAMPLE_AXES { get; }
        public Type DOWNLOAD_FORCE_SAMPLE_TYPE { get; }
        public int MAX_DEVICE_GAIN { get; }
        public int MAX_RAM_POOL { get; }
        public double DURATION_INFINITE { get; }
        public vJoyReportDescriptorProperties()
        {
            TO_RAD = (360d / 255d) * (Math.PI / 180d);
            HALF_PI = Math.PI / 2;

            MAX_GAIN = 255d;
            ENVELOPE_MAX = 10000d;
            DIRECTION_MAX = 255d;
            MAX_PHASE = 35999d;
            FREE_ALL_EFFECTS = 0xFF;
            MAX_LOOP = 255;
            DOWNLOAD_FORCE_SAMPLE_AXES = 2;
            DOWNLOAD_FORCE_SAMPLE_TYPE = typeof(byte);
            MAX_DEVICE_GAIN = 255;
            MAX_RAM_POOL = 65535;
            DURATION_INFINITE = -1;
}
    }
}