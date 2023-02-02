using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Http;
using System.Web;
using System.Diagnostics;
using System.IO;

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
        public string GetAddressFromSitemapList(int index)
        {
            return addressesFromSitemap.ElementAt(index);
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

    //https://learn.microsoft.com/ru-ru/dotnet/api/system.collections.generic.list-1.sort?view=net-7.0
    public class Part : IEquatable<Part>, IComparable<Part>
    {
        public string PartName { get; set; }

        public long PartId { get; set; }

        public override string ToString()
        {
            return "Ping: " + PartId + " ms" + "   Address: " + PartName;
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

    class Program
    {
        static void Main()
        {
            Storage storage = new Storage();

            Input(storage);

            Console.Clear();
            Console.WriteLine("\nProgram crawls the site, please wait\n");

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
                Console.WriteLine("Program has been tested on the next URLs: ");
                Console.WriteLine("https://ukad-group.com/ (~30 sec)");
                Console.WriteLine("https://dou.ua/ (>5 min, test not completed)");
                Console.WriteLine("https://metanit.com/ (error 403, access denied)");
                Console.Write("\nInput URL: ");
                tempAddress = Console.ReadLine();

                if (tempAddress.StartsWith("http://") || tempAddress.StartsWith("https://"))
                {
                    storage.SetAddress(tempAddress);

                    var hostAddress = new Uri(tempAddress);
                    storage.SetAddress(hostAddress.Host);

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

                    bool test = true;

                    for (int j = 0; j < storage.GetCountOfSitemapAddresses(); j++) 
                    {
                        if (storage.GetAddressFromSitemapList(j).Contains(tempLink))
                        {
                            for (int k = 0; k < storage.GetCountOfCodeAddresses(); k++) 
                            {
                                if (storage.GetAddressFromCodeList(k).Contains(tempLink)) 
                                {
                                    test = false;
                                    break;
                                }
                            }
                            break;
                        }
                    }

                    if (test) 
                    {
                        Console.WriteLine(tempLink);
                    }

                    index++;
                } while (index != storage.GetCountOfAllAddresses());
            }

            index = 0;
            Console.WriteLine("\nUrls FOUNDED BY CRAWLING THE WEBSITE but not in sitemap.xml");
            if (storage.GetCountOfAllAddresses() > 0) 
            {
                do
                {
                    int test = 0;
                    tempLink = storage.GetAllAddressesFromList(index);

                    for (int j = 0; j < storage.GetCountOfCodeAddresses(); j++)
                    {
                        if (storage.GetAddressFromCodeList(j).Contains(tempLink))
                        {
                            for (int k = 0; k < storage.GetCountOfSitemapAddresses(); k++)
                            {
                                if (!storage.GetAddressFromSitemapList(k).Contains(tempLink))
                                {
                                    test++;
                                    break;
                                }
                            }
                            break;
                        }
                    }

                    if (test + 1 == storage.GetCountOfSitemapAddresses()) 
                    {
                        Console.WriteLine(tempLink);
                    }

                    index++;
                } while (index != storage.GetCountOfAllAddresses());
            }

            Console.WriteLine("\nTiming");
            index = 0;
            do
            {
                PingLinks(storage.GetAllAddressesFromList(index), parts);
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

        static void OutputDebugMode(Storage storage) 
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

        static void Parsing(string address, out string toSave)
        {
            WebClient wc = new WebClient();
            wc.Credentials = CredentialCache.DefaultNetworkCredentials; //new

            try 
            {
                toSave = wc.DownloadString(address);
            }
            catch (WebException exception)
            {
                Console.WriteLine($"    Exception: {address}");
                Console.WriteLine($"    {exception.Message}\n");
                toSave = "";
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

        static bool PingLinks(string address, List<Part> parts)
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
                        return true;
                    }
                }
            }
            catch (WebException e)
            {
                watch.Stop();
                //Console.WriteLine($"{address} - {e.Message} - {watch.ElapsedMilliseconds / 10} ms");
                return false;
            }
        }

        // https://ukad-group.com/ --> https://ukad-group.com/sitemap.xml
        // get urls from sitemap
        static void SitemapCheck(Storage storage)
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

        static void SourceCodeCheck(Storage storage)
        {
            string siteCode;
            string[] splitCode;
            int index = 0;

            Parsing(storage.GetAddressUser(), out siteCode);
            storage.SetNewCheckedAddressToList(storage.GetAddressUser());

            /*обьявляем буфер в котором будем хранить собранную ссылку*/
            string buf;

            if(siteCode != "") 
            {
                do
                {
                    splitCode = siteCode.Split('\n'); //разбираем весь текст на строки

                    /*начинаем перебирать строки*/
                    foreach (string str in splitCode)
                    {
                        /*начинаем двигаться по строке с шагом 1 и с -6 символа с конца*/
                        for (int I = 0; I < str.Length - 5; I++)
                        {
                            /*читаем сразу 4 символа и смотрим что это*/
                            if (str.Substring(I, 5) == "href=") // \"
                            {

                                /*ссылка найдена ура будем ее читать*/
                                buf = "";

                                if (str.Substring(I, 6) == "href=\"") 
                                {
                                    /*заходим внутрь сдвигая индекс на href=" 6 символов*/
                                    I += 6;
                                }
                                else 
                                {
                                    I += 5;
                                }

                                string strTest = str.Remove(0, I);
                                //I = 0;
                                int indexTest = 0;

                                /*читаем пока не упремся в двойную кавычку или конец строки*/
                                while (strTest.Length != indexTest && strTest.Substring(indexTest, 1) != " " && strTest.Substring(indexTest, 1) != @"""")
                                {
                                    /*собираем в буфер ссылку*/
                                    buf += strTest.Substring(indexTest, 1);

                                    /*берем следующий символ*/
                                    indexTest++;
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
                                        /*добавляем данные в список*/
                                        storage.SetNewAddressToCodeList(buf);
                                    }
                                }
                            }
                        }
                    }

                    if (!storage.GetCheckAddressFromList().Contains(storage.GetAddressFromCodeList(index)))
                    {
                        Parsing(storage.GetAddressFromCodeList(index), out siteCode);
                        storage.SetNewCheckedAddressToList(storage.GetAddressFromCodeList(index));
                    }
                    index++;
                } while (storage.GetCountOfCodeAddresses() != storage.GetCountOfCheckedAddresses());
            }

            //bug - program dont see the link if link started with href=
            /*
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

                            if (Array.IndexOf(ignoreLinksWith, tempNewAddress) == -1) //if всё пропускает, переписать
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
            */
        }
    }
}