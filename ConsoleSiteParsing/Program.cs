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
                CheckUserAddress();
            }
            else 
            {
                hostAddress = address;
            }
        }

        public void SetNewAddressToCodeList(string newAddress) 
        {
            if (CheckAddressFromCode(newAddress)) 
            {
                addressesFromCode.Add(newAddress);
            }
        }
        public void SetNewAddressToSitemapList(string newAddress)
        {
            addressesFromSitemap.Add(newAddress);
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
        public int GetCountOfCodeAddresses()
        {
            return addressesFromCode.Count();
        }
        public int GetCountOfSitemapAddresses()
        {
            return addressesFromSitemap.Count();
        }
        // getters //

        // func //
        private void CheckUserAddress() 
        {
            if (!userAddress.Last().ToString().Equals("/"))
            {
                userAddress = userAddress.Insert(userAddress.Length, "/");
            }
        }

        private bool CheckAddressFromCode(string newCodeAddress)
        {
            if (!addressesFromCode.Contains(newCodeAddress))
            {
                return true;
            }

            return false;
        }
        // func //
    }

    class Program
    {
        static void Main()
        {
            Storage storage = new Storage();

            Input(storage);

            SitemapCheck(storage);
            SourceCodeCheck(storage);

            Output(storage);
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

        static void Output(Storage storage)
        {
            Console.Write($"User input the next link - {storage.GetAddressUser()}\n");
            Console.Write($"Host link - {storage.GetAddressHost()}\n");
            Console.Write($"Sitemap link - {storage.GetAddressSitemap()}\n");
            Console.WriteLine();
            Console.Write($"Count of links founded in sitemap.xml - {storage.GetCountOfSitemapAddresses()}\n");
            Console.Write($"Count of links founded in site code - {storage.GetCountOfCodeAddresses()}\n");
            Console.WriteLine();

            int counter = 0;
            do
            {
                Console.WriteLine($"Links from sitemap.xml - {storage.GetAddressFromSitemapList(counter)}");
                counter++;
            } while (storage.GetCountOfSitemapAddresses() != counter);

            counter = 0;
            do
            {
                Console.WriteLine($"Links from site - {storage.GetAddressFromCodeList(counter)}");
                counter++;
            } while (storage.GetCountOfCodeAddresses() != counter);

            Console.ReadKey();
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

        // https://ukad-group.com/ --> www.ukad-group.com
        static void UrlToHost(Storage storage)
        {
            string tempUserAddress, tempHostAddress;
            tempUserAddress = storage.GetAddressUser();

            // https://ukad-group.com/ --> www.ukad-group.com/
            if (tempUserAddress.StartsWith("http://"))
            {
                tempHostAddress = tempUserAddress.Replace("http://", "www.");
            }
            else
            {
                tempHostAddress = tempUserAddress.Replace("https://", "www.");
            }

            // www.ukad-group.com//////// --> www.ukad-group.com
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

        // https://ukad-group.com/ --> https://ukad-group.com/sitemap.xml
        // get urls from sitemap
        static void SitemapCheck(Storage storage) 
        {
            string tempUserAddressToSitemapAddress, sitemap;

            tempUserAddressToSitemapAddress = storage.GetAddressUser();
            storage.SetAddress(tempUserAddressToSitemapAddress.Insert(tempUserAddressToSitemapAddress.Length, "sitemap.xml"));

            Console.Write("\nSitemap.xml - ");
            if (Ping(storage.GetAddressHost()))
            {
                Console.WriteLine("online\n");
                
                Parsing(storage.GetAddressSitemap(), out sitemap);

                //начиная с исходной ссылки и заканчивая знаком <

                int firstCharIndexOfAddress, lastCharIndexOfAddress;
                int startSearchIndex = 0;

                do
                {
                    firstCharIndexOfAddress = sitemap.IndexOf(storage.GetAddressUser(), startSearchIndex);

                    if (firstCharIndexOfAddress != -1)
                    {
                        lastCharIndexOfAddress = sitemap.IndexOf("<", firstCharIndexOfAddress);

                        if (lastCharIndexOfAddress != -1)
                        {
                            storage.SetNewAddressToSitemapList(sitemap.Substring(firstCharIndexOfAddress,
                            lastCharIndexOfAddress - firstCharIndexOfAddress));

                            startSearchIndex = lastCharIndexOfAddress;
                        }
                    }
                } while (firstCharIndexOfAddress != -1);
            }
            else
            {
                Console.WriteLine("not found\n");
            }
        }

        static void SourceCodeCheck(Storage storage)
        {
            string siteCode, tempNewAddress;
            string startOfLink = "<a href=\"/"; //<a href="*link*"
            string endOfLink = "\"";
            int firstCharIndexOfAddress, lastCharIndexOfAddress, startSearchIndex = 0;

            Parsing(storage.GetAddressUser(), out siteCode);

            do
            {
                firstCharIndexOfAddress = siteCode.IndexOf(startOfLink, startSearchIndex);
                
                if (firstCharIndexOfAddress != -1)
                {
                    firstCharIndexOfAddress += 9; //skip <a href="
                    lastCharIndexOfAddress = siteCode.IndexOf(endOfLink, firstCharIndexOfAddress);

                    if (lastCharIndexOfAddress != -1)
                    {
                        tempNewAddress = siteCode.Substring(firstCharIndexOfAddress,
                            lastCharIndexOfAddress - firstCharIndexOfAddress);

                        if (tempNewAddress != "<a href=\"") //skip empty links
                        {
                            tempNewAddress = tempNewAddress.Remove(0, 1); //delete "/"
                            tempNewAddress = tempNewAddress.Insert(0, storage.GetAddressUser());

                            storage.SetNewAddressToCodeList(tempNewAddress);
                        }
                        
                        startSearchIndex = lastCharIndexOfAddress;
                    }
                }
            } while (firstCharIndexOfAddress != -1);
        }
    }
}