using System;
using System.Globalization;
using System.IO;
using System.ServiceModel;
using System.Configuration;
using Common.Contracts;
using Common.Faults;
using Common.Models;

namespace Server
{
    public class VetrogeneratorService : IVetrogeneratorService
    {
        private static readonly object _lock = new object();
        // Per-turbine analytics state
        private static readonly System.Collections.Generic.Dictionary<string, (double Mean, long Count)> _rpmStats
            = new System.Collections.Generic.Dictionary<string, (double Mean, long Count)>();

        private static readonly System.Collections.Generic.Dictionary<string, double?> _lastReactive
            = new System.Collections.Generic.Dictionary<string, double?>();
        private static string BaseDataPath
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data"); }
        }

       

        private string _currentSessionDir = "";

        public void StartSession(SessionMeta meta)
        {
            if (meta == null || string.IsNullOrWhiteSpace(meta.TurbineId))
                throw new FaultException<ValidationFault>(new ValidationFault("TurbineId missing", -1));

            

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[SERVER] StartSession → Turbine=" + meta.TurbineId + ", Source=" + meta.SourceFileName + ", UTC=" + meta.StartedAtUtc);
            Console.ResetColor();

           
        }

        public void PushSample(WindTurbineSample sample)
        {
            if (sample == null)
                throw new FaultException<ValidationFault>(new ValidationFault("Sample is null", -1));

            
            

            if (sample.RowIndex < 11)
                throw new FaultException<DataFormatFault>(
                    new DataFormatFault("RowIndex must be >= 11", sample.RowIndex, ""));

           
                
                if (!sample.PowerKW.HasValue || !sample.PotentialPowerDefaultKW.HasValue || !sample.GridFrequencyHz.HasValue)
                {
                    string rawData = "(no raw data)";
                    if (!string.IsNullOrWhiteSpace(sample.RawLine))
                        rawData = sample.RawLine.Replace(",", ";");

                    string rejectLine = sample.RowIndex + ",Missing key fields," + rawData;
                    

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("[REJECT] Row=" + sample.RowIndex + " Missing key fields");
                    Console.ResetColor();
                    return;
                }

                
               
        }

        public void EndSession(string turbineId)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[SERVER] EndSession → Turbine=" + turbineId);
            Console.ResetColor();

            
        }

        
        private static void AppendLine(string path, string line)
        {
            lock (_lock)
            {
                using (var fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read))
                using (var sw = new StreamWriter(fs))
                {
                    sw.WriteLine(line);
                }
            }
        }

        private static string D(double? x)
        {
            return x.HasValue ? x.Value.ToString("0.00", CultureInfo.InvariantCulture) : "";
        }

        private static string Escape(string s)
        {
            return string.IsNullOrEmpty(s) ? "" : s.Replace(",", "_");
        }

        private static double GetDoubleAppSetting(string key, double def)
        {
            try
            {
                string val = ConfigurationManager.AppSettings[key];
                if (string.IsNullOrWhiteSpace(val)) return def;
                return double.Parse(val, CultureInfo.InvariantCulture);
            }
            catch
            {
                return def;
            }
        }
    }
}
