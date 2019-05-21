using System;
using System.Security.Cryptography;
using System.Web;
using System.Text;
using System.IO;
using System.Net;
using System.Xml;

namespace TourCMS.Utils
{
    /// <summary>
    /// Wrapper class for the XML REST based TourCMS Marketplace API
    /// </summary>
    public class marketplaceWrapper

    {
        // Properties

        /// <summary>
        /// API base URL
        /// </summary>
        protected string _baseUrl = "https://api.tourcms.com";

        /// <summary>
        /// Marketplace ID, set via the constructor
        /// </summary>
        protected int _marketplaceId = 0;

        /// <summary>
        /// API Private Key, set via the constructor
        /// </summary>
        protected string _privateKey = "";

        // Constructor

        /// <summary>
        /// Create a new instance of the TourCMS Marketplace wrapper, pass Marketplace ID and Key
        /// </summary>
        public marketplaceWrapper(int marketplaceId, string privateKey)
        {
            _marketplaceId = marketplaceId;
            _privateKey = privateKey;
        }

        // API request

        /// <summary>
        /// Make a generic request to the API, most documented calls will have wrappers for this
        /// </summary>
        public XmlDocument Request(string path)
        {
            return Request(path, 0, "GET", null);
        }
        /// <summary>
        /// Make a generic request to the API, most documented calls will have wrappers for this
        /// </summary>
        public XmlDocument Request(string path, int channelId)
        {
            return Request(path, channelId, "GET", null);
        }
        /// <summary>
        /// Make a generic request to the API, most documented calls will have wrappers for this
        /// </summary>
        public XmlDocument Request(string path, int channelId, string verb)
        {
            return Request(path, channelId, verb, null);
        }
        /// <summary>
        /// Make a generic request to the API, most documented calls will have wrappers for this
        /// </summary>
        public XmlDocument Request(string path, int channelId, string verb, XmlDocument postData)
        {
            string url = _baseUrl + path;
            DateTime outboundTime = DateTime.Now.ToUniversalTime();
            string signature = GenerateSignature(path, verb, channelId, outboundTime);
            XmlDocument doc = new XmlDocument();

            // Prepare web request...
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
            myRequest.Method = verb;
            myRequest.ContentType = "text/xml;charset=\"utf-8\"";
            myRequest.Headers.Add("Authorization", "TourCMS " + channelId + ":" + _marketplaceId + ":" + signature);
            myRequest.Headers.Add("x-tourcms-date", outboundTime.ToString("r"));

            if (postData != null)
            {
                // Wrap the request stream with a text-based writer
                StreamWriter writer = new StreamWriter(myRequest.GetRequestStream());
                // Write the XML text into the stream
                writer.WriteLine(postData.OuterXml);
                writer.Close();
            }

            // Call the API
            try
            {
                HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();
                StreamReader responseStream = new StreamReader(myResponse.GetResponseStream(), true);
                string responseText = responseStream.ReadToEnd();
                doc.LoadXml(responseText);
            }
            catch (WebException e)
            {
                StreamReader responseStream = new StreamReader(e.Response.GetResponseStream(), true);
                string responseText = responseStream.ReadToEnd();
                doc.LoadXml(responseText);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //doc.LoadXml("<response><error></error></response>");
            }
            return doc;
        }

        // Wrapper functions

        #region Housekeeping

        /// <summary>
        /// Check how many requests are left before throttling, handy for checking API connectivity
        /// </summary>
        public XmlDocument ApiRateLimitStatus()
        {
            return ApiRateLimitStatus(0);
        }
        /// <summary>
        /// Check how many requests are left before throttling, handy for checking API connectivity
        /// </summary>

        public XmlDocument ApiRateLimitStatus(int channelId)
        {
            return Request("/api/rate_limit_status.xml", channelId);
        }
        #endregion

        #region Channels (Suppliers)
        /// <summary>
        /// List all connected Channels (Suppliers)
        /// </summary>
        public XmlDocument ListChannels()
        {
            return Request("/p/channels/list.xml");
        }

        /// <summary>
        /// Show details on a specific Channel (Supplier)
        /// </summary>
        public XmlDocument ShowChannel(int channel)
        {
            return Request("/c/channel/show.xml", channel);
        }

        /// <summary>
        /// Get the statistics on the top 50 performing Channels (Suppliers)
        /// </summary>
        public XmlDocument ChannelPerformance()
        {
            return ChannelPerformance(0);
        }

        /// <summary>
        /// Show details on a specific Channel (Supplier)
        /// </summary>
        public XmlDocument ChannelPerformance(int channel)
        {
            if (channel == 0)
                return Request("/p/channels/performance.xml");
            else
                return Request("/c/channel/performance.xml", channel);
        }

        /// <summary>
        /// Check a promo code is valid for a Channel and view some info
        /// </summary>
        public XmlDocument ShowPromo(string promo, int channel)
        {
            return Request("'/c/promo/show.xml?promo_code=" + promo, channel);
        }

        #endregion

        #region Tours - General Use

        // Search Tours

        /// <summary>
        /// Get a list of Tours, optionally filter by a search querystring and/or limit to a specific channel
        /// </summary>
        public XmlDocument SearchTours()
        {
            return SearchTours("", 0);
        }

        /// <summary>
        /// Get a list of Tours, optionally filter by a search querystring and/or limit to a specific channel
        /// </summary>
        public XmlDocument SearchTours(string queryString)
        {
            return SearchTours(queryString, 0);
        }

        /// <summary>
        /// Get a list of Tours, optionally filter by a search querystring and/or limit to a specific channel
        /// </summary>
        public XmlDocument SearchTours(string queryString, int channelId)
        {
            if (channelId == 0)
                return Request("/p/tours/search.xml?" + queryString);
            else
                return Request("/c/tours/search.xml?" + queryString, channelId);
        }

        // Search Hotels

        /// <summary>
        /// Search Hotel type tours by a date range, optionally filter by a search querystring and/or limit to a specific channel
        /// </summary>
        public XmlDocument SearchHotelsByRange()
        {
            return SearchHotelsByRange("", "", 0);
        }

        /// <summary>
        /// Search Hotel type tours by a date range, optionally filter by a search querystring and/or limit to a specific channel
        /// </summary>
        public XmlDocument SearchHotelsByRange(string queryString)
        {
            return SearchHotelsByRange(queryString, "", 0);
        }

        /// <summary>
        /// Search Hotel type tours by a date range, optionally filter by a search querystring and/or limit to a specific channel
        /// </summary>
        public XmlDocument SearchHotelsByRange(string queryString, string tour)
        {
            return SearchHotelsByRange(queryString, tour, 0);
        }

        /// <summary>
        /// Search Hotel type tours by a date range, optionally filter by a search querystring and/or limit to a specific channel
        /// </summary>
        public XmlDocument SearchHotelsByRange(string queryString, string tour, int channelId)
        {
            if (channelId == 0)
                return Request("/p/hotels/search_range.xml?" + queryString + "single_tour_id=" + tour);
            else
                return Request("/c/hotels/search_range.xml?" + queryString + "single_tour_id=" + tour, channelId);
        }

        /// <summary>
        /// Search Hotel type tours for specific dates/occupancy, optionally filter by a search querystring and/or limit to a specific channel
        /// </summary>
        public XmlDocument SearchHotelsBySpecific()
        {
            return SearchHotelsBySpecific("", "", 0);
        }

        /// <summary>
        /// Search Hotel type tours for specific dates/occupancy, optionally filter by a search querystring and/or limit to a specific channel
        /// </summary>
        public XmlDocument SearchHotelsBySpecific(string queryString)
        {
            return SearchHotelsBySpecific(queryString, "", 0);
        }

        /// <summary>
        /// Search Hotel type tours for specific dates/occupancy, optionally filter by a search querystring and/or limit to a specific channel
        /// </summary>
        public XmlDocument SearchHotelsBySpecific(string queryString, string tour)
        {
            return SearchHotelsBySpecific(queryString, tour, 0);
        }

        /// <summary>
        /// Search Hotel type tours for specific dates/occupancy, optionally filter by a search querystring and/or limit to a specific channel
        /// </summary>
        public XmlDocument SearchHotelsBySpecific(string queryString, string tour, int channelId)
        {
            if (channelId == 0)
                return Request("/p/hotels/search_avail.xml?" + queryString + "single_tour_id=" + tour);
            else
                return Request("/c/hotels/search_avail.xml?" + queryString + "single_tour_id=" + tour, channelId);
        }

        // Show Tour

        /// <summary>
        /// Show all of the details on a specific Tour, all descriptions and other content etc
        /// </summary>
        public XmlDocument ShowTour(int tourId, int ChannelId)
        {
            return ShowTour(tourId, ChannelId, "");
        }

        /// <summary>
        /// Show all of the details on a specific Tour, all descriptions and other content etc
        /// </summary>
        public XmlDocument ShowTour(int tourId, int ChannelId, bool showOptions)
        {
            if (showOptions)
            {
                return ShowTour(tourId, ChannelId, "show_options=1");
            }
            else
            {
                return ShowTour(tourId, ChannelId);
            }
        }

        /// <summary>
        /// Show all of the details on a specific Tour, all descriptions and other content etc
        /// </summary>
        public XmlDocument ShowTour(int tourId, int ChannelId, string queryString)
        {
            return Request("/c/tour/show.xml?id=" + tourId + "&" + queryString, ChannelId);
        }

        // Update Tour

        /// <summary>
        /// Update a Tour/Hotel
        /// </summary>
        public XmlDocument UpdateTour(XmlDocument tourData, int channelId)
        {
            return Request("/c/tour/update.xml", channelId, "POST", tourData);
        }

        // Update Tour Product URL

        /// <summary>
        /// Update the "Product URL" on a Tour/Hotel
        /// </summary>
        public XmlDocument UpdateTourUrl(int tourId, int channelId, string tourUrl)
        {
            // Build XML Document to hold the new Url
            XmlDocument tourData = new XmlDocument();

            // Create the XML Declaration, append it to XML document
            XmlDeclaration dec = tourData.CreateXmlDeclaration("1.0", null, null);
            tourData.AppendChild(dec);

            // Create the root element, append it to the XML document
            XmlElement root = tourData.CreateElement("tour");
            tourData.AppendChild(root);

            // Set the Tour ID so TourCMS knows which Tour/Hotel to update
            XmlElement tourIdXml = tourData.CreateElement("tour_id");
            tourIdXml.InnerText = tourId.ToString();
            root.AppendChild(tourIdXml);

            // Set the new URL
            XmlElement tourUrlXml = tourData.CreateElement("tour_url");
            tourUrlXml.InnerText = tourUrl;
            root.AppendChild(tourUrlXml);

            // Call the regular API wrapper for updating the Tour/Hotel
            return UpdateTour(tourData, channelId);
        }

        #endregion

        #region Tours - Raw departure management

        // Search Raw Departures

        /// <summary>
        /// Get a list of "Raw" Departures for a particular Tour
        /// </summary>
        public XmlDocument SearchRawDepartures(int tourId, int channelId)
        {
            return Request("/c/tour/datesprices/dep/manage/search.xml?id=" + tourId, channelId);
        }

        // Create Departure

        /// <summary>
        /// Create a new departure
        /// </summary>
        public XmlDocument CreateDeparture(XmlDocument departureData, int channelId)
        {
            return Request("/c/tour/datesprices/dep/manage/new.xml", channelId, "POST", departureData);
        }

        // Update Departure

        /// <summary>
        /// Update an existing departure
        /// </summary>
        public XmlDocument UpdateDeparture(XmlDocument departureData, int channelId)
        {
            return Request("/c/tour/datesprices/dep/manage/update.xml", channelId, "POST", departureData);
        }

        // Delete Departure

        /// <summary>
        /// Update an existing departure
        /// </summary>
        public XmlDocument DeleteDeparture(int departure, int tourId, int channelId)
        {
            return Request("/c/tour/datesprices/dep/manage/delete.xml?id=" + tourId + "&departure_id=" + departure, channelId, "POST");
        }

        #endregion

        #region Tours - Bulk exporting Use
        // List Tours

        /// <summary>
        /// Get a list of Tours with only very basic information. Probably most useful when bulk importing. For a more detailed list use <c>SearchTours</c>
        /// </summary>
        public XmlDocument ListTours()
        {
            return ListTours(0, "");
        }

        /// <summary>
        /// Get a list of Tours with only very basic information. Probably most useful when bulk importing. For a more detailed list use <c>SearchTours</c>
        /// </summary>
        public XmlDocument ListTours(int channelId)
        {
            return ListTours(channelId, "");
        }

        /// <summary>
        /// Get a list of Tours with only very basic information. Probably most useful when bulk importing. For a more detailed list use <c>SearchTours</c>
        /// </summary>
        public XmlDocument ListTours(string queryString)
        {
            return ListTours(0, queryString);
        }

        /// <summary>
        /// Get a list of Tours with only very basic information. Probably most useful when bulk importing. For a more detailed list use <c>SearchTours</c>
        /// </summary>
        public XmlDocument ListTours(int channelId, string queryString)
        {
            if (channelId == 0)
                return Request("/p/tours/list.xml?" + queryString);
            else
                return Request("/c/tours/list.xml?" + queryString, channelId);
        }

        // List Tour Images

        /// <summary>
        /// Get a list of Image URLs, use this if you want to bulk-mirror product images on your own hosting
        /// </summary>
        public XmlDocument ListTourImages()
        {
            return ListTourImages("", 0);
        }

        /// <summary>
        /// Get a list of Image URLs for a specific Channel (Company), use this if you want to bulk-mirror product images on your own hosting
        /// </summary>
        public XmlDocument ListTourImages(int channelId)
        {
            return ListTourImages("", channelId);
        }

        /// <summary>
        /// Get a list of Image URLs for a specific Channel (Company), use this if you want to bulk-mirror product images on your own hosting
        /// </summary>
        public XmlDocument ListTourImages(string queryString)
        {
            return ListTourImages(queryString, 0);
        }

        /// <summary>
        /// Get a list of Image URLs for a specific Channel (Company), use this if you want to bulk-mirror product images on your own hosting
        /// </summary>
        public XmlDocument ListTourImages(string queryString, int channelId)
        {
            if (channelId == 0)
                return Request("/p/tours/images/list.xml?" + queryString);
            else
                return Request("/c/tours/images/list.xml?" + queryString, channelId);
        }

        /// <summary>
        /// Retrieve a list of connected tour locations
        /// </summary>
        public XmlDocument ListTourLocations()
        {
            return ListTourLocations("", 0);
        }

        /// <summary>
        /// Retrieve a list of connected tour locations for a specific Channel (Company)
        /// </summary>
        public XmlDocument ListTourLocations(int channelId)
        {
            return ListTourImages("", channelId);
        }

        /// <summary>
        /// Retrieve a list of connected tour locations on any connected Channel, with a queryString
        /// </summary>
        public XmlDocument ListTourLocations(string queryString)
        {
            return ListTourLocations(queryString, 0);
        }

        /// <summary>
        /// Retrieve a list of connected tour locations for a specific Channel (Company)
        /// </summary>
        public XmlDocument ListTourLocations(string queryString, int channelId)
        {
            if (channelId == 0)
                return Request("/p/tours/locations.xml");
            else
                return Request("/c/tours/locations.xml", channelId);
        }


        // Check Tour/Hotel availability

        /// <summary>
        /// Check Tour/Hotel availability for a specific number of people on specific rates on a specific date, generally used when making a booking
        /// </summary>
        public XmlDocument CheckTourAvailability(string queryString, int tourId, int channelId)
        {
            return Request("/c/tour/datesprices/checkavail.xml?id=" + tourId + "&" + queryString, channelId);
        }

        // Show Tour Dates and Deals

        /// <summary>
        /// Get a list of dates and deals/special offers for a particular Tour
        /// </summary>
        public XmlDocument ShowTourDatesAndDeals(int tourId, int channelId)
        {
            return Request("/c/tour/datesprices/datesndeals/search.xml?id=" + tourId, channelId);
        }

        // Show Tour Departures

        /// <summary>
        /// Get a list of all future Departures for a specific Tour/Hotel, useful if bulk importing availability information
        /// </summary>
        public XmlDocument ShowTourDepartures(int tourId, int channelId)
        {
            return ShowTourDepartures(tourId, channelId, "");
        }

        /// <summary>
        /// Get a list of all future Departures for a specific Tour/Hotel, useful if bulk importing availability information
        /// </summary>
        public XmlDocument ShowTourDepartures(int tourId, int channelId, string queryString)
        {
            return Request("/c/tour/datesprices/dep/show.xml?id=" + tourId + "&" + queryString, channelId);
        }

        // Show Tour Freesale

        /// <summary>
        /// Get a list of all future Freesale Seasons for a specific Tour/Hotel, useful if bulk importing availability information
        /// </summary>
        public XmlDocument ShowTourFeesale(int tourId, int channelId)
        {
            return Request("/c/tour/datesprices/freesale/show.xml?id=" + tourId, channelId);
        }

        #endregion

        #region Bookings - Creation

        // Get booking redirect URL

        /// <summary>
        /// Get a redirect URL from TourCMS, send the customer via it to retrieve a booking key, Tour Op use only
        /// </summary>
        public XmlDocument GetBookingRedirectUrl(XmlDocument urlData, int channelId)
        {
            return Request("/c/booking/new/get_redirect_url.xml", channelId, "POST", urlData);
        }

        // Start new booking

        /// <summary>
        /// Create a temporary booking
        /// </summary>
        public XmlDocument StartNewBooking(XmlDocument bookingData, int channelId)
        {
            return Request("/c/booking/new/start.xml", channelId, "POST", bookingData);
        }

        // Commit new booking

        /// <summary>
        /// Convert a temporary booking to a confirmed booking
        /// </summary>
        public XmlDocument CommitNewBooking(XmlDocument bookingData, int channelId)
        {
            return Request("/c/booking/new/commit.xml", channelId, "POST", bookingData);
        }

        #endregion

        #region Bookings - Retrieval

        // Search bookings

        /// <summary>
        /// Get a list of all bookings, optionally filter by channel / querystring
        /// </summary>
        public XmlDocument SearchBookings()
        {
            return SearchBookings("", 0);
        }

        /// <summary>
        /// Get a list of all bookings, optionally filter by channel / querystring
        /// </summary>
        public XmlDocument SearchBookings(string queryString)
        {
            return SearchBookings(queryString, 0);
        }

        /// <summary>
        /// Get a list of all bookings, optionally filter by channel / querystring
        /// </summary>
        public XmlDocument SearchBookings(string queryString, int channelId)
        {
            if (channelId == 0)
                return Request("/p/bookings/search.xml?" + queryString);
            else
                return Request("/c/bookings/search.xml?" + queryString, channelId);
        }

        // Show booking

        /// <summary>
        /// Get details on a particular booking
        /// </summary>
        public XmlDocument ShowBooking(int bookingId, int channelId)
        {
            return Request("/c/booking/show.xml?booking_id=" + bookingId, channelId);
        }

        #endregion

        #region Bookings - Updating

        // Update booking

        /// <summary>
        /// Update some of the details on a booking
        /// </summary>
        public XmlDocument UpdateBooking(XmlDocument bookingData, int channelId)
        {
            return Request("/c/booking/update.xml", channelId, "POST", bookingData);
        }

        // Record a payment / refund

        /// <summary>
        /// Log details of a payment/refund on a booking sales ledger
        /// </summary>
        public XmlDocument CreatePayment(XmlDocument paymentData, int channelId)
        {
            return Request("/c/booking/payment/new.xml", channelId, "POST", paymentData);
        }

        /// <summary>
        /// Log details of a payment/refund on a booking sales ledger
        /// </summary>
        public XmlDocument LogFailedPayment(XmlDocument paymentData, int channelId)
        {
            return Request("/c/booking/payment/fail.xml", channelId, "POST", paymentData);
        }

        #endregion

        #region Bookings - Cancel

        /// <summary>
        /// Cancel a booking
        /// </summary>
        public XmlDocument CancelBooking(XmlDocument bookingData, int channelId)
        {
            return Request("/c/booking/cancel.xml", channelId, "POST", bookingData);
        }

        #endregion

        #region Customers and Enquiries

        // Create Customer/Enquiry

        /// <summary>
        /// Create an enquiry, either with a new customer record or associated with an existing one
        /// </summary>
        public XmlDocument CreateEnquiry(XmlDocument enquiryData, int channelId)
        {
            return Request("/c/enquiry/new.xml", channelId, "POST", enquiryData);
        }

        /// <summary>
        /// Create a new customer record, optionally include some enquiry data
        /// </summary>
        public XmlDocument CreateCustomer(XmlDocument customerData, int channelId)
        {
            return CreateEnquiry(customerData, channelId);
        }

        // Update Customer

        /// <summary>
        /// Update an existing customer record, e.g. contact details
        /// </summary>
        public XmlDocument UpdateCustomer(XmlDocument customerData, int channelId)
        {
            return Request("/c/customer/update.xml", channelId, "POST", customerData);
        }

        // Search Enquiries

        /// <summary>
        /// Get a list of enquiries, filter by date, channel etc
        /// </summary>
        public XmlDocument SearchEnquiries()
        {
            return SearchEnquiries("", 0);
        }

        /// <summary>
        /// Get a list of enquiries, filter by date, channel etc
        /// </summary>
        public XmlDocument SearchEnquiries(String queryString)
        {
            return SearchEnquiries(queryString, 0);
        }

        /// <summary>
        /// Get a list of enquiries, filter by date, channel etc
        /// </summary>
        public XmlDocument SearchEnquiries(String queryString, int channelId)
        {
            if (channelId == 0)
            {
                return Request("/p/enquiries/search.xml?" + queryString, channelId);
            }
            else
            {
                return Request("/c/enquiries/search.xml?" + queryString, channelId);
            }
        }

        // Show Enquiry

        /// <summary>
        /// Get information on a particular enquiry, by passing it's ID and Channel
        /// </summary>
        public XmlDocument ShowEnquiry(int enquiryId, int channelId)
        {
            return Request("/c/enquiry/show.xml?enquiry_id=" + enquiryId, channelId);
        }

        // Show Customer

        /// <summary>
        /// Get information on a particular Customer, by passing their ID and Channel
        /// </summary>
        public XmlDocument ShowCustomer(int customerId, int channelId)
        {
            return Request("/c/customer/show.xml?customer_id=" + customerId, channelId);
        }


        // Check Customer Login

        /// <summary>
        /// Validate a Customer Username and Password, retrieve their Name and User ID
        /// </summary>
        public XmlDocument CheckCustomerLogin(string username, string password, int channelId)
        {
            return Request("/c/customers/login_search.xml?customer_username=" + username + "&customer_password=" + password, channelId);
        }

        #endregion

        #region Internal Supplier Methods

        // Show Supplier

        /// <summary>
        /// Get details on an internal Supplier ID (For Operator use only, not for Marketplace Partner use)
        /// </summary>
        public XmlDocument ShowSupplier(int supplierId, int channelId)
        {
            return Request("/c/supplier/show.xml?supplier_id=" + supplierId, channelId);
        }

        #endregion

        // End wrapper functions

        /// <summary>
        /// Create an encrypted signature for a particular request, generally you won't neeed to call this directly
        /// </summary>
        protected string GenerateSignature(string path, string verb, int channelId, DateTime outboundTime)
        {
            string stringToSign = "";
            string signature = "";
            string outboundStamp = DateTimeToStamp(outboundTime).ToString();

            // Build the basic string that gets signed
            stringToSign = channelId + "/" + _marketplaceId + "/" + verb + "/" + outboundStamp + path;
            stringToSign = stringToSign.Trim();

            // Sign with private API Key
            var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(_privateKey));
            hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));
            signature = Convert.ToBase64String(hmacsha256.Hash);
            signature = HttpUtility.UrlEncode(signature);

            return signature;
        }

        /// <summary>
        /// Convert a DateTime object (in UTC) to a Unix Timestapm, generally you won't neeed to call this directly
        /// </summary>
        protected int DateTimeToStamp(DateTime value)
        {
            TimeSpan span = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0));
            return (int)span.TotalSeconds;
        }

        /// <summary>
        /// Convert a Unix Timestamp to a UTC DateTime object, generally you won't need to call this directly
        /// </summary>
        protected DateTime StampToDateTime(int value)
        {
            System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            return dateTime.AddSeconds(value);
        }
    }
}