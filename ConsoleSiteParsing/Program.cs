using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

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

            allAddresses.AddRange(addressesFromSitemap);
            while (index != addressesFromCode.Count()) 
            {
                tempLink = addressesFromCode.ElementAt(index);
                if (!allAddresses.Contains(tempLink))
                {
                    allAddresses.Add(tempLink);
                }
                index++;
            }
        }
        public void RemoveLinkFromSitemapList(int index) 
        {
            addressesFromSitemap.RemoveAt(index);
        }
        public bool AnyLinksInCodeList()
        {
            if (addressesFromCode.Any()) 
            {
                return true;
            }
            return false;
        }
    }

    public class InputOutput 
    {
        public static void Input(Storage storage)
        {
            //async void
            string tempAddress, inputStrCheck; //user input address
            bool inputCheck = false; //host online or offline

            do
            {
                Console.WriteLine("Program has been tested on the next URLs: ");
                Console.WriteLine("https://ukad-group.com/ (~ 40 sec)");
                Console.WriteLine("https://faromstudio.com/ (~ 2 min 30 sec)");
                Console.WriteLine("https://www.playwing.com/ (~ 30 sec)");
                //Console.WriteLine("https://mirowin.com/ (~ 10 min)"); //https://example.com/*link*/]]> --> ]]> it is bug 
                //Console.WriteLine("https://dou.ua/ (> 5 min, test not completed)");
                Console.Write("\nInput URL: ");
                tempAddress = Console.ReadLine();

                if(!tempAddress.Contains(" ")) 
                {
                    if (tempAddress.StartsWith("http://") || tempAddress.StartsWith("https://"))
                    {
                        storage.SetAddress(tempAddress);

                        
                        var hostAddress = new Uri(tempAddress);
                        storage.SetAddress(hostAddress.Host);

                        //await Program.Ping
                        inputCheck = Program.Ping(storage.GetAddressHost());
                        
                        if (!inputCheck) 
                        {
                            Console.WriteLine("Do you want to continue? [Y or something else]");
                            inputStrCheck = Console.ReadLine();

                            if (inputStrCheck == "Y") 
                            {
                                break;
                            }

                            Console.Clear();
                        }
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("Link should start with \"http://\" or \"https://\"\n");
                    }
                }
                else 
                {
                    Console.Clear();
                    Console.WriteLine("Link should not contain \" \" (space)!\n");
                }
            } while (!inputCheck);

            Console.Clear();
            Console.WriteLine("\nProgram crawls the site, please wait\n");
        }
        public static void Output(Storage storage)
        {
            Console.Clear();
            Console.Write($"User input the next link - {storage.GetAddressUser()}\n");
            Console.Write($"Host link - {storage.GetAddressHost()}\n");
            Console.Write($"Sitemap link - {storage.GetAddressSitemap()}\n");
            Console.WriteLine();
            Console.Write($"Count of links founded in sitemap.xml - {storage.GetCountOfSitemapAddresses()}\n");
            Console.Write($"Count of links founded in site code - {storage.GetCountOfCodeAddresses()}\n");
            Console.Write($"Count of all founded links - {storage.GetCountOfAllAddresses()}\n");
            Console.WriteLine();

            int index = 0;
            string tempLink;
            List<Part> parts = new List<Part>(); //from Microsoft documentation

            if (storage.GetCountOfAllAddresses() > 0)
            {
                Console.WriteLine("Urls FOUNDED IN SITEMAP.XML but not founded after crawling a web site:");
                //if sitemap list contain link and link-from-code list NOT contain link --> Console.WriteLine(tempLink); = true
                do
                {
                    tempLink = storage.GetAllAddressesFromList(index);
                    bool urlNotFoundAfterCrawling = false;

                    for (int j = 0; j < storage.GetCountOfSitemapAddresses(); j++)
                    {
                        if (storage.GetAddressFromSitemapList(j).Contains(tempLink))
                        {
                            if (storage.GetCountOfCodeAddresses() != 0) 
                            {
                                for (int k = 0; k < storage.GetCountOfCodeAddresses(); k++)
                                {
                                    if (!storage.GetAddressFromCodeList(k).Contains(tempLink))
                                    {
                                        urlNotFoundAfterCrawling = true;
                                    }
                                    else
                                    {
                                        urlNotFoundAfterCrawling = false;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                urlNotFoundAfterCrawling = true;
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

                Console.WriteLine("\nUrls FOUNDED BY CRAWLING THE WEBSITE but not in sitemap.xml");
                //if link-from-code list contain link and sitemap list NOT contain link --> Console.WriteLine(tempLink); = true
                //use storage.RemoveLinkFromSitemapList(INDEX); for debug and test
                index = 0;
                do
                {
                    bool urlNotFoundInSitemap = false;
                    tempLink = storage.GetAllAddressesFromList(index);

                    for (int j = 0; j < storage.GetCountOfCodeAddresses(); j++)
                    {
                        if (storage.GetAddressFromCodeList(j).Equals(tempLink))
                        {
                            if (storage.GetCountOfSitemapAddresses() != 0)
                            {
                                for (int k = 0; k < storage.GetCountOfSitemapAddresses(); k++)
                                {
                                    if (!storage.GetAddressFromSitemapList(k).Equals(tempLink))
                                    {
                                        urlNotFoundInSitemap = true;
                                    }
                                    else
                                    {
                                        urlNotFoundInSitemap = false;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                urlNotFoundInSitemap = true;
                            }
                            break;
                        }
                    }

                    if (urlNotFoundInSitemap)
                    {
                        Console.WriteLine(tempLink);
                    }

                    index++;
                } while (index != storage.GetCountOfAllAddresses());

                Console.WriteLine("\nTiming, please wait");
                index = 0;
                do
                {
                    Program.Response(storage.GetAllAddressesFromList(index), parts);
                    index++;
                } while (index != storage.GetCountOfAllAddresses());

                //from Microsoft documentation
                parts.Sort();
                foreach (Part aPart in parts)
                {
                    Console.WriteLine(aPart);
                }
            }
            else 
            {
                Console.WriteLine("Links not found");
            }

            Console.WriteLine("\nPress any key to finish");
            Console.ReadKey();
        }
    }

    //https://learn.microsoft.com/ru-ru/dotnet/api/system.collections.generic.list-1.sort?view=net-7.0
    public class Part : IEquatable<Part>, IComparable<Part>
    {
        public string PartName { get; set; }
        public long PartId { get; set; }

        public override string ToString()
        {
            return "Response Time: " + PartId + " ms " + "    " + PartName;
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
        public bool Equals(Part other)
        {
            if (other == null) return false;
            return (this.PartId.Equals(other.PartId));
        }
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

        private static void Parsing(Storage storage, string address, out string toSave)
        {
            //если сайтмап ссылка относительная
            if (address.Contains(".xml") && !address.StartsWith(storage.GetAddressUser())) 
            {
                if (address.StartsWith("//")) 
                {
                    address = address.Remove(0, 2); // delete "//"
                }
                else 
                {
                    address = address.Remove(0, 1); // delete "/"
                }
                address = address.Insert(0, storage.GetAddressUser());
            }

            //для симулированя отсутствия сайтмапа
            //if (address.Contains(".xml"))
            //{
            //    address = address.Insert(address.Length, "TEST");
            //}

            WebClient wc = new WebClient();
            try 
            {
                toSave = wc.DownloadString(address);
            }
            catch
            {
                toSave = "";
            }
        }

        //ignore links with:
        private static bool Filter(string link) 
        {
            if (!link.StartsWith("//") && !link.Contains("#") && !link.Contains("?") && !link.Contains(".json") && !link.Contains(".ico") && 
                !link.Contains(".png") && !link.Contains(".css") && !link.Contains(".pdf") && !link.Contains(".gif") && !link.Contains(".jpg") && 
                !link.Contains(".jpeg") && !link.Contains(".ttf") && !link.Contains(".woff") && !link.Contains(".js"))
            {
                return true;
            }
            return false;
        }

        public static bool Ping(string hostAddress)
        {
            //async Task<bool>
            Ping pingSend = new Ping();

            //IPHostEntry hostInfo = Dns.GetHostByName(hostAddress);
            //var host = hostInfo.AddressList.FirstOrDefault().ToString();

            try
            {
                PingReply reply = pingSend.Send(hostAddress);
                //var reply = await pingSend.SendPingAsync(host, 15000);

                if (reply.Status == IPStatus.Success)
                {
                    return true;
                }
                else 
                {
                    Console.WriteLine($"Host {hostAddress} not response. Status: {reply.Status}\n");
                }
            }
            catch (PingException exception)
            {
                Console.WriteLine($"    Exception: {hostAddress}");
                Console.WriteLine($"    {exception.InnerException.Message}\n");
            }

            return false;
        }

        public static void Response(string address, List<Part> parts)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(address);
            //Stopwatch timer = Stopwatch.StartNew();
            Stopwatch timer = new Stopwatch();

            timer.Start();

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                response.Close();
            }
            catch (WebException ex)
            {
                address = address.Insert(0, $"{ex.Message} - ");
            }

            timer.Stop();
            
            TimeSpan timeTaken = timer.Elapsed;

            timer.Reset();

            parts.Add(new Part() { PartName = address, PartId = (long)timeTaken.TotalMilliseconds / 10 });

            /*
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
                        }
                    }
                }
                catch
                {
                    watch.Stop();
                    parts.Add(new Part() { PartName = address, PartId = watch.ElapsedMilliseconds / 10 });
                }
            }
            */
        }

        // https://ukad-group.com/ --> https://ukad-group.com/sitemap.xml
        // get urls from sitemap
        public static void SitemapCheck(Storage storage)
        {
            List<string> xmlLinks = new List<string>();
            string tempUserAddressToSitemapAddress, sitemap, tempSitemapLink;
            string[] splitCode;
            int index = 0;
            int firstCharIndexOfAddress, lastCharIndexOfAddress;

            tempUserAddressToSitemapAddress = storage.GetAddressUser();
            storage.SetAddress(tempUserAddressToSitemapAddress.Insert(tempUserAddressToSitemapAddress.Length, "sitemap.xml"));

            Parsing(storage, storage.GetAddressSitemap(), out sitemap);

            if (sitemap != "")
            {
                do
                {
                    //для дебага на относительные ссылки в сайтмапе
                    //Uri test = new Uri(storage.GetAddressSitemap());
                    //xmlLinks.Insert(0, test.AbsolutePath);

                    if (xmlLinks.Any())
                    {
                        Parsing(storage, xmlLinks.ElementAt(0), out sitemap);
                        xmlLinks.RemoveAt(0);
                    }

                    splitCode = sitemap.Split('\n');

                    //начиная с исходной ссылки и заканчивая знаком <
                    foreach (string sitemapStr in splitCode)
                    {
                        firstCharIndexOfAddress = sitemapStr.IndexOf(storage.GetAddressUser(), 0);

                        if (firstCharIndexOfAddress != -1)
                        {
                            lastCharIndexOfAddress = sitemapStr.IndexOf("<", firstCharIndexOfAddress);

                            if (lastCharIndexOfAddress != -1)
                            {
                                tempSitemapLink = sitemapStr.Substring(firstCharIndexOfAddress, lastCharIndexOfAddress - firstCharIndexOfAddress);

                                // delete all after .xml
                                if (tempSitemapLink.Contains(".xml"))
                                {
                                    int indexOfXml = tempSitemapLink.IndexOf(".xml") + 4;
                                    tempSitemapLink = tempSitemapLink.Remove(indexOfXml, tempSitemapLink.Length - indexOfXml);
                                }

                                //если ссылка относительная и не ведет на внешний ресурс
                                string addressUserWithoutProtocol = storage.GetAddressUser();
                                string protocol = storage.GetAddressUser();
                                string addressUserWithoutProtocolAndBackslash;

                                addressUserWithoutProtocol = addressUserWithoutProtocol.Remove(0, addressUserWithoutProtocol.IndexOf(":") + 1);
                                protocol = protocol.Remove(protocol.IndexOf(":"), protocol.Length - protocol.IndexOf(":"));
                                addressUserWithoutProtocolAndBackslash = addressUserWithoutProtocol.Remove(addressUserWithoutProtocol.Length - 1, 1);

                                //дополнительная проверка
                                if (tempSitemapLink.StartsWith("//"))
                                {
                                    if (tempSitemapLink.Contains(addressUserWithoutProtocol) || tempSitemapLink.Contains(addressUserWithoutProtocolAndBackslash))
                                    {
                                        tempSitemapLink = tempSitemapLink.Insert(0, protocol + ":");
                                        if (!tempSitemapLink.EndsWith("/"))
                                        {
                                            tempSitemapLink = tempSitemapLink.Insert(tempSitemapLink.Length, "/");
                                        }
                                    }
                                }

                                if (Filter(tempSitemapLink))
                                {
                                    //если в сайтмапе относительная ссылка
                                    if (!tempSitemapLink.StartsWith(storage.GetAddressUser()))
                                    {
                                        //и сслыка начинается с /
                                        if (tempSitemapLink.StartsWith("/"))
                                        {
                                            //убрать первый символ
                                            tempSitemapLink = tempSitemapLink.Remove(0, 1);
                                        }

                                        //сделать ссылку абсолютной
                                        tempSitemapLink = tempSitemapLink.Insert(0, storage.GetAddressUser());
                                    }
                                    storage.SetNewAddressToSitemapList(tempSitemapLink);
                                }
                            }
                        }
                    }

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
                } while (xmlLinks.Any());
            }
        }

        public static void SiteCodeCheck(Storage storage)
        {
            string siteCode, buf;
            string[] splitCode;
            int index = 0;
            List<string> checkedAddressesFromCode = new List<string>();

            Parsing(storage, storage.GetAddressUser(), out siteCode);
            checkedAddressesFromCode.Add(storage.GetAddressUser());

            //https://sites.google.com/site/raznyeurokipoinformatiki/home/rabota-so-ssylkami/polucenie-dannyh-so-starnici-sajta
            if (siteCode != "") 
            {
                do
                {
                    splitCode = siteCode.Split('\n');

                    foreach (string str in splitCode)
                    {
                        for (int I = 0; I < str.Length - 5; I++)
                        {
                            if (str.Substring(I, 5) == "href=")
                            {
                                buf = "";

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

                                while (strTest.Length != indexTest && strTest.Substring(indexTest, 1) != "]" && 
                                    strTest.Substring(indexTest, 1) != " " && strTest.Substring(indexTest, 1) != @"""")
                                {
                                    buf += strTest.Substring(indexTest, 1); // собираем в буфер ссылку
                                    indexTest++; // берем следующий символ
                                }

                                //buf = buf.Remove(0, 6);

                                //если ссылка относительная и не ведет на внешний ресурс
                                string addressUserWithoutProtocol = storage.GetAddressUser();
                                string protocol = storage.GetAddressUser();
                                string addressUserWithoutProtocolAndBackslash;

                                addressUserWithoutProtocol = addressUserWithoutProtocol.Remove(0, addressUserWithoutProtocol.IndexOf(":") + 1);
                                protocol = protocol.Remove(protocol.IndexOf(":"), protocol.Length - protocol.IndexOf(":"));
                                addressUserWithoutProtocolAndBackslash = addressUserWithoutProtocol.Remove(addressUserWithoutProtocol.Length - 1, 1);

                                if (buf.StartsWith("//"))
                                {
                                    if (buf.Contains(addressUserWithoutProtocol) || buf.Contains(addressUserWithoutProtocolAndBackslash))
                                    {
                                        buf = buf.Insert(0, protocol + ":");
                                        if (!buf.EndsWith("/")) 
                                        {
                                            buf = buf.Insert(buf.Length, "/");
                                        }
                                    }   
                                }

                                if (Filter(buf) && buf.Any())
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

                    if (!storage.AnyLinksInCodeList()) 
                    {
                        break;
                    }

                    if (!checkedAddressesFromCode.AsReadOnly().ToString().Contains(storage.GetAddressFromCodeList(index)))
                    {
                        Parsing(storage, storage.GetAddressFromCodeList(index), out siteCode);
                        checkedAddressesFromCode.Add(storage.GetAddressFromCodeList(index));
                    }
                    index++;
                } while (storage.GetCountOfCodeAddresses() != checkedAddressesFromCode.Count());
            }
        }
    }
}