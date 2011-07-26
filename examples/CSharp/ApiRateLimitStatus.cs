using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TourCMS.Utils;
using System.Xml;

namespace TourCMSTesterCSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            // API Settings

                // Set your Marketplace ID 
                // For Suppliers this will be 0 
                // For Partners this can be found in the API settings page 
                int marketplaceId = 0;

                // Set the Channel ID 
                // For Suppliers this can be found in the API settings page 
                // For Partners this will be 0 
                int channelId = 0;

                // Set your API priate Key, find this in the API settings page
                string privateKey = "";

            // End API Settings

            // Create a new TourCMS object
            marketplaceWrapper myTourCMS = new marketplaceWrapper(marketplaceId, privateKey);

            // Call the API
            XmlDocument doc = myTourCMS.ApiRateLimitStatus(channelId);

            // Get the status from the XML, will be "OK" unless there's a problem
            string status = doc.GetElementsByTagName("error")[0].InnerText;

            if (status == "OK")
            {
                // Display API hits before the limit is reached
                string limit = doc.GetElementsByTagName("remaining_hits")[0].InnerText;
                Console.WriteLine("Remaining hits: " + limit);
            }
            else
            {
                Console.WriteLine("Error: " + status);
                Console.WriteLine("http://www.tourcms.com/support/api/mp/error_messages.php");
            }

        }
    }
}
