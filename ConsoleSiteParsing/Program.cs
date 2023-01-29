using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;

namespace ConsoleSiteParsing
{
    class Program
    {
        static void Main()
        {
            string address;

            address = Input();

            WebClient wc = new WebClient();
            string content = wc.DownloadString(address);

            Output(content);
        }

        static string Input()
        {
            string userAddress, hostAddress;
            bool inputCheck = false;

            do
            {
                Console.WriteLine("Example URL: https://ukad-group.com/");
                Console.Write("Input URL: ");
                userAddress = Console.ReadLine();

                if (userAddress.StartsWith("http://") || userAddress.StartsWith("https://"))
                {
                    //hostAddress = "www.ukad-group.com";

                    hostAddress = urlToHost(userAddress);

                    inputCheck = Ping(hostAddress);

                    if (!inputCheck)
                    {
                        Console.WriteLine("Site not responce.");
                    }
                }
                else
                {
                    Console.WriteLine("Input error! Please try again.");
                }

            } while (!inputCheck);

            return userAddress;
        }

        static void Output(string content)
        {
            Console.WriteLine(content);
            Console.ReadKey();
        }

        static void Parsing()
        {

        }

        static bool Ping(string hostAddress)
        {
            Ping pingSend = new Ping();
            PingReply reply = pingSend.Send(hostAddress);

            if (reply.Status.ToString() == "Success")
            {
                return true;
            }

            return false;
        }

        static string urlToHost(string address)
        {
            string hostAddress;

            if (address.StartsWith("http://"))
            {
                hostAddress = address.Replace("http://", "www.");
            }
            else
            {
                hostAddress = address.Replace("https://", "www.");
            }

            while (true) 
            {
                if (hostAddress.Last().ToString() == "/")
                {
                    hostAddress = hostAddress.Remove(hostAddress.Length - 1);
                }
                else
                {
                    return hostAddress;
                }
            }
        }
    }
}