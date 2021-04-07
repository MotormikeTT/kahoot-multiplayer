/*
 * Program:         CardsServiceHost.exe
 * Module:          Program.cs
 * Author:          T. Haworth
 * Date:            March 9, 2021
 * Description:     Implements a WCF service host for the Shoe service in 
 *                  CardsLibrary.dll.
 *                  
 *                  Note that we had to add a reference to the .NET Framework 
 *                  assembly System.ServiceModel.dll.
 *                  
 * Modifications:   Mar 16, 2021
 *                  Now uses an administrative endpoint which is configured
 *                  in the project's App.config file.
 */

using System;
using System.ServiceModel;  // WCF types
using KahootLibrary; // Shoe and IShoe types

namespace KahootServiceHost
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost servHost = null;

            try
            {
                // Register the service Address
                //servHost = new ServiceHost(typeof(Shoe), new Uri("net.tcp://localhost:13200/CardsLibrary/"));

                // Register the service Contract and Binding
                //servHost.AddServiceEndpoint(typeof(IShoe), new NetTcpBinding(), "ShoeService");

                servHost = new ServiceHost(typeof(Game));


                // Run the service
                servHost.Open();

                Console.WriteLine("Service started. Press any key to quit.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                // Wait for a keystroke
                Console.ReadKey();
                servHost?.Close();
            }
        }
    }
}
