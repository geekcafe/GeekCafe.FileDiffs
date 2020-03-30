using System;

namespace GeekCafe.FileDiffs.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var diffService = new Service.DirectoryCompareService();

            diffService.Compare("/Users/eric.wilson/projects/GeekCafe/GeekCafe.FileDiffs/tests/diff_paths/dir1",
                "/Users/eric.wilson/projects/GeekCafe/GeekCafe.FileDiffs/tests/diff_paths/dir2"
                );

            if (diffService.IsDifferent())
            {

                var reportSevice = new Service.ReportService();

                reportSevice.GenerateAsync(diffService);
            }




           
        }
    }
}
