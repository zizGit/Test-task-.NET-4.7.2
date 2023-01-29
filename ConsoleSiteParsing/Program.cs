using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;

namespace ConsoleSiteParsing
{
    class Storage 
    {
        string userAddress, hostAddress, sitemapAddres;

        void SetAddress(string address) 
        {
            if(address.StartsWith("http") && address.Last().ToString() == "/") 
            {
                userAddress = address;
            }
            else if (address.StartsWith("http") && address.Last().ToString() == "xml")
            {
                sitemapAddres = address;
            }
            else 
            {
                hostAddress = address;
            }
        }
    }

    class Program
    {
        static void Main()
        {
            string userAddress, hostAddress, sitemapAddres;

            Input(out userAddress, out hostAddress, out sitemapAddres);

            Parsing(userAddress);
        }

        static void Input(out string userAddress, out string hostAddress, out string sitemapAddres)
        {
            bool inputCheck = false;
            sitemapAddres = " ";
            hostAddress = " ";

            do
            {
                Console.WriteLine("Example URL: https://ukad-group.com/");
                Console.Write("Input URL: ");
                userAddress = Console.ReadLine();

                if (userAddress.StartsWith("http://") || userAddress.StartsWith("https://"))
                {
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
        }

        static void Output(string content)
        {
            Console.WriteLine(content);
            Console.ReadKey();
        }

        static void Parsing(string address)
        {
            WebClient wc = new WebClient();
            string content = wc.DownloadString(address);

            Output(content);
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

        static void sitemapCheck() 
        {

        }
    }
}