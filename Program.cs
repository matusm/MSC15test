using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            for (int i = 0; i < 10; i++)
            {
                device.Measure();
                Console.WriteLine($"{i+1,4}:   {device.CctValue:F0} K    {device.PhotopicValue:F2} lx  {device.GetLastIntegrationTime():F4} s");
            }

            Console.WriteLine();
            var spec = device.GetVisSpectrum();

            for (int i = 0; i < spec.Length; i++)
            {
                Console.WriteLine($"{i,4} {spec[i].Wavelength:F2} nm  ->  {spec[i].Irradiance:F6}");
            }

            var streamWriter = new StreamWriter("MSC15vis.csv", false);
            streamWriter.WriteLine("wavelength / nm , irradiance / W/(m²cm)");
            for (int i = 0; i < spec.Length; i++)
            {
                streamWriter.WriteLine($"{spec[i].Wavelength} , {spec[i].Irradiance}");
            }
            streamWriter.Close();

            streamWriter = new StreamWriter("MSC15native.csv", false);
            streamWriter.WriteLine("wavelength / nm , irradiance / W/(m²cm)");
            spec = device.GetNativeSpectrum();
            for (int i = 0; i < spec.Length; i++)
            {
                streamWriter.WriteLine($"{spec[i].Wavelength:F2} , {spec[i].Irradiance}");
            }
            streamWriter.Close();

        }
    }
}
