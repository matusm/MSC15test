using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using At.Matus.StatisticPod;
using Bev.Instruments.Msc15;

namespace MSC15test
{
    class Program
    {
        static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            DateTime timeStamp = DateTime.UtcNow;
            string appName = Assembly.GetExecutingAssembly().GetName().Name;
            var appVersion = Assembly.GetExecutingAssembly().GetName().Version;
            string appVersionString = $"{appVersion.Major}.{appVersion.Minor}";

            Options options = new Options();
            if (!CommandLine.Parser.Default.ParseArgumentsStrict(args, options))
                Console.WriteLine("*** ParseArgumentsStrict returned false");

            var streamWriter = new StreamWriter(options.LogFileName, true);
            var stpE = new StatisticPod("illuminance");
            var stpS = new StatisticPod("scotopic");
            var covXY = new CovariancePod("xy1932");
            var covUV = new CovariancePod("uprime vprime");
            var stpCct = new StatisticPod("CCT");
            var stpT = new StatisticPod("internal temperature");
            var stpPeak = new StatisticPod("peak WL");
            var stpCen = new StatisticPod("centre WL");
            var stpCog = new StatisticPod("centroid WL");
            var stpFwhm = new StatisticPod("FWHM");
            var stpIntTime = new StatisticPod("integration time");
            if (options.MaximumSamples < 2) options.MaximumSamples = 2;
            var device = new Msc15("MSC15_0");
            RawSpectrum rawSpectrum = new RawSpectrum();
            VisSpectrum visSpectrum = new VisSpectrum();

            //  format user comment
            string prefixForIndex = $"{options.UserComment.Trim()} - ";
            if (string.IsNullOrWhiteSpace(options.UserComment))
            {
                options.UserComment = "---";
                prefixForIndex = string.Empty;
            }

            DisplayOnly("");
            LogOnly(fatSeparator);
            DisplayOnly($"Application:  {appName} {appVersionString}");
            LogOnly($"Application:  {appName} {appVersion}");
            LogAndDisplay($"DLL version:  {device.DllVersion}");
            LogAndDisplay($"StartTimeUTC: {timeStamp:dd-MM-yyyy HH:mm}");
            LogAndDisplay($"InstrumentID: {device.InstrumentManufacturer} {device.InstrumentID}"); 
            LogAndDisplay($"Samples (n):  {options.MaximumSamples}");
            LogAndDisplay($"Comment:      {options.UserComment}");
            LogOnly(fatSeparator);
            DisplayOnly("");

            if (options.NoOffset == false) MeasureDarkOffset();
            SetDynamicDarkMode();

            DisplayOnly("");

            int measurementIndex = 0;

            bool shallLoop = true;
            while (shallLoop)
            {
                DisplayOnly("press any key to start a measurement - 'd' to get dark offset, 'q' to quit");
                ConsoleKeyInfo cki = Console.ReadKey(true);
                switch (cki.Key)
                {
                    case ConsoleKey.Q:
                        shallLoop = false;
                        break;
                    case ConsoleKey.D:
                        MeasureDarkOffset();
                        LogOnly($"Dark offset measured at {DateTime.UtcNow:dd-MM-yyyy HH:mm:ss}");
                        LogOnly(thinSeparator);
                        break;
                    default:
                        int iterationIndex = 0;
                        measurementIndex++;
                        DisplayOnly("");
                        DisplayOnly($"Measurement #{measurementIndex}");
                        RestartValues();
                        timeStamp = DateTime.UtcNow;

                        while (iterationIndex < options.MaximumSamples)
                        {
                            iterationIndex++;
                            device.Measure();
                            UpdateValues();
                            DisplayOnly($"{iterationIndex,4}:  {device.CctValue:F0} K  {device.PhotopicValue:F2} lx  {device.PeakWL:F1} nm  {device.Fwhm:F1} nm");
                        }

                        string rawSpecFilename = $"{options.SpecFilePrefix}{device.InstrumentType}_SN{device.InstrumentSerialNumber}_RAW_{timeStamp:yyyyMMddHHmmss}.csv";
                        SaveRawSpectrum(rawSpecFilename);
                        string visSpecFilename = $"{options.SpecFilePrefix}{device.InstrumentType}_SN{device.InstrumentSerialNumber}_VIS_{timeStamp:yyyyMMddHHmmss}.csv";
                        SaveVisSpectrum(visSpecFilename);

                        DisplayOnly("");
                        LogOnly($"Measurement number:            {prefixForIndex}{measurementIndex}");
                        LogOnly($"Triggered at:                  {timeStamp:dd-MM-yyyy HH:mm:ss}");
                        LogOnly($"Spectrum (raw):                {rawSpecFilename}");
                        LogOnly($"Spectrum (vis):                {visSpecFilename}");
                        LogAndDisplay($"Photopic illuminance:          {stpE.AverageValue:F2} ± {stpE.StandardDeviation:F2} lx");
                        LogAndDisplay($"Scotopic illuminance:          {stpS.AverageValue:F2} ± {stpS.StandardDeviation:F2} lx");
                        LogAndDisplay($"CCT value:                     {stpCct.AverageValue:F1} ± {stpCct.StandardDeviation:F1} K");
                        LogAndDisplay($"x (CIE 1931):                  {covXY.AverageValueOfX:F5} ± {covXY.StandardDeviationOfX:F5}");
                        LogAndDisplay($"y (CIE 1931):                  {covXY.AverageValueOfY:F5} ± {covXY.StandardDeviationOfY:F5}");
                        LogAndDisplay($"r_xy (CIE 1931):               {covXY.CorrelationCoefficient:F3}");
                        LogAndDisplay($"u' (CIE 1976):                 {covUV.AverageValueOfX:F5} ± {covUV.StandardDeviationOfX:F5}");
                        LogAndDisplay($"v' (CIE 1976):                 {covUV.AverageValueOfY:F5} ± {covUV.StandardDeviationOfY:F5}");
                        LogAndDisplay($"r_u'v' (CIE 1976):             {covUV.CorrelationCoefficient:F3}");
                        LogAndDisplay($"Peak:                          {stpPeak.AverageValue:F2} ± {stpPeak.StandardDeviation:F2} nm");
                        LogAndDisplay($"Centre:                        {stpCen.AverageValue:F2} ± {stpCen.StandardDeviation:F2} nm");
                        LogAndDisplay($"Centroid:                      {stpCog.AverageValue:F2} ± {stpCog.StandardDeviation:F2} nm");
                        LogAndDisplay($"FWHM:                          {stpFwhm.AverageValue:F2} ± {stpFwhm.StandardDeviation:F2} nm");
                        LogAndDisplay($"Integration time:              {stpIntTime.AverageValue} s");
                        LogAndDisplay($"Internal temperature:          {stpT.AverageValue:F1} °C");
                        LogOnly(thinSeparator);
                        DisplayOnly("");
                        break;
                }
            }

            DisplayOnly("bye.");
            LogOnly("");
            LogOnly(fatSeparator);
            if (measurementIndex == 1)
                LogOnly($"{measurementIndex} measurement logged - StopTimeUTC: {timeStamp:dd-MM-yyyy HH:mm}");
            else
                LogOnly($"{measurementIndex} measurements logged - StopTimeUTC: {timeStamp:dd-MM-yyyy HH:mm}");
            LogOnly(fatSeparator);
            LogOnly("");

            streamWriter.Close();


            /***************************************************/
            void SetDynamicDarkMode()
            {
                if (device.HasShutter)
                {
                    device.ActivateDynamicDarkMode();
                    DisplayOnly("Dynamic dark mode activated.");
                }
                else
                {
                    device.DeactivateDynamicDarkMode();
                    DisplayOnly("Dynamic dark mode deactivated.");
                }
            }
            /***************************************************/
            void MeasureDarkOffset()
            {
                if (device.HasShutter)
                {
                    DisplayOnly("measure dark offset ...");
                    device.MeasureDark();
                }
                else
                {
                    // manual close shutter (only for MSC15)
                    DisplayOnly("close shutter and press enter");
                    Console.ReadLine();
                    device.MeasureDark();
                    DisplayOnly("open Shutter and press enter");
                    Console.ReadLine();
                }
            }
            /***************************************************/
            void LogAndDisplay(string line)
            {
                DisplayOnly(line);
                LogOnly(line);
            }
            /***************************************************/
            void LogOnly(string line)
            {
                streamWriter.WriteLine(line);
                streamWriter.Flush();
            }
            /***************************************************/
            void DisplayOnly(string line)
            {
                Console.WriteLine(line);
            }
            /***************************************************/
            void UpdateValues()
            {
                stpE.Update(device.PhotopicValue);
                stpS.Update(device.ScotopicValue);
                stpCct.Update(device.CctValue);
                stpT.Update(device.InternalTemperature);
                stpPeak.Update(device.PeakWL);
                stpCen.Update(device.CentreWL);
                stpCog.Update(device.CentroidWL);
                stpFwhm.Update(device.Fwhm);
                covXY.Update(device.ColorValues.x, device.ColorValues.y);
                covUV.Update(device.ColorValues.uPrime, device.ColorValues.vPrime);
                rawSpectrum.Update(device.GetNativeSpectrum());
                visSpectrum.Update(device.GetVisSpectrum());
                stpIntTime.Update(device.GetLastIntegrationTime());
            }
            /***************************************************/
            void RestartValues()
            {
                stpE.Restart();
                stpCct.Restart();
                stpT.Restart();
                stpPeak.Restart();
                stpCen.Restart();
                stpCog.Restart();
                stpFwhm.Restart();
                rawSpectrum.Restart();
                visSpectrum.Restart();
                stpIntTime.Restart();
                stpS.Restart();
                covXY.Restart();
                covUV.Restart();

            }
            /***************************************************/
            void SaveRawSpectrum(string csvFilename)
            {
                using (StreamWriter sw = new StreamWriter(csvFilename, false))
                {
                    string csvHeader = $"pixel , wavelength , average irradiance , minimum , maximum, standard deviation";
                    sw.WriteLine(csvHeader);
                    for (int i = 0; i < rawSpectrum.Spectrum.Length; i++)
                    {
                        var point = rawSpectrum.Spectrum[i];
                        string csvLine = $"{i,3} , {point.Wavelength} , {point.AverageValue} , {point.MinimumValue} , {point.MaximumValue} , {point.StandardDeviation}";
                        sw.WriteLine(csvLine);
                    }
                }
            }
            /***************************************************/
            void SaveVisSpectrum(string csvFilename)
            {
                using (StreamWriter sw = new StreamWriter(csvFilename, false))
                {
                    string csvHeader = $"wavelength , average irradiance , minimum , maximum, standard deviation";
                    sw.WriteLine(csvHeader);
                    for (int i = 0; i < visSpectrum.Spectrum.Length; i++)
                    {
                        var point = visSpectrum.Spectrum[i];
                        string csvLine = $"{point.Wavelength} , {point.AverageValue} , {point.MinimumValue} , {point.MaximumValue} , {point.StandardDeviation}";
                        sw.WriteLine(csvLine);
                    }
                }
            }
            /***************************************************/

        }

        readonly static string fatSeparator = new string('=', 80);
        readonly static string thinSeparator = new string('-', 80);

    }
}
