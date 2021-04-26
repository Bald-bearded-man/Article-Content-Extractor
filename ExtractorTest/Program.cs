using System;

namespace ExtractorTest
{
    class Program
    {
        static void Main(string[] args)
        {
            String strContent = Article_Content_Extractor.ArticleContentExtractor.GetArticleContentFromURL("https://www.20minutes.fr/societe/3027947-20210423-retenir-experience-deep-time-apres-40-jours-grotte");
            Console.WriteLine("Content found :\n" + strContent);
            Console.ReadKey();
        }
    }
}
