using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using Common.Contracts;
using Common.Models;
using Common.Faults;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== Vetrogenerator Client pokrenut ===\n");
            Console.ResetColor();

            // Povezivanje na WCF servis
            string address = "net.tcp://localhost:8088/VetrogeneratorService";
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None)
            {
                MaxReceivedMessageSize = 10485760 // 10 MB
            };
            ChannelFactory<IVetrogeneratorService> factory =
                new ChannelFactory<IVetrogeneratorService>(binding, new EndpointAddress(address));
            IVetrogeneratorService proxy = factory.CreateChannel();

            // Lokacija CSV fajlova
            string dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(dataDir))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ERROR] Folder sa podacima nije pronađen: " + dataDir);
                Console.ResetColor();
                PauseAndExit();
                return;
            }

            // Učitaj sve CSV fajlove
            string[] csvFiles = Directory.GetFiles(dataDir, "*.csv");
            if (csvFiles.Length == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ERROR] Nema CSV fajlova u folderu Data.");
                Console.ResetColor();
                PauseAndExit();
                return;
            }

            
            var firstSix = csvFiles.Take(Math.Min(csvFiles.Length, 6)).ToList();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Dostupni CSV fajlovi:");
            for (int i = 0; i < firstSix.Count; i++)
            {
                Console.WriteLine((i + 1) + ". " + Path.GetFileName(firstSix[i]));
            }
            Console.ResetColor();


            Console.Write("\nKoji fajl želite da koristite (1–6)? ");
            string input = Console.ReadLine();
            if (input == null) input = string.Empty;

            int index;
            if (!int.TryParse(input, out index) || index < 1 || index > firstSix.Count)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ Neispravan izbor. Program se prekida.");
                Console.ResetColor();
                PauseAndExit();
                return;
            }

            string selectedFile = firstSix[index - 1];
            string turbineId = Path.GetFileNameWithoutExtension(selectedFile);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n--- Izabran fajl: " + Path.GetFileName(selectedFile) + " ---");
            Console.ResetColor();

            try
            {
                proxy.StartSession(new SessionMeta
                {
                    TurbineId = turbineId,
                    Operator = Environment.UserName,
                    Description = "Ručni izbor fajla za prenos podataka"
                });

                int rowIndex = 0;
                int sentCount = 0;

                using (var reader = new StreamReader(selectedFile))
                using (var log = new StreamWriter("client_log.txt", true))
                {
                    //  Zaglavlje (nazivi kolona) je u 10. redu, a podaci počinju od 11. reda. 
                    for (int i = 0; i < 10 && !reader.EndOfStream; i++)
                        reader.ReadLine();

                    int limit = 250; 
                    while (!reader.EndOfStream && rowIndex < limit)
                    {
                        string line = reader.ReadLine();
                        rowIndex++;
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        string[] parts = line.Split(',');

                        try
                        {
                            var sample = new WindTurbineSample
                            {
                                Timestamp = DateTime.Parse(parts[0], CultureInfo.InvariantCulture),
                                WindSpeed = ParseNullable(parts[1]),
                                WindDirection = ParseNullable(parts[2]),
                                NacellePosition = ParseNullable(parts[3]),
                                PowerKW = ParseNullable(parts[4]),
                                PotentialPowerDefaultKW = ParseNullable(parts[5]),
                                PowerFactor = ParseNullable(parts[6]),
                                ReactivePowerKvar = ParseNullable(parts[7]),
                                GridFrequencyHz = ParseNullable(parts[8]),
                                GeneratorRpm = ParseNullable(parts[9]),
                                RowIndex = rowIndex + 10, 
                                TurbineId = turbineId,
                                RawLine = line 

                            };

                            proxy.PushSample(sample);
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine("[CLIENT] Šaljem red " + (rowIndex + 10) + "...");
                            Console.ResetColor();
                            sentCount++;
                        }
                        catch (Exception ex)
                        {
                            log.WriteLine("[" + DateTime.Now + "] RED " + (rowIndex + 10) + ": " + ex.Message);
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("[WARN] Greška pri parsiranju reda " + (rowIndex + 10) + ": " + ex.Message);
                            Console.ResetColor();
                        }
                    }

                    proxy.EndSession(turbineId);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n[CLIENT] Prenos fajla " + Path.GetFileName(selectedFile) + " završen (" + sentCount + " redova).");
                    Console.ResetColor();
                }
            }
            catch (FaultException<ValidationFault> vf)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[SERVER VALIDATION FAULT] " + vf.Detail.Message);
                Console.ResetColor();
            }
            catch (FaultException<DataFormatFault> df)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[SERVER DATA FORMAT FAULT] " + df.Detail.Message);
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[CLIENT ERROR] " + ex.Message);
                Console.ResetColor();
            }

            PauseAndExit();
        }

        static double? ParseNullable(string s)
        {
            if (string.IsNullOrWhiteSpace(s) || s.Equals("NaN", StringComparison.OrdinalIgnoreCase))
                return null;
            return double.Parse(s, CultureInfo.InvariantCulture);
        }

        static void PauseAndExit()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\nPritisni ENTER da zatvoriš klijenta...");
            Console.ResetColor();
            Console.ReadLine();
        }
    }
}
