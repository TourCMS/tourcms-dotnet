using System;
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

            #endregion

            #region Cancel Booking example

            //channelId = 0;
            //marketplaceId = 0;
            //privateKey = "";
            //int booking_id = -1;

            //TestCancelBooking(channelId, marketplaceId, privateKey, booking_id);

            #endregion

            #region Search tours example

            channelId = 0;
            marketplaceId = 0;
            privateKey = "";
            //string queryString = "lat=56.82127&long=-6.09139&k=walking&per_page=3";
            //string queryString = "";
            string query_string = "lat=51.510545&long=-0.121096&per_page=3";

            TestSearchTours(channelId, marketplaceId, privateKey, query_string);            

            #endregion

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }

        private static void SearchTours(int channel_id, int marketplace_id, string private_key, string query_string)
        {
            // Check settings.
            if ((channel_id == 0 && marketplace_id == 0) || private_key == "")
            {
                Console.WriteLine("Warning: You need to enter your API settings for this test app to work!");
            }
            else
            {
                // Create a new TourCMS object.
                marketplaceWrapper tourCMS = new marketplaceWrapper(marketplace_id, private_key);

                // Query the TourCMS API.
                XmlDocument doc = tourCMS.SearchTours(query_string, channel_id);

                // Check the status, will be OK if there's no problem.
                string status = doc.GetElementsByTagName("error")[0].InnerText;
                if (status == "OK")
                {
                    // Display Tour list.
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

        private static void TestSearchTours(int channel_id, int marketplace_id, string private_key, string query_string)
        {
            SearchTours(channel_id, marketplace_id, private_key, query_string);
        }

        private static void CancelBooking(int channel_id, int marketplace, string api_key, XmlDocument booking_data)
        {
            // Check settings
            if ((channel_id == 0 && marketplace == 0) || api_key == "")
            {
                Console.WriteLine("Warning: You need to enter your API settings for this test app to work!");
            }
            else
            {
                // Create a new TourCMS object.
                marketplaceWrapper tourcms = new marketplaceWrapper(marketplace, api_key);

                // Query the TourCMS API.
                XmlDocument response = tourcms.CancelBooking(booking_data, channel_id);

                // Check the status, will be OK if there's no problem.
                string status = response.GetElementsByTagName("error")[0].InnerText;
                string booking_id = booking_data.GetElementsByTagName("booking_id")[0].InnerText;
                if (!string.IsNullOrEmpty(booking_id))
                    Console.WriteLine("Booking ID: " + booking_id);
                if (status == "OK")
                {
                    Console.WriteLine("Booking cancelled.");
                }
                else
                {
                    Console.WriteLine("Error: " + status);
                }
            }
        }

        private static void TestCancelBooking(int channel_id, int marketplace_id, string private_key, int booking_id)
        {
            // Create an XMLDocument to hold the booking details
            XmlDocument booking_data = new XmlDocument();

            // Create the XML Declaration, append it to XML document
            XmlDeclaration dec = booking_data.CreateXmlDeclaration("1.0", null, null);
            booking_data.AppendChild(dec);

            // Create the root element, append it to the XML document
            XmlElement root = booking_data.CreateElement("booking");
            booking_data.AppendChild(root);

            // Must set the Booking ID so TourCMS knows which booking to update
            XmlElement bookingId = booking_data.CreateElement("booking_id");
            bookingId.InnerText = booking_id.ToString();
            root.AppendChild(bookingId);

            // Optionally add a note explaining the reason for cancellation
            XmlElement req = booking_data.CreateElement("note");
            req.InnerText = "Booking created for testing Cancel Booking Method";
            root.AppendChild(req);

            CancelBooking(channel_id, marketplace_id, private_key, booking_data);
        }
    }
}
