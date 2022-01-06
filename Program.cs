﻿using System;
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

            MeasureDarkOffset();
            SetDynamicDarkMode();

            DisplayOnly("");

            int measurementIndex = 0;

            while (true)
            {
                bool shallQuit = false;
                DisplayOnly("press any key to start a measurement - 'd' to get dark offset, 'q' to quit");
                ConsoleKeyInfo cki = Console.ReadKey(true);
                switch (cki.Key)
                {
                    case ConsoleKey.Q:
                        shallQuit = true;
                        break;
                    case ConsoleKey.D:
                        MeasureDarkOffset();
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
                            DisplayOnly($"{iterationIndex,4}:  {device.CctValue:F0} K  {device.PhotopicValue:F2} lx  " +
                                $"{device.PeakWL:F1} nm  {device.CentreWL:F1} nm  {device.Fwhm:F1} nm");
                        }

                        string rawSpecFilename = $"{options.SpecFilePrefix}_RAW_{timeStamp:yyyyMMddHHmmss}.csv";
                        SaveRawSpectrum(rawSpecFilename);
                        string visSpecFilename = $"{options.SpecFilePrefix}_VIS_{timeStamp:yyyyMMddHHmmss}.csv";
                        SaveVisSpectrum(visSpecFilename);

                        DisplayOnly("");
                        LogOnly($"Measurement number:            {measurementIndex}");
                        LogOnly($"Triggered at:                  {timeStamp:dd-MM-yyyy HH:mm:ss}");
                        LogOnly($"Spectrum (raw):                {rawSpecFilename}");
                        LogOnly($"Spectrum (vis):                {visSpecFilename}");
                        LogAndDisplay($"CCT value:                     {stpCct.AverageValue:F1} ± {stpCct.StandardDeviation:F1} K");
                        LogAndDisplay($"Illuminance:                   {stpE.AverageValue:F2} ± {stpE.StandardDeviation:F2} lx");
                        LogAndDisplay($"Peak:                          {stpPeak.AverageValue:F2} ± {stpPeak.StandardDeviation:F2} nm");
                        LogAndDisplay($"Centre:                        {stpCen.AverageValue:F2} ± {stpCen.StandardDeviation:F2} nm");
                        LogAndDisplay($"Centroid:                      {stpCog.AverageValue:F2} ± {stpCog.StandardDeviation:F2} nm");
                        LogAndDisplay($"FWHM:                          {stpFwhm.AverageValue:F2} ± {stpFwhm.StandardDeviation:F2} nm");
                        LogAndDisplay($"integration time:              {stpIntTime.AverageValue:F4} s");
                        LogAndDisplay($"Internal temperature:          {stpT.AverageValue:F1} °C");
                        LogOnly(thinSeparator);
                        DisplayOnly("");
                        break;
                }
                if (shallQuit) break;
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
                stpCct.Update(device.CctValue);
                stpT.Update(device.InternalTemperature);
                stpPeak.Update(device.PeakWL);
                stpCen.Update(device.CentreWL);
                stpCog.Update(device.CentroidWL);
                stpFwhm.Update(device.Fwhm);
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
