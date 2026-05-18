using System;
using System.ServiceModel;
using Common.Contracts;

namespace Server
{
    internal class Program
    {
        static void Main()
        {
            Console.Title = "Vetrogenerator Server";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("============================================");
            Console.WriteLine("     SISTEM ZA ANALIZU VETROGENERATORA");
            Console.WriteLine("============================================");
            Console.ResetColor();

            try
            {
                // Definiši adresu servisa
                var baseAddress = new Uri("net.tcp://localhost:8088/VetrogeneratorService");

                using (var host = new ServiceHost(typeof(VetrogeneratorService), baseAddress))
                {
                    var binding = new NetTcpBinding
                    {
                        MaxReceivedMessageSize = 10485760, 
                        Security = { Mode = SecurityMode.None }
                    };

                    host.AddServiceEndpoint(typeof(IVetrogeneratorService), binding, "");

                   
                    host.Faulted += (s, e) =>
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[SERVER ERROR] Host je prešao u Faulted stanje!");
                        Console.ResetColor();
                    };

                    try
                    {
                        host.Open();
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[SERVER ERROR pri otvaranju hosta]");
                        Console.WriteLine(ex.Message);
                        if (ex.InnerException != null)
                            Console.WriteLine("Inner: " + ex.InnerException.Message);
                        Console.ResetColor();
                        return;
                    }

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($" Server pokrenut: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    Console.WriteLine($" Endpoint: {baseAddress}");
                    Console.ResetColor();
                    Console.WriteLine("--------------------------------------------------");
                    Console.WriteLine("Server čeka podatke od klijenta...\n");

                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("Pritisni ENTER za gašenje servera...");
                    Console.ResetColor();
                    Console.ReadLine();

                  
                    if (host.State == CommunicationState.Faulted)
                        host.Abort();
                    else
                        host.Close();

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("🟡 Server zaustavljen. Završetak sesije.");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ GREŠKA PRI STARTOVANJU SERVERA:");
                Console.WriteLine(ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine("Inner: " + ex.InnerException.Message);
                Console.ResetColor();
            }
        }
    }
}
