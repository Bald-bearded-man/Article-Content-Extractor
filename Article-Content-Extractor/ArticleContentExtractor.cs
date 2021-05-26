using System;
using System.Net;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Web;

namespace Article_Content_Extractor
{
    public class ArticleContentExtractor
    {
        #region Events

        public static event ContentExtractedDelegate OnContentExtracted;

        #endregion

        #region Public methods

        /// <summary>
        /// Extract content from HTML fetched from URL
        /// </summary>
        /// <param name="strURL"></param>
        /// <returns></returns>
        public static String GetArticleContentFromURL(String strURL)
        {
            WebClient wc = new WebClient();
            String strContent = wc.DownloadString(strURL);
            return GetArticleContentFromRawHTML(strContent);
        }

        /// <summary>
        /// Extract content from HTML fetched from URL
        /// </summary>
        /// <param name="strURL"></param>
        /// <returns></returns>
        public static void GetArticleContentFromURLAsync(String strURL)
        {
            WebClient wc = new WebClient();
            wc.DownloadStringCompleted += wc_DownloadStringCompleted; ;
            wc.DownloadStringAsync(new Uri(strURL));
        }

        /// <summary>
        /// Extract content from a raw HTML document
        /// </summary>
        /// <param name="strRawHTMLContent"></param>
        /// <returns></returns>
        public static String GetArticleContentFromRawHTML(String strRawHTMLContent)
        {
            String strContent = RemoveScripts(strRawHTMLContent);
            strContent = RemoveHeaders(strContent);
            strContent = RemoveSoloTags(strContent);
            strContent = CleanAttributes(strContent);
            strContent = RemoveEmptyTags(strContent);
            strContent = RemoveComments(strContent);
            strContent = RemoveUnwantedTags(strContent);
            strContent = RemovingUnusedTags(strContent);
            strContent = RemoveSuccessiveTags(strContent);
            strContent = LookingForContent(strContent);
            strContent = OptimiseContent(strContent);
            return RemoveEmptyLines(strContent);
        }

        #endregion

        #region Private methods

        private static void wc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            OnContentExtracted(null, new ContentExtractedEventArgs(GetArticleContentFromRawHTML(e.Result)));
        }

        private static String RemoveScripts(String strContent)
        {
            return Regex.Replace(strContent, @"<script.*>((?!<\/?script>)[.\s\S])*<\/script>", "");
        }

        private static String RemoveHeaders(String strContent)
        {
            return Regex.Replace(strContent, "<head>[.\\s\\S]*</head>", "");
        }

        private static String RemoveSoloTags(String strContent)
        {
            return Regex.Replace(strContent, "<[a-zA-Z]+[\\w\\t\\r\\n\\s\"'’   \\/:\\-\\.= &*#;,À-ÿ\\(\\)+%«»?|]+[\\/]>", "");
        }

        private static String CleanAttributes(String strContent)
        {
            bool blnAtLeastOneChange = false;
            foreach (Match m in Regex.Matches(strContent, "<[a-zA-Z]+( [\\w\\r\\n\\s\\\"'’ \\/:\\-\\.= &*#;,À-ÿ\\(\\)+%«»?|]+)>"))
            {
                strContent = strContent.Replace(m.Groups[1].Value, "");
                blnAtLeastOneChange = true;
            }
            if (blnAtLeastOneChange)
                strContent = CleanAttributes(strContent);
            return strContent;
        }

        private static String RemoveEmptyTags(String strContent)
        {
            return Regex.Replace(strContent, @"<[A-Z-a-z0-9]+[\s\r\n]*>[\s\r\n]*<\/[A-Z-a-z0-9]+[\s\r\n]*>", "");
        }

        private static String RemoveComments(String strContent)
        {
            return Regex.Replace(strContent, @"<!--((?!-->).)*-->", "");
        }

        private static String RemoveUnwantedTags(String strContent)
        {
            return Regex.Replace(strContent, @"(<button[\s\r\n]*>((?!<\/button)([\r\n]|.))*<\/button[\s\r\n]*>)|(<nav[\s\r\n]*>((?!<\/nav)([\r\n]|.))*<\/nav[\s\r\n]*>)", "");
        }

        private static String RemovingUnusedTags(String strContent)
        {
            strContent = Regex.Replace(strContent, @"<p[\s\r\n]*>", "");
            strContent = Regex.Replace(strContent, @"<\/(p|li|h[0-9])[\s\r\n]*>", " \n");
            strContent = Regex.Replace(strContent, @"<\/?(a|em)[\s\r\n]*>", "");
            strContent = Regex.Replace(strContent, @"<h[0-9][\s\r\n]*>", "");
            return strContent;
        }

        private static String RemoveSuccessiveTags(String strContent)
        {
            bool blnAtLeastOneChange = false;
            foreach (Match m in Regex.Matches(strContent, @"<\/?[A-Z-a-z0-9]+[\s\r\n]*>[\s\r\n]*(<\/?[A-Z-a-z0-9]+[\s\r\n]*>[\s\r\n]*)+"))
            {
                if (Regex.IsMatch(strContent, @"<\/p[\s\r\n]*>"))
                    strContent = strContent.Replace(m.Value, " \n");
                else
                    strContent = strContent.Replace(m.Value, "<blank>");
                blnAtLeastOneChange = true;
            }
            if (blnAtLeastOneChange)
                strContent = RemoveSuccessiveTags(strContent);
            return strContent;
        }

        private static String RemoveEmptyLines(String strContent)
        {
            string strCleanedContent = "";
            bool blnLastWasEmpty = false;
            foreach (String strLine in strContent.Split('\n'))
            {
                if (!String.IsNullOrWhiteSpace(strLine))
                {
                    strCleanedContent += strLine + "\n";
                    blnLastWasEmpty = false;
                }
                else if (!blnLastWasEmpty)
                {
                    strCleanedContent += strLine + "\n";
                    blnLastWasEmpty = true;
                }
            }
            strCleanedContent = Regex.Replace(strCleanedContent, @"[ ]{2,}", " ");
            return strCleanedContent;
        }

        private static String LookingForContent(String strContent)
        {
            Decimal decBestScore = 0;
            String strBestContent = "";
            foreach (Match mBegin in Regex.Matches(strContent, @"<[\w]+[\r\n\s]*>"))
            {
                String strSelectedContent = strContent.Substring(mBegin.Index + mBegin.Value.Length);
                foreach (Match mEnd in Regex.Matches(strSelectedContent, @"<\/?[\w]+[\r\n\s]*>"))
                {
                    String strFinalSelection = strSelectedContent.Substring(0, mEnd.Index);
                    Decimal decScore = GetScore(strFinalSelection);
                    if (decScore > decBestScore)
                    {
                        decBestScore = decScore;
                        strBestContent = strFinalSelection;
                    }
                }
            }
            return strBestContent;
        }

        private static Decimal GetScore(String strContent)
        {
            int intNbBalise = 0;
            foreach (Match m in Regex.Matches(strContent, @"<\/?[\w]+[\r\n\s]*>"))
            {
                intNbBalise++;
                strContent = strContent.Replace(m.Value, "");
            }

            if (intNbBalise <= 2)
                return 0;
            else
                return strContent.Length / (decimal)intNbBalise;
        }

        private static String OptimiseContent(String strContent)
        {
            strContent =  Regex.Replace(strContent, @"<\/?[\w]+[\r\n\s]*>", " \n");
            return HttpUtility.HtmlDecode(strContent);
        }

        #endregion

        #region Events

        public delegate void ContentExtractedDelegate(object sender, ContentExtractedEventArgs args);

        public class ContentExtractedEventArgs : EventArgs
        {
            public String Content { get; set; }

            public ContentExtractedEventArgs(String strContent)
            {
                Content = strContent;
            }
        }

        #endregion
    }
}
