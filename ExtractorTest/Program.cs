using System;

namespace ExtractorTest
{
    class Program
    {
        static void Main(string[] args)
        {
            String strContent = Article_Content_Extractor.ArticleContentExtractor.GetArticleContentFromURL("https://www.lepoint.fr/sante/vaccinodrome-du-stade-de-france-plus-de-limite-d-age-pour-les-metiers-prioritaires-29-04-2021-2424298_40.php");
            Console.WriteLine("Content found :\n" + strContent);
            Console.ReadKey();
        }
    }
}
