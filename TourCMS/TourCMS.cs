using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Web;
using System.Text;
using System.IO;
using System.Net;
using System.Xml;
using System.Linq;

namespace TourCMS.Utils
{
    public class marketplaceWrapper
    {
        protected string _baseUrl = "https://api.tourcms.com";
        protected int _marketplaceId = 0;
        protected string _privateKey = "";

        // Constructor
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
            return Request(path, 0, "GET");
        }
        /// <summary>
        /// Make a generic request to the API, most documented calls will have wrappers for this
        /// </summary>
        public XmlDocument Request(string path, int channelId)
        {
            return Request(path, channelId, "GET");
        }
        /// <summary>
        /// Make a generic request to the API, most documented calls will have wrappers for this
        /// </summary>
        public XmlDocument Request(string path, int channelId, string verb)
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
            public XmlDocument ListChannels() {
		        return Request("/p/channels/list.xml");
	        }

            /// <summary>
            /// Show details on a specific Channel (Supplier)
            /// </summary>
	        public XmlDocument ShowChannel(int channel) {
		        return Request("/c/channel/show.xml", channel);
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
            public XmlDocument SearchTours(string queryString) {
                return SearchTours(queryString, 0);
	        }

            /// <summary>
            /// Get a list of Tours, optionally filter by a search querystring and/or limit to a specific channel
            /// </summary>
            public XmlDocument SearchTours(string queryString, int channelId) {
		        if(channelId==0) 
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
		        if(channelId==0)
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
            public XmlDocument ShowTour(int tourId, int ChannelId) {
		        return Request("/c/tour/show.xml?id=" + tourId, ChannelId);		
	        }
	
        #endregion

        #region Tours - Bulk exporting Use
            // List Tours

            /// <summary>
            /// Get a list of Tours with only very basic information. Probably most useful when bulk importing. For a more detailed list use <c>SearchTours</c>
            /// </summary>
            public XmlDocument ListTours()
            {
                return ListTours(0);
            }

            /// <summary>
            /// Get a list of Tours with only very basic information. Probably most useful when bulk importing. For a more detailed list use <c>SearchTours</c>
            /// </summary>
            public XmlDocument ListTours(int channelId)
            {
                if (channelId == 0)
                    return Request("/p/tours/list.xml");
                else
                    return Request("/c/tours/list.xml", channelId);
            }

            // List Tour Images

            /// <summary>
            /// Get a list of Image URLs, use this if you want to bulk-mirror product images on your own hosting
            /// </summary>
            public XmlDocument ListTourImages()
            {
                return ListTourImages(0);
            }

            /// <summary>
            /// Get a list of Image URLs for a specific Channel (Company), use this if you want to bulk-mirror product images on your own hosting
            /// </summary>
            public XmlDocument ListTourImages(int channelId)
            {
                if (channelId == 0)
                    return Request("/p/tours/images/list.xml");
                else
                    return Request("/c/tours/images/list.xml", channelId);
            }

            // Show Tour Departures

            /// <summary>
            /// Get a list of all future Departures for a specific Tour/Hotel, useful if bulk importing availability information
            /// </summary>
            public XmlDocument ShowTourDepartures(int tourId, int channelId)
	        {
		        return Request("/c/tour/datesprices/dep/show.xml?id=" + tourId, channelId);
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

        // End wrapper functions

        // Create an encrypted signature for a particular request
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

        // Functions for converting between PHP dates
        protected int DateTimeToStamp(DateTime value)
        {
            TimeSpan span = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0));
            return (int)span.TotalSeconds;
        }
        protected DateTime StampToDateTime(int value)
        {
            System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            return dateTime.AddSeconds(value);
        }
    }
}