using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bev.Instruments.Msc15;

namespace MSC15test
{
    public class NativeSpectrum
    {
        private const int numberPixel = 288;
        private SpectralQuantityValue[] nativeSpectrum;

        public SpectralQuantityValue[] Spectrum => nativeSpectrum;
        public int NumberOfSpectra => nativeSpectrum[0].NumberOfSpectra;

        public NativeSpectrum()
        {
            nativeSpectrum = new SpectralQuantityValue[numberPixel];
            for (int i = 0; i < numberPixel; i++)
            {
                nativeSpectrum[i] = new SpectralQuantityValue();
            }
        }

        public void Update(SpectralValue[] spectrum)
        {
            if (spectrum.Length != numberPixel)
                return;
            for (int i = 0; i < spectrum.Length; i++)
            {
                nativeSpectrum[i].UpdateValue(spectrum[i]);
            }
        }

    }
}
