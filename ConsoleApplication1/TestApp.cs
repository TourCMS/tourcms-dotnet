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
            #region TestApp Settings
            //
            // Tour Operators: 
            // Set channelId and privateKey, leave marketplaceId as 0
            //
            // Partners (Affiliates, Agents etc):
            // Set marketplaceId and privateKey, leave channelId as 0
            // optionally set channelId to return just products from  
            // that connection
            //
            // Both:
            // Optionally set a queryString to filter results
            // http://www.tourcms.com/support/api/mp/tour_search.php
            //

            int channelId = Properties.Settings.Default.channelId;
            int marketplaceId = Properties.Settings.Default.marketplaceId;
            string privateKey = Properties.Settings.Default.privateKey;

            //string queryString = "lat=56.82127&long=-6.09139&k=walking&per_page=3";
            string queryString = "";
            //
            //
            #endregion

            // Check settings
            if ((channelId == 0 && marketplaceId == 0) || privateKey == "")
            {
                Console.WriteLine("Warning: You need to enter your API settings for this test app to work!");
            }

            // Create a new TourCMS object
            marketplaceWrapper myTourCMS = new marketplaceWrapper(marketplaceId, privateKey);

            // Query the TourCMS API 
            XmlDocument doc = myTourCMS.SearchTours(queryString, channelId);

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
            }
            else
            {
                Console.WriteLine("Error: " + status);
            }
        }
    }
}
