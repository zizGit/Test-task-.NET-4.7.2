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
        private static string userAddress, hostAddress, sitemapAddress;
        private List<string> addressesFromCode = new List<string>();
        private List<string> addressesFromSitemap = new List<string>();

        public void SetAddress(string address) 
        {
            if(address.StartsWith("http") && address.Last().Equals("xml")) 
            {
                sitemapAddress = address;
            }
            else if (address.StartsWith("http"))
            {
                userAddress = address;
            }
            else 
            {
                hostAddress = address;
            }
        }

        public void SetNewAddressToList(string newAddress) 
        {
            addressesFromCode.Add(newAddress);
        }

        public string GetAddressUser() 
        {
            return userAddress;
        }
        public string GetAddressHost()
        {
            return hostAddress;
        }
        public string GetAddressSitemap()
        {
            return sitemapAddress;
        }

        public string GetAddressFromList(int index) 
        {
            return addressesFromCode.ElementAt(index);
        }
    }

    class Program
    {
        static void Main()
        {
            Storage storage = new Storage();

            Input(storage);
            SitemapCheck(storage);

            Console.ReadKey();
            //Parsing(storage.GetAddressUser());
        }

        static void Input(Storage storage)
        {
            string tempAddress; //user input address
            bool inputCheck = false;
            
            do
            {
                Console.WriteLine("Example URL: https://ukad-group.com/"); //for debug
                Console.Write("Input URL: ");
                tempAddress = Console.ReadLine();

                if (tempAddress.StartsWith("http://") || tempAddress.StartsWith("https://"))
                {
                    storage.SetAddress(tempAddress);
                    UrlToHost(storage);
                    inputCheck = Ping(storage.GetAddressHost());

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
            string siteCode = wc.DownloadString(address);
        }

        static void Parsing(string address, out string toSave)
        {
            WebClient wc = new WebClient();
            toSave = wc.DownloadString(address);
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

        //https://ukad-group.com/ --> www.ukad-group.com
        static void UrlToHost(Storage storage)
        {
            string tempUserAddress, tempHostAddress;

            tempUserAddress = storage.GetAddressUser();

            if (tempUserAddress.StartsWith("http://"))
            {
                tempHostAddress = tempUserAddress.Replace("http://", "www.");
            }
            else
            {
                tempHostAddress = tempUserAddress.Replace("https://", "www.");
            }

            while (true) 
            {
                if (tempHostAddress.Last().ToString().Equals("/"))
                {
                    tempHostAddress = tempHostAddress.Remove(tempHostAddress.Length - 1);
                }
                else
                {
                    storage.SetAddress(tempHostAddress);
                    break;
                }
            }
        }

        //https://ukad-group.com/ --> https://ukad-group.com/sitemap.xml
        //get urls from sitemap
        static void SitemapCheck(Storage storage) 
        {
            string tempUserAddress, sitemap;

            tempUserAddress = storage.GetAddressUser();

            if (tempUserAddress.Last().Equals("/")) 
            {
                storage.SetAddress(tempUserAddress.Insert(tempUserAddress.Length, "sitemap.xml"));
            }

            Console.Write("\nSitemap.xml - ");
            if (Ping(storage.GetAddressHost())) 
            {
                Console.WriteLine("online");
                Parsing(storage.GetAddressSitemap(), out sitemap);

                if (sitemap.Contains("<a href =")) 
                {
                    Console.WriteLine("<a href = - founded");
                }

                
            }
            else
            {
                Console.WriteLine("not found");
            }
        }
    }
}