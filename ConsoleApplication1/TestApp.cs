using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using TourCMS.Utils;

namespace ConsoleApplication1
{
    class TestApp
    {
        static void Main(string[] args)
        {
            // Create a new TourCMS object
            marketplaceWrapper myTourCMS = new marketplaceWrapper(Properties.Settings.Default.marketplaceId, Properties.Settings.Default.privateKey);

            int channelId = Properties.Settings.Default.channelId;

            //string queryString = "lat=56.82127&long=-6.09139&k=walking&per_page=3";
            string queryString = "";

            // Query the TourCMS API 
            XmlDocument doc = myTourCMS.SearchTours("", channelId);

            // Check the status, will be OK if there's no problem
            string status = doc.GetElementsByTagName("error")[0].InnerText;

            if (status == "OK")
            {

                // Display Tour list 
                XmlNodeList tourList = doc.GetElementsByTagName("tour");
                foreach (XmlNode tour in tourList)
                {
                    string tourName = tour.SelectSingleNode("tour_name").InnerText;
                    Console.WriteLine(tourName);
                }
            } else {
                Console.WriteLine("Error: " + status);
            }

            
        }
    }
}
