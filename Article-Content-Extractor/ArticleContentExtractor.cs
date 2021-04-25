using System;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Article_Content_Extractor
{
    public class ArticleContentExtractor
    {
        /// <summary>
        /// Extract content from HTML fetched from URL
        /// </summary>
        /// <param name="strURL"></param>
        /// <returns></returns>
        public static String GetArticleContentFromURL(String strURL)
        {
            HttpClient hc = new HttpClient();
            String strContent = hc.GetStringAsync(strURL).GetAwaiter().GetResult();
            return GetArticleContentFromRawHTML(strContent);
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
            string strCleanedContent = "";
            foreach (String strLine in strContent.Split('\n'))
                if (!String.IsNullOrWhiteSpace(strLine))
                    strCleanedContent += strLine + "\n";
            return LookingForContent(strCleanedContent);
        }


        private static String RemoveScripts(String strContent)
        {
            bool blnAtLeastOneChange = false;
            foreach (Match m in Regex.Matches(strContent, "<script.*>((?!<script>)[.\\s\\S])*<\\/script>"))
            {
                strContent = strContent.Replace(m.Value, "");
                blnAtLeastOneChange = true;
            }
            if (blnAtLeastOneChange)
                strContent = RemoveScripts(strContent);
            return strContent;
        }

        private static String RemoveHeaders(String strContent)
        {
            bool blnAtLeastOneChange = false;
            foreach (Match m in Regex.Matches(strContent, "<head>[.\\s\\S]*</head>"))
            {
                strContent = strContent.Replace(m.Value, "");
                blnAtLeastOneChange = true;
            }
            if (blnAtLeastOneChange)
                strContent = RemoveHeaders(strContent);
            return strContent;
        }

        private static String RemoveSoloTags(String strContent)
        {
            bool blnAtLeastOneChange = false;
            foreach (Match m in Regex.Matches(strContent, "<[a-zA-Z]+[\\w\\t\\r\\n\\s\"'’   \\/:\\-\\.= &*#;,À-ÿ\\(\\)+%«»?]+[\\/]>"))
            {
                strContent = strContent.Replace(m.Value, "");
                blnAtLeastOneChange = true;
            }
            if (blnAtLeastOneChange)
                strContent = RemoveSoloTags(strContent);
            return strContent;
        }

        private static String CleanAttributes(String strContent)
        {
            bool blnAtLeastOneChange = false;
            foreach (Match m in Regex.Matches(strContent, "<[a-zA-Z]+ ([\\w\\r\\n\\s\\\"'’ \\/:\\-\\.= &*#;,À-ÿ\\(\\)+%«»?]+)>"))
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
            bool blnAtLeastOneChange = false;
            foreach (Match m in Regex.Matches(strContent, @"<[A-Z-a-z0-9]+[\s\r\n]*>[\s\r\n]*<\/[A-Z-a-z0-9]+[\s\r\n]*>"))
            {
                strContent = strContent.Replace(m.Value, "");
                blnAtLeastOneChange = true;
            }
            if (blnAtLeastOneChange)
                strContent = RemoveEmptyTags(strContent);
            return strContent;
        }

        private static String RemoveComments(String strContent)
        {
            bool blnAtLeastOneChange = false;
            foreach (Match m in Regex.Matches(strContent, @"<!--((?!-->).)*-->"))
            {
                strContent = strContent.Replace(m.Value, "");
                blnAtLeastOneChange = true;
            }
            if (blnAtLeastOneChange)
                strContent = RemoveComments(strContent);
            return strContent;
        }

        private static String RemoveUnwantedTags(String strContent)
        {
            bool blnAtLeastOneChange = false;
            foreach (Match m in Regex.Matches(strContent, @"(<button[\s\r\n]*>((?!<\/button)([\r\n]|.))*<\/button[\s\r\n]*>)|(<nav[\s\r\n]*>((?!<\/nav)([\r\n]|.))*<\/nav[\s\r\n]*>)"))
            {
                strContent = strContent.Replace(m.Value, "");
                blnAtLeastOneChange = true;
            }
            if (blnAtLeastOneChange)
                strContent = RemoveUnwantedTags(strContent);
            return strContent;
        }

        private static String RemovingUnusedTags(String strContent)
        {
            strContent = Regex.Replace(strContent, @"<p[\s\r\n]*>", "");
            strContent = Regex.Replace(strContent, @"<\/p[\s\r\n]*>", "\n");
            strContent = Regex.Replace(strContent, @"<\/?(a|em)[\s\r\n]*>", "");
            strContent = Regex.Replace(strContent, @"<h[0-9][\s\r\n]*>", "");
            strContent = Regex.Replace(strContent, @"<\/h[0-9][\s\r\n]*>", "\n");
            return strContent;
        }

        private static String LookingForContent(String strContent)
        {
            Decimal decBestScore = 0;
            String strBestContent = "";
            foreach (Match mBegin in Regex.Matches(strContent, @"<[\w]+[\r\n\s]*>"))
            {
                String strSelectedContent = strContent.Substring(mBegin.Index + mBegin.Value.Length);
                foreach (Match mEnd in Regex.Matches(strSelectedContent, @"<\/[\w]+[\r\n\s]*>"))
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
            strContent = Regex.Replace(strContent, @"\s{2,}", "");

            if (intNbBalise <= 3)
                return 0;
            else
                return strContent.Length / (decimal)intNbBalise;
        }
    }
}
