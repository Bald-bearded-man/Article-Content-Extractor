using System;

namespace ExtractorTest
{
    class Program
    {
        static void Main(string[] args)
        {
            String strContent = Article_Content_Extractor.ArticleContentExtractor.GetArticleContentFromURL("http://www.legorafi.fr/2021/04/21/les-connards-annoncent-la-creation-dune-super-league-de-connards/");
            Console.WriteLine("Content found :\n" + strContent);
            Console.ReadKey();
        }
    }
}
