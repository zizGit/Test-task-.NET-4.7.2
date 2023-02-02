using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.IO;

namespace ConsoleSiteParsing
{
    public class Storage
    {
        private static string userAddress, hostAddress, sitemapAddress;
        private static List<string> addressesFromCode = new List<string>();
        private static List<string> addressesFromSitemap = new List<string>();
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
        public string GetAllAddressesFromList(int index)
        {
            return allAddresses.ElementAt(index);
        }
        public int GetCountOfCodeAddresses()
        {
            return addressesFromCode.Count();
        }
        public int GetCountOfSitemapAddresses()
        {
            return addressesFromSitemap.Count();
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
            allAddresses.Sort();
        }
        public void MergeLinks() 
        {
            string tempLink;
            int index = 0;

            if(addressesFromSitemap.Count > 0) 
            {
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
        }
        public void RemoveLinkFromSitemapList(int index) 
        {
            addressesFromSitemap.RemoveAt(index);
        }
        // func //
    }

    public class InputOutput 
    {
        public static void Input(Storage storage)
        {
            string tempAddress; //user input address
            bool inputCheck = false;

            do
            {
                Console.WriteLine("Program has been tested on the next URLs: ");
                Console.WriteLine("https://ukad-group.com/ (~30 sec)");
                //Console.WriteLine("https://dou.ua/ (>5 min, test not completed)");
                //Console.WriteLine("https://metanit.com/ (error 403, access denied)");
                Console.Write("\nInput URL: ");
                tempAddress = Console.ReadLine();

                if (tempAddress.StartsWith("http://") || tempAddress.StartsWith("https://"))
                {
                    storage.SetAddress(tempAddress);

                    var hostAddress = new Uri(tempAddress);
                    storage.SetAddress(hostAddress.Host);

                    inputCheck = Program.Ping(storage.GetAddressHost());
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("Input error! Please try again.\n");
                }

            } while (!inputCheck);

            Console.Clear();
            Console.WriteLine("\nProgram crawls the site, please wait\n");
        }
        public static void Output(Storage storage)
        {
            int index = 0;
            string tempLink;
            List<Part> parts = new List<Part>();

            Console.Write($"User input the next link - {storage.GetAddressUser()}\n");
            Console.Write($"Host link - {storage.GetAddressHost()}\n");
            Console.Write($"Sitemap link - {storage.GetAddressSitemap()}\n");
            Console.WriteLine();
            Console.Write($"Count of links founded in sitemap.xml - {storage.GetCountOfSitemapAddresses()}\n");
            Console.Write($"Count of links founded in site code - {storage.GetCountOfCodeAddresses()}\n");
            Console.Write($"Count of all founded links - {storage.GetCountOfAllAddresses()}\n");
            Console.WriteLine();

            Console.WriteLine("Urls FOUNDED IN SITEMAP.XML but not founded after crawling a web site:");
            if (storage.GetCountOfAllAddresses() > 0)
            {
                do
                {
                    tempLink = storage.GetAllAddressesFromList(index);
                    bool urlNotFoundAfterCrawling = true;

                    for (int j = 0; j < storage.GetCountOfSitemapAddresses(); j++)
                    {
                        if (storage.GetAddressFromSitemapList(j).Contains(tempLink))
                        {
                            for (int k = 0; k < storage.GetCountOfCodeAddresses(); k++)
                            {
                                if (storage.GetAddressFromCodeList(k).Contains(tempLink))
                                {
                                    urlNotFoundAfterCrawling = false;
                                    break;
                                }
                            }
                            break;
                        }
                    }

                    if (urlNotFoundAfterCrawling)
                    {
                        Console.WriteLine(tempLink);
                    }

                    index++;
                } while (index != storage.GetCountOfAllAddresses());
            }

            Console.WriteLine("\nUrls FOUNDED BY CRAWLING THE WEBSITE but not in sitemap.xml");
            if (storage.GetCountOfAllAddresses() > 0)
            {
                index = 0;
                do
                {
                    int urlNotFoundInSitemap = 0;
                    tempLink = storage.GetAllAddressesFromList(index);

                    for (int j = 0; j < storage.GetCountOfCodeAddresses(); j++)
                    {
                        if (storage.GetAddressFromCodeList(j).Contains(tempLink))
                        {
                            for (int k = 0; k < storage.GetCountOfSitemapAddresses(); k++)
                            {
                                if (!storage.GetAddressFromSitemapList(k).Contains(tempLink))
                                {
                                    urlNotFoundInSitemap++;
                                    break;
                                }
                            }
                            break;
                        }
                    }

                    if (urlNotFoundInSitemap + 1 == storage.GetCountOfSitemapAddresses())
                    {
                        Console.WriteLine(tempLink);
                    }

                    index++;
                } while (index != storage.GetCountOfAllAddresses());
            }

            Console.WriteLine("\nTiming, please wait");
            index = 0;
            do
            {
                Program.PingLinks(storage.GetAllAddressesFromList(index), parts);
                index++;
            } while (index != storage.GetCountOfAllAddresses());

            parts.Sort();

            foreach (Part aPart in parts)
            {
                Console.WriteLine(aPart);
            }

            //OutputDebugMode(storage);

            Console.ReadKey();
        }
        private static void OutputDebugMode(Storage storage)
        {
            int counter = 0;

            Console.WriteLine("\nAll links from sitemap.xml: ");
            if (storage.GetCountOfSitemapAddresses() > 0)
            {
                do
                {
                    Console.WriteLine($"{counter + 1}. {storage.GetAddressFromSitemapList(counter)}");
                    counter++;
                } while (storage.GetCountOfSitemapAddresses() != counter);
            }

            counter = 0;
            Console.WriteLine("\nAll links from site: ");
            if (storage.GetCountOfCodeAddresses() > 0)
            {
                do
                {
                    Console.WriteLine($"{counter + 1}. {storage.GetAddressFromCodeList(counter)}");
                    counter++;
                } while (storage.GetCountOfCodeAddresses() != counter);
            }
        }
    }

    //https://learn.microsoft.com/ru-ru/dotnet/api/system.collections.generic.list-1.sort?view=net-7.0
    public class Part : IEquatable<Part>, IComparable<Part>
    {
        public string PartName { get; set; }

        public long PartId { get; set; }

        public override string ToString()
        {
            return "Ping: " + PartId + " ms " + "    " + PartName;
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Part objAsPart = obj as Part;
            if (objAsPart == null) return false;
            else return Equals(objAsPart);
        }
        public int SortByNameAscending(string name1, string name2)
        {

            return name1.CompareTo(name2);
        }

        // Default comparer for Part type.
        public int CompareTo(Part comparePart)
        {
            // A null value means that this object is greater.
            if (comparePart == null)
                return 1;

            else
                return this.PartId.CompareTo(comparePart.PartId);
        }
        public long GetHashCode_()
        {
            return PartId;
        }
        public bool Equals(Part other)
        {
            if (other == null) return false;
            return (this.PartId.Equals(other.PartId));
        }
        // Should also override == and != operators.
    }

    public class Program
    {
        private static void Main()
        {
            Storage storage = new Storage();

            InputOutput.Input(storage);

            SitemapCheck(storage);
            SiteCodeCheck(storage);

            storage.MergeLinks();
            storage.SortLinksInAllList();

            InputOutput.Output(storage);
        }

        public static void Parsing(string address, out string toSave)
        {
            WebClient wc = new WebClient();
            wc.Credentials = CredentialCache.DefaultNetworkCredentials; //new

            try 
            {
                toSave = wc.DownloadString(address);
            }
            catch (WebException exception)
            {
                Console.WriteLine($" Exception: {address}");
                Console.WriteLine($" {exception.Message}\n");
                toSave = "";
            }
        }

        public static bool Ping(string hostAddress)
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
                    Console.WriteLine("Site or sitemap not responce.\n");
                }
            }
            catch (PingException exception)
            {
                Console.WriteLine($"    Exception: {hostAddress}");
                Console.WriteLine($"    {exception.InnerException.Message}\n");
            }

            return false;
        }

        public static void PingLinks(string address, List<Part> parts)
        {
            var request = WebRequest.Create(address);
            var watch = Stopwatch.StartNew();
            
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream answer = response.GetResponseStream())
                    {
                        watch.Stop();
                        parts.Add(new Part() { PartName = address, PartId = watch.ElapsedMilliseconds / 10 });
                        //Console.WriteLine($"{address} - Success - {watch.ElapsedMilliseconds / 10} ms");
                    }
                }
            }
            catch (WebException exception)
            {
                watch.Stop();
                parts.Add(new Part() { PartName = address, PartId = watch.ElapsedMilliseconds / 10 });
                //Console.WriteLine($"{address} - {exception.Message} - {watch.ElapsedMilliseconds / 10} ms");
            }
        }

        // https://ukad-group.com/ --> https://ukad-group.com/sitemap.xml
        // get urls from sitemap
        public static void SitemapCheck(Storage storage)
        {
            List<string> xmlLinks = new List<string>();
            string tempUserAddressToSitemapAddress, sitemap;
            int index = 0;
            int firstCharIndexOfAddress, lastCharIndexOfAddress;
            int startSearchIndex = 0;

            tempUserAddressToSitemapAddress = storage.GetAddressUser();
            storage.SetAddress(tempUserAddressToSitemapAddress.Insert(tempUserAddressToSitemapAddress.Length, "sitemap.xml"));

            if (Ping(storage.GetAddressHost()))
            {
                Parsing(storage.GetAddressSitemap(), out sitemap);

                if (sitemap != "")
                {
                    do
                    {
                        if (xmlLinks.Any())
                        {
                            Parsing(xmlLinks.ElementAt(0), out sitemap);
                            xmlLinks.RemoveAt(0);
                        }

                        //начиная с исходной ссылки и заканчивая знаком <

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

                        while (index != storage.GetCountOfSitemapAddresses())
                        {
                            if (storage.GetAddressFromSitemapList(index).EndsWith(".xml"))
                            {
                                xmlLinks.Add(storage.GetAddressFromSitemapList(index));
                                storage.RemoveLinkFromSitemapList(index);
                            }
                            else
                            {
                                index++;
                            }
                        }
                        startSearchIndex = 0;

                    } while (index == 0);
                }
                else
                {
                    Console.WriteLine("Sitemap.xml not found (Parsing)\n");
                }
            }
            else
            {
                Console.WriteLine("Sitemap.xml not found (Ping)\n");
            } 
        }

        public static void SiteCodeCheck(Storage storage)
        {
            string siteCode, buf;
            string[] splitCode;
            int index = 0;
            List<string> checkedAddressesFromCode = new List<string>();

            Parsing(storage.GetAddressUser(), out siteCode);
            checkedAddressesFromCode.Add(storage.GetAddressUser());

            if (siteCode != "") 
            {
                do
                {
                    splitCode = siteCode.Split('\n'); //разбираем весь текст на строки

                    /*начинаем перебирать строки*/
                    foreach (string str in splitCode)
                    {
                        /*начинаем двигаться по строке с шагом 1 и с -5 символа с конца*/
                        for (int I = 0; I < str.Length - 5; I++)
                        {
                            /*читаем сразу 5 символа и смотрим что это*/
                            if (str.Substring(I, 5) == "href=")
                            {
                                //link found
                                buf = "";

                                /*заходим внутрь сдвигая индекс на href=" символов*/
                                if (str.Substring(I, 6) == "href=\"") 
                                {
                                    I += 6;
                                }
                                else 
                                {
                                    I += 5;
                                }

                                string strTest = str.Remove(0, I);
                                int indexTest = 0;

                                /*читаем пока не упремся в двойную кавычку или конец строки*/
                                while (strTest.Length != indexTest && strTest.Substring(indexTest, 1) != " " && strTest.Substring(indexTest, 1) != @"""")
                                {
                                    buf += strTest.Substring(indexTest, 1); // собираем в буфер ссылку
                                    indexTest++; // берем следующий символ
                                }

                                if (!buf.StartsWith("//") && !buf.Contains("/fonts") && !buf.StartsWith("/images") &&
                                    !buf.Contains("/media/") && !buf.Contains("png") && !buf.Contains("pdf") && 
                                    !buf.Contains("json") && !buf.Contains("ico") && !buf.Contains("#") && !buf.Contains("css") && 
                                    buf.Any())
                                {
                                    if (buf.StartsWith("/"))
                                    {
                                        buf = buf.Remove(0, 1); //delete "/"
                                        buf = buf.Insert(0, storage.GetAddressUser());
                                    }
                                    if (buf.EndsWith("\"")) 
                                    {
                                        buf = buf.Remove(buf.Length - 1, 1);
                                    }
                                    if (buf.StartsWith(storage.GetAddressUser()))
                                    {
                                        storage.SetNewAddressToCodeList(buf); //save link
                                    }
                                }
                            }
                        }
                    }

                    if (!checkedAddressesFromCode.AsReadOnly().ToString().Contains(storage.GetAddressFromCodeList(index)))
                    {
                        Parsing(storage.GetAddressFromCodeList(index), out siteCode);
                        checkedAddressesFromCode.Add(storage.GetAddressFromCodeList(index));
                    }
                    index++;
                } while (storage.GetCountOfCodeAddresses() != checkedAddressesFromCode.Count());
            }
        }
    }
}