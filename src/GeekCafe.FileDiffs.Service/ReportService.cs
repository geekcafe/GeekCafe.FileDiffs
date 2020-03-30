using System;
using System.IO;
using System.Linq;

namespace GeekCafe.FileDiffs.Service
{
    public class ReportService
    {
        public ReportService()
        {
        }

        public void GenerateAsync(DirectoryCompareService dcs)
        {
            // only get the ones with a diff
            var list = dcs.Files.Where(m => !m.Value.IsEqual).ToDictionary(o => o.Key, o => o.Value);
           
            var html = new HtmlService();

            var files = html.GenerateDiffFiles(list).Result;

            var summaryFile = html.BuildSummaryAndDiffs(list).Result;

            foreach (var file in files)
            {

                Console.WriteLine($"Generated Diff Files: {file}");
            }

            Console.WriteLine($"Generated Summary Diff File: {summaryFile}");
        }
    }
}
