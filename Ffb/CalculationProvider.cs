using System;
using System.Collections.Generic;
using System.Linq;

namespace Ffb
{
    internal class CalculationProvider : ICalculationProvider
    {
        private IReportDescriptorProperties _reportDescriptorProperties;

        double TO_RAD = (360d / 255d) * (Math.PI / 180d);
        double HALF_PI = Math.PI / 2;

        public CalculationProvider(IReportDescriptorProperties reportDescriptorProperties)
        {
            _reportDescriptorProperties = reportDescriptorProperties;
        }

        public double ApplyGain(double value, double gain)
        {
            return ((value * gain) / _reportDescriptorProperties.MAX_GAIN);
        }

        public double GetEnvelope(ENVELOPE envParms, double elapsedTime, double duration)
        {
            double attackTime = envParms.attackTime;
            double fadeTime = envParms.fadeTime;
            double attackLevel = envParms.attackLevel;
            double fadeLevel = envParms.fadeLevel;
            double envelope = 1;

            if (elapsedTime < attackTime)
            {
                double attackSlope = (_reportDescriptorProperties.ENVELOPE_MAX - attackLevel) / attackTime;
                envelope = (attackLevel + (attackSlope * elapsedTime)) / _reportDescriptorProperties.ENVELOPE_MAX;
            }
            if (elapsedTime > (duration - fadeTime))
            {
                double fadeSlope = (_reportDescriptorProperties.ENVELOPE_MAX - fadeLevel) / fadeTime;
                envelope = (fadeLevel + (fadeSlope * (duration - elapsedTime))) / _reportDescriptorProperties.ENVELOPE_MAX;
            }

            return envelope;
        }

        public List<double> GetDirection(SET_EFFECT dirParms)
        {
            double directionX = dirParms.directionX;
            double directionY = dirParms.directionY;
            double polarDirection = dirParms.direction;

            double angle;
            List<double> axes = new List<double>();

            if (dirParms.polar)
            {
                angle = polarDirection * TO_RAD;
            }
            else
            {
                double x = directionX / _reportDescriptorProperties.DIRECTION_MAX;
                double y = directionY / _reportDescriptorProperties.DIRECTION_MAX;
                angle = Math.Atan2(y, x) + HALF_PI;
            }

            axes.Add(Math.Cos(angle));
            axes.Add(Math.Sin(angle));
            return axes;
        }

        public List<double> GetCondition(List<CONDITION> condInput, List<double> axesPosition)
        {
            List<double> axisForces = new List<double>();

            if (condInput.Count > 1)
            {
                var combinedList = condInput.Zip(axesPosition, (u, v) => new { condition = u, position = v });
                foreach (var comb in combinedList)
                {
                    double deadBand = comb.condition.deadBand;
                    double cpOffset = comb.condition.cpOffset;
                    double negativeSaturation = -comb.condition.negativeSaturation;
                    double positiveSaturation = comb.condition.positiveSaturation;
                    double negativeCoefficient = -comb.condition.negativeCoefficient;
                    double positiveCoefficient = comb.condition.positiveCoefficient;

                    if (comb.position < (cpOffset - deadBand))
                    {
                        double tempForce = (comb.position - (cpOffset - deadBand)) * negativeCoefficient;
                        axisForces.Add(tempForce < negativeSaturation ? negativeSaturation : tempForce);
                    }
                    else if (comb.position > (cpOffset + deadBand))
                    {
                        double tempForce = (comb.position - (cpOffset + deadBand)) * positiveCoefficient;
                        axisForces.Add(tempForce > positiveSaturation ? positiveSaturation : tempForce);
                    }
                    else axisForces.Add(0);
                }
                return axisForces;
            }

            if (condInput.Count == 1)
            {
                double tempForce = 0;
                double deadBand = condInput[0].deadBand;
                double cpOffset = condInput[0].cpOffset;
                double negativeSaturation = -condInput[0].negativeSaturation;
                double positiveSaturation = condInput[0].positiveSaturation;
                double negativeCoefficient = condInput[0].negativeCoefficient;
                double positiveCoefficient = condInput[0].positiveCoefficient;

                foreach (var axisPosition in axesPosition)
                {
                    if (axisPosition < (cpOffset - deadBand))
                    {
                        tempForce = ((cpOffset - deadBand) - axisPosition) * negativeCoefficient;
                        axisForces.Add(tempForce < negativeSaturation ? negativeSaturation : tempForce);
                    }
                    else if (axisPosition > (cpOffset + deadBand))
                    {
                        tempForce = (axisPosition - (cpOffset + deadBand)) * positiveCoefficient;
                        axisForces.Add(tempForce > positiveSaturation ? positiveSaturation : tempForce);
                    }
                    else axisForces.Add(0d);
                }
                return axisForces;
            }

            return axisForces;
        }
    }
}