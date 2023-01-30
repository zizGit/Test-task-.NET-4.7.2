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
        private static int countOfSitemapAddresses, countOfCodeAddresses;
        private static List<string> addressesFromCode = new List<string>();
        private static List<string> addressesFromSitemap = new List<string>();

        // setters //
        public void SetAddress(string address) 
        {
            if(address.Contains("sitemap.xml")) 
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

        public void SetNewAddressToCodeList(string newAddress) 
        {
            addressesFromCode.Add(newAddress);
        }
        public void SetNewAddressToSitemapList(string newAddress)
        {
            addressesFromSitemap.Add(newAddress);
        }
        
        public void SetCountOfSitemapAddresses(int count) 
        {
            countOfSitemapAddresses = count;
        }
        public void SetCountOfCodeAddresses(int count)
        {
            countOfCodeAddresses = count;
        }
        // setters //

        // getters //
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

        public string GetAddressFromCodeList(int index) 
        {
            return addressesFromCode.ElementAt(index);
        }
        public string GetAddressFromSitemapList(int index)
        {
            return addressesFromSitemap.ElementAt(index);
        }

        public int GetCountOfSitemapAddresses()
        {
            return countOfSitemapAddresses;
        }
        public int GetCountOfCodeAddresses()
        {
            return countOfCodeAddresses;
        }
        // getters //
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
            string tempUserAddressToSitemapAddress, sitemap;

            tempUserAddressToSitemapAddress = storage.GetAddressUser();

            if (tempUserAddressToSitemapAddress.Last().ToString().Equals("/")) 
            {
                storage.SetAddress(tempUserAddressToSitemapAddress.Insert(tempUserAddressToSitemapAddress.Length, "sitemap.xml"));

                Console.Write("\nSitemap.xml - ");
                if (Ping(storage.GetAddressHost()))
                {
                    Console.WriteLine("online");
                    Parsing(storage.GetAddressSitemap(), out sitemap);

                    if (sitemap.Contains(storage.GetAddressUser()))
                    {
                        Console.WriteLine("links - founded");

                        //начиная с исходной ссылки и заканчивая пробелом или знаком <
                        //индекс первого знака и первого пробела после него

                        int firstCharIndexOfSourceAddress, lastCharIndexOfSourceAddress;
                        int startSearchIndex = 0, linksCounter = 0;

                        do
                        {
                            firstCharIndexOfSourceAddress = sitemap.IndexOf(storage.GetAddressUser(), startSearchIndex);

                            if(firstCharIndexOfSourceAddress != -1) 
                            {
                                lastCharIndexOfSourceAddress = sitemap.IndexOf("<", firstCharIndexOfSourceAddress);

                                storage.SetNewAddressToSitemapList(sitemap.Substring(firstCharIndexOfSourceAddress, 
                                    lastCharIndexOfSourceAddress - firstCharIndexOfSourceAddress));

                                linksCounter++;
                                startSearchIndex = lastCharIndexOfSourceAddress;
                            }
                        } while (firstCharIndexOfSourceAddress != -1);

                        storage.SetCountOfSitemapAddresses(linksCounter);
                    }
                    else 
                    {
                        Console.WriteLine("links - not founded");
                    }
                }
                else
                {
                    Console.WriteLine("not found");
                }
            }
            else 
            {
                Console.WriteLine("ERROR! if (tempUserAddress.Last().ToString().Equals(''/''))");
            }
        }
    }
}