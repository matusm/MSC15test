using Bev.Instruments.Msc15;
using At.Matus.StatisticPod;

namespace MSC15test
{
    public class SpectralQuantityValue
    {
        public int NumberOfSpectra => (int)valuePod.SampleSize;
        public double AverageValue => valuePod.AverageValue;
        public double StandardDeviation => valuePod.StandardDeviation;
        public double MaximumValue => valuePod.MaximumValue;
        public double MinimumValue => valuePod.MinimumValue;
        public double Wavelength { get; private set; }

        private StatisticPod valuePod;

        public SpectralQuantityValue()
        {
            valuePod = new StatisticPod();
        }

        public void Update(SpectralValue sv)
        {
            Update(sv.Irradiance);
            Wavelength = sv.Wavelength;
        }

        public void Update(double value)
        {
            valuePod.Update(value);
        }

        public void Restart()
        {
            valuePod.Restart();
        }
    }
}
