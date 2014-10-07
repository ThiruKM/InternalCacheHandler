namespace Utilities.Web.HttpHandlers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Reflection;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    /// <summary>
    /// This class is an HttpHandler and reports on the ASP.NET Cache
    /// </summary>
    public class InternalCacheHandler : IHttpHandler
    {
        /// <summary>
        /// The page style.
        /// </summary>
        private readonly string pageStyle;

        /// <summary>
        /// The context.
        /// </summary>
        private HttpContext context;

        /// <summary>
        /// The request.
        /// </summary>
        private HttpRequest request;

        /// <summary>
        /// The response.
        /// </summary>
        private HttpResponse response;

        /// <summary>
        /// The writer.
        /// </summary>
        private HtmlTextWriter writer;

        /// <summary>
        /// The display output cache.
        /// </summary>
        private bool displayOutputCache;

        /// <summary>
        /// The display user control cache.
        /// </summary>
        private bool displayUserControlCache;


        /// <summary>
        /// Initializes a new instance of the <see cref="InternalCacheHandler"/> class.
        /// </summary>
        public InternalCacheHandler()
        {
            this.pageStyle = @"<style> {background-color:white; color:black;    font: 10pt verdana, arial;}
                table {font: 10pt verdana, arial; cellspacing:0;     cellpadding:0;     margin-bottom:25}
                tr.subhead { background-color:cccccc;}
                th { padding:0,3,0,3 }
                th.alt { background-color:black; color:white; padding:3,3,2,3; }
                td { padding:0,3,0,3 }
                tr.alt { background-color:eeeeee }
                td.duplicate {background-color:red }
                h1 { font: 24pt verdana, arial; margin:0,0,0,0}
                h2 { font: 18pt verdana, arial; margin:0,0,0,0}
                h3 { font: 12pt verdana, arial; margin:0,0,0,0}
                th a { color:darkblue; font: 8pt verdana, arial; }
                a { color:darkblue;text-decoration:none }
                a:hover { color:darkblue;text-decoration:underline; }
                div.outer { width:90%; margin:15,15,15,15}
                table.viewmenu td { background-color:006699; color:white; padding:0,5,0,5; }
                table.viewmenu td.end { padding:0,0,0,0; }
                table.viewmenu a {color:white; font: 8pt verdana, arial; }
                table.viewmenu a:hover {color:white; font: 8pt verdana, arial; }
                a.tinylink {color:darkblue; font: 8pt verdana, arial;text-decoration:underline;}
                a.link {color:darkblue; text-decoration:underline;}
                div.buffer {padding-top:7; padding-bottom:17;}
                .small { font: 8pt verdana, arial }
                table td { padding-right:20 }
                table td.nopad { padding-right:5 }
            </style>";
        }

        /// <summary>
        /// The enumerate and display internal cache.
        /// </summary>
        /// <param name="displayOutCache">
        /// The display output cache.
        /// </param>
        /// <param name="displayUserCtlCache">
        /// The display user control cache.
        /// </param>
        private void EnumerateAndDisplayInternalCache(bool displayOutCache, bool displayUserCtlCache)
        {
            var runtimeType = typeof(HttpRuntime);

            var ci = runtimeType.GetProperty("CacheInternal", BindingFlags.NonPublic | BindingFlags.Static);

            object cache = ci.GetValue(ci, new object[0]);

            var cachesInfo = cache.GetType().GetField("_caches", BindingFlags.NonPublic | BindingFlags.Instance);
            if (cachesInfo == null)
            {
                return;
            }

            var cacheEntries = cachesInfo.GetValue(cache);

            var outputCacheEntries = new List<object>();
            var partialCachingCacheEntries = new List<object>();
            Table cacheTable;

            foreach (var singleCache in cacheEntries as Array)
            {
                var singleCacheInfo = singleCache.GetType()
                    .GetField("_entries", BindingFlags.NonPublic | BindingFlags.Instance);
                if (singleCacheInfo != null)
                {
                    var entries = singleCacheInfo.GetValue(singleCache);

                    var hashtable = entries as Hashtable;
                    if (hashtable != null)
                    {
                        foreach (DictionaryEntry cacheEntry in hashtable)
                        {
                            var cacheEntryInfo = cacheEntry.Value.GetType()
                                .GetField("_value", BindingFlags.NonPublic | BindingFlags.Instance);
                            if (cacheEntryInfo != null)
                            {
                                var value = cacheEntryInfo.GetValue(cacheEntry.Value);

                                if (value.GetType().Name == "PartialCachingCacheEntry")
                                {
                                    partialCachingCacheEntries.Add(value);
                                }

                                if (value.GetType().Name == "CachedRawResponse")
                                {
                                    outputCacheEntries.Add(value);
                                }
                            }
                        }
                    }
                }
            }


            if (displayOutCache)
            {
                cacheTable = TableHelper.CreateTable();

                // Table Header
                var mainHeadingRow = new TableRow();
                cacheTable.Rows.Add(mainHeadingRow);

                var mainHeading = TableHelper.AddHeaderCell(mainHeadingRow, "<h3><b>Output Cache Details</b></h3>");
                mainHeading.ColumnSpan = 3;
                mainHeading.CssClass = "alt";

                var secondaryHeadingRow = new TableRow { CssClass = "subhead" };
                cacheTable.Rows.Add(secondaryHeadingRow);
                var alternatingRowToggle = false;
                foreach (object cachedItem in outputCacheEntries)
                {
                    var dataRow = new TableRow();

                    var httpRawResponseField = cachedItem.GetType()
                        .GetField("_rawResponse", BindingFlags.NonPublic | BindingFlags.Instance);
                    var httpRawResponse = httpRawResponseField.GetValue(cachedItem);

                    var buffersField = httpRawResponse.GetType()
                        .GetField("_buffers", BindingFlags.NonPublic | BindingFlags.Instance);
                    var buffers = buffersField.GetValue(httpRawResponse) as ArrayList;

                    var result = string.Empty;
                    foreach (object buffer in buffers)
                    {
                        MethodInfo getBytesMethodInfo = buffer.GetType()
                            .GetMethod("System.Web.IHttpResponseElement.GetBytes", BindingFlags.NonPublic | BindingFlags.Instance);
                        //MethodInfo getBytesMethodInfo1 =
                        //    buffer.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)[8];
                        var resultBytes = getBytesMethodInfo.Invoke(buffer, null) as byte[];
                        if (resultBytes != null)
                        {
                            result += Encoding.Default.GetString(resultBytes);
                        }
                    }

                    TableHelper.AddCell(dataRow, result.ToString());


                    if (alternatingRowToggle)
                    {
                        dataRow.CssClass = "alt";
                    }

                    alternatingRowToggle = alternatingRowToggle == Convert.ToBoolean(0);
                    cacheTable.Rows.Add(dataRow);
                }
                cacheTable.RenderControl(this.writer);
            }


            if (displayUserCtlCache)
            {
                cacheTable = TableHelper.CreateTable();
                var mainHeadingRow = new TableRow();
                cacheTable.Rows.Add(mainHeadingRow);

                var mainHeading = TableHelper.AddHeaderCell(mainHeadingRow, "<h3><b>Parial Cache(UserControl) Details</b></h3>");
                mainHeading.ColumnSpan = 3;
                mainHeading.CssClass = "alt";

                var secondaryHeadingRow = new TableRow { CssClass = "subhead" };
                cacheTable.Rows.Add(secondaryHeadingRow);

                var alternatingRowToggle = false;
                foreach (object cachedItem in partialCachingCacheEntries)
                {
                    var dataRow = new TableRow();

                    var cachedOutputFieldInfo = cachedItem.GetType()
                        .GetField("OutputString", BindingFlags.NonPublic | BindingFlags.Instance);
                    var cachedOutputValue = cachedOutputFieldInfo.GetValue(cachedItem);

                    TableHelper.AddCell(dataRow, HttpUtility.HtmlEncode(cachedOutputValue.ToString()));

                    if (alternatingRowToggle)
                    {
                        dataRow.CssClass = "alt";
                    }

                    alternatingRowToggle = alternatingRowToggle == Convert.ToBoolean(0);
                    cacheTable.Rows.Add(dataRow);
                }
                cacheTable.RenderControl(this.writer);
            }
        }


        /// <summary>
        /// Standard HttpHandler Entry point. Coordinate the displaying of the Cache View
        /// </summary>
        /// <param name="context">The current HttpContext</param>
        public void ProcessRequest(HttpContext context)
        {

            if (RequestIsLocal(context.Request) == false)
            {
                context.AddError(new ApplicationException("CacheView can only be accessed locally i.e. localhost"));
                return;
            }

            if (ConfigurationManager.AppSettings["showOutputCache"] != null
                && ConfigurationManager.AppSettings["showOutputCache"] == "true")
            {
                this.displayOutputCache = true;
            }

            if (ConfigurationManager.AppSettings["showUserControlCache"] != null
                && ConfigurationManager.AppSettings["showUserControlCache"] == "true")
            {
                this.displayUserControlCache = true;
            }

            this.context = context;
            this.request = context.Request;
            this.response = context.Response;

            this.writer = new HtmlTextWriter(this.response.Output);

            this.writer.Write("<html>\r\n");

            // Write out Html head and style tags
            this.writer.Write("<head>\r\n");
            this.writer.Write(this.pageStyle);
            this.writer.Write("</head>\r\n");


            this.EnumerateAndDisplayInternalCache(this.displayOutputCache, this.displayUserControlCache);

            this.writer.Write("\r\n</body>\r\n</html>\r\n");
        }

        /// <summary>
        /// Gets a value indicating whether is reusable.
        /// </summary>
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }


        /// <summary>
        /// Check if current request originated locally
        /// </summary>
        /// <param name="request">The current HttpRequest</param>
        /// <returns>True if the request originated locally</returns>
        internal static bool RequestIsLocal(HttpRequest request)
        {
            if (request.UserHostAddress == "127.0.0.1"
                || request.UserHostAddress == request.ServerVariables["LOCAL_ADDR"])
            {
                return true;
            }

            return false;

        }
    }
}
