using System;
using System.Globalization;
using System.IO;
using Bev.Instruments.Msc15;

namespace MSC15test
{
    class Program
    {
        static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            var device = new Msc15("MSC15_0");

            Console.WriteLine($"ID: {device.InstrumentID}");
            //device.MeasureDark();

            NativeSpectrum spectrum = new NativeSpectrum();

            for (int i = 0; i < 10; i++)
            {
                device.Measure();
                Console.WriteLine($"{i+1,4}:   {device.CctValue:F0} K    {device.PhotopicValue:F2} lx  {device.GetLastIntegrationTime():F4} s");
                var singleSpec = device.GetNativeSpectrum();
                spectrum.Update(singleSpec);
            }

            Console.WriteLine();

            StreamWriter streamWriter = new StreamWriter("MSC15nativeAverage.csv", false);
            string csvHeader = $"index , wavelength , average irradiance , minimum, maximum, standard deviation";
            streamWriter.WriteLine(csvHeader);
            Console.WriteLine(csvHeader);

            for (int i = 0; i < spectrum.Spectrum.Length; i++)
            {
                var point = spectrum.Spectrum[i];
                string csvLine = $"{i,3} , {point.Wavelength:F2} , {point.AverageValue} , {point.MinimumValue} , {point.MaximumValue} , {point.StandardDeviation}";
                streamWriter.WriteLine(csvLine);
                Console.WriteLine(csvLine);
            }
            streamWriter.Close();

        }
    }
}
