using Bev.Instruments.Msc15;

namespace MSC15test
{
    public abstract class BaseSpectrum
    {
        private readonly int numberOfWavelengths;
        private SpectralQuantityValue[] genericSpectrum;

        public SpectralQuantityValue[] Spectrum => genericSpectrum;
        public int NumberOfSpectra => genericSpectrum[0].NumberOfSpectra;

        public BaseSpectrum(int numberOfWavelengths)
        {
            this.numberOfWavelengths = numberOfWavelengths;
            genericSpectrum = new SpectralQuantityValue[this.numberOfWavelengths];
            for (int i = 0; i < this.numberOfWavelengths; i++)
                genericSpectrum[i] = new SpectralQuantityValue();
        }

        public void Update(SpectralValue[] spectrum)
        {
            if (spectrum.Length != numberOfWavelengths)
                return;
            for (int i = 0; i < spectrum.Length; i++)
                genericSpectrum[i].Update(spectrum[i]);
        }

        public void Restart()
        {
            for (int i = 0; i < genericSpectrum.Length; i++)
                genericSpectrum[i].Restart();
        }

    }
}
