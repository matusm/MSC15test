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
            MeasureDarkOffset();
            SetDynamicDarkMode();


            NativeSpectrum spectrum = new NativeSpectrum();

            for (int i = 0; i < 30; i++)
            {
                device.Measure();
                Console.WriteLine($"{i+1,4}:   {device.CctValue:F0} K    {device.PhotopicValue:F2} lx  {device.GetLastIntegrationTime():F4} s");
                var singleSpec = device.GetNativeSpectrum();
                spectrum.Update(singleSpec);
            }

            Console.WriteLine();

            StreamWriter streamWriter = new StreamWriter("CSS45nativeAverage.csv", false);
            string csvHeader = $"index , wavelength , average irradiance , minimum , maximum, standard deviation";
            streamWriter.WriteLine(csvHeader);
            Console.WriteLine(csvHeader);

            for (int i = 0; i < spectrum.Spectrum.Length; i++)
            {
                var point = spectrum.Spectrum[i];
                string csvLine = $"{i,3} , {point.Wavelength:F5} , {point.AverageValue:G5} , {point.MinimumValue:G5} , {point.MaximumValue:G5} , {point.StandardDeviation:G5}";
                streamWriter.WriteLine(csvLine);
                Console.WriteLine(csvLine);
            }
            streamWriter.Close();

            /***************************************************/
            void SetDynamicDarkMode()
            {
                if (device.HasShutter)
                {
                    device.ActivateDynamicDarkMode();
                    Console.WriteLine("Dynamic dark mode activated.");
                }
                else
                {
                    device.DeactivateDynamicDarkMode();
                    Console.WriteLine("Dynamic dark mode deactivated.");
                }
            }
            /***************************************************/
            void MeasureDarkOffset()
            {
                if (device.HasShutter)
                {
                    Console.WriteLine("measure dark offset ...");
                    device.MeasureDark();
                }
                else
                {
                    // manual close shutter (only for MSC15)
                    Console.WriteLine("close shutter and press enter");
                    Console.ReadLine();
                    device.MeasureDark();
                    Console.WriteLine("open Shutter and press enter");
                    Console.ReadLine();
                }
            }
            /***************************************************/


        }


    }
}
