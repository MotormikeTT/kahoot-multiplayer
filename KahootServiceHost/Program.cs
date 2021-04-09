/*
 * Program:         KahootServiceHost.exe
 * Module:          Program.cs
 * Author:          George Moussa, Michael Mac Lean
 * Date:            April 4, 2021
 * Description:     Implements a WCF service host for the Game service in 
 *                  KahootLibrary.dll.
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
