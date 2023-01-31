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
        private static List<string> checkedAddressesFromCode = new List<string>();
        private static List<string> allAddresses = new List<string>();

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
                if (!userAddress.Last().ToString().Equals("/"))
                {
                    userAddress = userAddress.Insert(userAddress.Length, "/");
                }
            }
            else 
            {
                hostAddress = address;
            }
        }

        public void SetNewAddressToCodeList(string newAddress) 
        {
            if (!newAddress.Last().ToString().Equals("/"))
            {
                newAddress = newAddress.Insert(newAddress.Length, "/");
            }
            if (!addressesFromCode.Contains(newAddress)) 
            {
                addressesFromCode.Add(newAddress);
            }
        }
        public void SetNewAddressToSitemapList(string newAddress)
        {
            addressesFromSitemap.Add(newAddress);
        }
        public void SetNewCheckedAddressToList(string newAddress)
        {
            checkedAddressesFromCode.Add(newAddress);
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
        public string GetAddressFromCodeList()
        {
            return addressesFromCode.AsReadOnly().ToString();
        }
        public string GetAddressFromSitemapList(int index)
        {
            return addressesFromSitemap.ElementAt(index);
        }
        public string GetAddressFromSitemapList()
        {
            return addressesFromSitemap.AsReadOnly().ToString();
        }
        public string GetCheckAddressFromList(int index)
        {
            return checkedAddressesFromCode.ElementAt(index);
        }
        public string GetAllAddressesFromList(int index)
        {
            return allAddresses.ElementAt(index);
        }
        public string GetCheckAddressFromList()
        {
            return checkedAddressesFromCode.AsReadOnly().ToString();
        }
        public int GetCountOfCodeAddresses()
        {
            return addressesFromCode.Count();
        }
        public int GetCountOfSitemapAddresses()
        {
            return addressesFromSitemap.Count();
        }
        public int GetCountOfCheckedAddresses()
        {
            return checkedAddressesFromCode.Count();
        }
        public int GetCountOfAllAddresses()
        {
            return allAddresses.Count();
        }
        // getters //

        // func //
        public void SortLinksInAllList() 
        {
            addressesFromCode.Sort();
            addressesFromSitemap.Sort();
            checkedAddressesFromCode.Sort();
            allAddresses.Sort();
        }
        public void MergeLinks() 
        {
            string tempLink;
            int index = 0;

            allAddresses = addressesFromSitemap;

            do
            {
                tempLink = addressesFromCode.ElementAt(index);

                if (!allAddresses.Contains(tempLink))
                {
                    allAddresses.Add(tempLink);
                }

                index++;
            } while (index != addressesFromCode.Count());
        }
        // func //
    }

    class Program
    {
        static void Main()
        {
            Storage storage = new Storage();

            Input(storage);

            Console.WriteLine("Program crawls the site, please wait");

            SitemapCheck(storage);
            SourceCodeCheck(storage);

            storage.MergeLinks();
            storage.SortLinksInAllList();

            Output(storage);
        }

        static void Input(Storage storage)
        {
            string tempAddress; //user input address
            bool inputCheck = false;
            
            do
            {
                Console.WriteLine("Example URL: https://ukad-group.com/");
                Console.Write("Input URL: ");
                tempAddress = Console.ReadLine();

                if (tempAddress.StartsWith("http://") || tempAddress.StartsWith("https://"))
                {
                    storage.SetAddress(tempAddress);
                    UrlToHost(storage);
                    inputCheck = Ping(storage.GetAddressHost());
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("Input error! Please try again.\n");
                }

            } while (!inputCheck);
        }

        static void Output(Storage storage)
        {
            int index = 0;
            string tempLink;

            Console.Clear();

            Console.Write($"User input the next link - {storage.GetAddressUser()}\n");
            Console.Write($"Host link - {storage.GetAddressHost()}\n");
            Console.Write($"Sitemap link - {storage.GetAddressSitemap()}\n");
            Console.WriteLine();
            Console.Write($"Count of links founded in sitemap.xml - {storage.GetCountOfSitemapAddresses()}\n");
            Console.Write($"Count of links founded in site code - {storage.GetCountOfCodeAddresses()}\n");
            Console.Write($"Count of all founded links - {storage.GetCountOfAllAddresses()}\n");
            Console.WriteLine();

            Console.WriteLine("Urls FOUNDED IN SITEMAP.XML but not founded after crawling a web site:");
            do
            {
                if (index == storage.GetCountOfCodeAddresses())
                {
                    do
                    {
                        Console.WriteLine(storage.GetAddressFromSitemapList(index));
                        index++;
                    } while (index != storage.GetCountOfSitemapAddresses());
                    break;
                }

                tempLink = storage.GetAllAddressesFromList(index);

                if (storage.GetAddressFromSitemapList().Contains(tempLink) && !storage.GetAddressFromCodeList().Contains(tempLink))
                {
                    Console.WriteLine(tempLink);
                }

                index++;
            } while (index != storage.GetCountOfAllAddresses());

            index = 0;
            Console.WriteLine("\nUrls FOUNDED BY CRAWLING THE WEBSITE but not in sitemap.xml");
            do
            {
                tempLink = storage.GetAllAddressesFromList(index);

                if (!storage.GetAddressFromSitemapList().Contains(tempLink) && storage.GetAddressFromCodeList().Contains(tempLink))
                {
                    Console.WriteLine(tempLink);
                }

                index++;
            } while (index != storage.GetCountOfAllAddresses());

            //OutputDebugMode(storage);

            Console.ReadKey();
        }

        static void OutputDebugMode(Storage storage) 
        {
            Console.WriteLine("\nAll links from sitemap.xml: ");
            int counter = 0;
            do
            {
                Console.WriteLine($"{counter + 1}. {storage.GetAddressFromSitemapList(counter)}");
                counter++;
            } while (storage.GetCountOfSitemapAddresses() != counter);

            Console.WriteLine("\nAll links from site: ");
            counter = 0;
            do
            {
                Console.WriteLine($"{counter + 1}. {storage.GetAddressFromCodeList(counter)}");
                counter++;
            } while (storage.GetCountOfCodeAddresses() != counter);
        }

        static void Parsing(string address, out string toSave)
        {
            WebClient wc = new WebClient();
            try 
            {
                toSave = wc.DownloadString(address);
            }
            catch 
            {
                toSave = " ";
            }
        }

        static bool Ping(string hostAddress)
        {
            Ping pingSend = new Ping();

            try
            {
                PingReply reply = pingSend.Send(hostAddress);

                if (reply.Status.ToString() == "Success")
                {
                    return true;
                }
                else 
                {
                    Console.WriteLine("Site not responce.");
                }
            }
            catch (PingException exception)
            {
                Console.Clear();
                Console.WriteLine($"Exception - {exception.InnerException.Message}\n");
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
            string[] ignoreLinksWith = { "#", "?" };
            int firstCharIndexOfAddress, lastCharIndexOfAddress; 
            int startSearchIndex = 0, newLinksCounter = 0;

            Parsing(storage.GetAddressUser(), out siteCode);
            storage.SetNewCheckedAddressToList(storage.GetAddressUser());

            //bug - program dont see the link if link started with href=

            do
            {
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

                            if (Array.IndexOf(ignoreLinksWith, tempNewAddress) == -1) 
                            {
                                tempNewAddress = tempNewAddress.Remove(0, 1); //delete "/"
                                tempNewAddress = tempNewAddress.Insert(0, storage.GetAddressUser());

                                storage.SetNewAddressToCodeList(tempNewAddress);
                            }

                            startSearchIndex = lastCharIndexOfAddress;
                        }
                    }
                } while (firstCharIndexOfAddress != -1);

                startSearchIndex = 0;

                do
                { 
                    if (!storage.GetCheckAddressFromList().Contains(storage.GetAddressFromCodeList(newLinksCounter)))
                    {
                        Parsing(storage.GetAddressFromCodeList(newLinksCounter), out siteCode);
                        if(siteCode != " ") 
                        {
                            storage.SetNewCheckedAddressToList(storage.GetAddressFromCodeList(newLinksCounter));
                        }
                    }
                    newLinksCounter++;
                } while (storage.GetCheckAddressFromList().Contains(storage.GetAddressFromCodeList(newLinksCounter - 1)));
            } while (storage.GetCountOfCodeAddresses() != storage.GetCountOfCheckedAddresses());
        }
    }
}