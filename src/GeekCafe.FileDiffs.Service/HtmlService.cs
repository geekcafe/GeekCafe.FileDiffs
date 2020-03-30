using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DiffPlex;
using DiffPlex.DiffBuilder;
using System.Threading.Tasks;
using GeekCafe.FileDiffs.Service.Model;
using Microsoft.Extensions.Logging;

namespace GeekCafe.FileDiffs.Service
{
    public class HtmlService
    {
        private ILogger _loggerService = null;

        public async Task<List<string>> GenerateDiffFiles(Dictionary<string, FileCompareModel> diffs)
        {
            var files = new List<string>();

            var tempPath = GetTempPath();




            foreach (var diff in diffs)
            {
                string file = await GenerateDiffFile (tempPath, diff.Key, diff.Value);

                files.Add(file);
            }

            return files;

        }

        public async Task<string> GenerateDiffFile(string tempPath, string fileName, FileCompareModel diff)
        {
            
            var tempDiffFile = Path.Join(tempPath, fileName + ".html");

            // get the directory
            var p = Path.GetDirectoryName(tempDiffFile);
            Directory.CreateDirectory(p);

           
            using (var stream = File.CreateText(tempDiffFile))
            {
                // html header
                await stream.WriteAsync(GetHtmlStartBodyBlock());                
                // content
                await DiffTwoFilesAsync(fileName, diff, stream);
                // footer of html
                await stream.WriteAsync(GetHtmlEndBodyBlock());
            }

            return tempDiffFile;

        }


        private string GetTempPath()
        {
            var tempPath = Path.GetTempPath();
            tempPath = Path.Join(tempPath, DateTime.UtcNow.Ticks.ToString());

            Directory.CreateDirectory(tempPath);

            return tempPath;
        }

        public async Task<string> BuildSummaryAndDiffs(Dictionary<string, FileCompareModel> diffs)
        {
            var title = (diffs?.Count > 0) ? "The following files are included in this report" : "No files were available for diff comparisons.";
            var titleHtml = GetSectionTitle(title);

            var html = new StringBuilder();
            // build the header row
            var tempPath = GetTempPath();

            if (diffs?.Count > 0)
            {
                html.AppendLine(BuildRow(true, "#", "File"));
                var count = 0;
                foreach (var diff in diffs)
                {
                    count++;

                    string file = await GenerateDiffFile(tempPath, diff.Key, diff.Value);

                    var link = $"<a href=\"file://{file}\">{diff.Key}</a>";

                    html.AppendLine(BuildRow(count.ToString(), link));
                }
            }

             var finalHtml =  titleHtml + WrapTableDefinition(html.ToString());

            var summaryFile = $"{DateTime.UtcNow.Ticks}-summary.html";

            summaryFile = Path.Join(tempPath, summaryFile);

            using (var stream = File.CreateText(summaryFile))
            {
                // html header
                await stream.WriteAsync(GetHtmlStartBodyBlock());
                                
                // content
                await stream.WriteAsync(finalHtml);
                // footer of html
                await stream.WriteAsync(GetHtmlEndBodyBlock());
            }

            return summaryFile;


        }

        private async Task DiffTwoFilesAsync(string file, FileCompareModel diffViewModel, StreamWriter streamWriter)
        {
            var title = GetSectionTitle("Diff Comparison");

            var builder = new SideBySideDiffBuilder(new Differ());

            if (!File.Exists(diffViewModel.RightPath) && !File.Exists(diffViewModel.LeftPath))
            {
                // use the repository item path in the message
                var html = title + WrapTableDefinition(BuildRow($"Could not locate file {diffViewModel.RightPath}.  A Diff Comparison was not generated"));

                streamWriter.Write(html);
                return;
            }

            if (!File.Exists(diffViewModel.RightPath) || !File.Exists(diffViewModel.LeftPath))
            {
                var html = title + WrapTableDefinition(BuildRow("Only One Version of this file is available.  A Diff Comparison was not generated"));

                streamWriter.Write(html);
                return;

            }

            // add the repository items path for the sub title.
            title += GetSubSectionTitle(file);

            var leftText = "";
            var rightText = "";

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            using (var reader = File.OpenText(diffViewModel.LeftPath))
            {
                Log($"[AuditReports]:[Reading Left]:[Started]");
                leftText = await reader.ReadToEndAsync();
                Log($"[AuditReports]:[Reading Left]:[Completed]");
            }

            using (var reader = File.OpenText(diffViewModel.RightPath))
            {
                Log($"[AuditReports]:[Reading Right]:[Started]");
                rightText = await reader.ReadToEndAsync();
                Log($"[AuditReports]:[Reading Right]:[Completed]");
            }



            Log($"[AuditReports]:[BuildDiffModel]:[Started]");
            var diff = await Task.Run(() => { return builder.BuildDiffModel(leftText, rightText); });
            Log($"[AuditReports]:[BuildDiffModel]:[Completed]");
            var htmlBuilder = new Html.DiffHtml();




            var include = IncludeUnChangedText(diffViewModel.RightPath, diffViewModel.LeftPath);

            if (!include)
            {
                title += GetSubSectionTitle("Due to the size of the original files, the comparison will only show modified lines.");
            }

            await streamWriter.WriteLineAsync(title);


            Log($"[AuditReports]:[Build]:[Started]");
            await htmlBuilder.BuildAsync(diff, streamWriter, include);
            Log($"[AuditReports]:[Build]:[Completed]");

            Console.ForegroundColor = ConsoleColor.Green;
            sw.Stop();
            Log($"[AuditReports]:[{nameof(DiffTwoFilesAsync)}]:[Completed In]:[{ElapsedTime(sw.Elapsed)}]");
            Console.ResetColor();

        }


        private string ElapsedTime(TimeSpan ts)
        {
            return $"{ts.Days}:Days {ts.Hours}:Hours {ts.Minutes}:Minutes {ts.Seconds}:Seconds {ts.Milliseconds}:Milliseconds";
        }

        private void Log(string message)
        {
            if (_loggerService != null)
            {
                _loggerService?.LogInformation(message);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(message);
                Console.ResetColor();
            }
        }

        private bool IncludeUnChangedText(string leftPath, string rightPath)
        {
            var megs = 10;
            var max = 1024 * 1024 * megs;


            if (GetFileSize(leftPath) > max || GetFileSize(rightPath) > max)
            {
                return false;
            }

            return true;
        }

        private long GetFileSize(string path)
        {
            var fi = new FileInfo(path);
            var len = fi.Length;
            fi = null;
            return len;
        }


        

        private string WrapTableDefinition(string body) => $"<table class=\"summary\" >{body}</table>";

        private string GetSectionTitle(string title, string cssClass = "") => $"<h2 class=\"{cssClass}\">{title}</h2>";

        private string GetSubSectionTitle(string title, string cssClass = "") => $"<h3 class=\"{cssClass}\">{title}</h3>";

        

        private string BuildRow(bool header, params string[] columns)
        {
            var html = new StringBuilder();

            var cell = (header) ? "th" : "td";

            html.Append("<tr>");

            foreach (var column in columns)
            {
                html.Append($"<{cell}>{column}</{cell}>");
            }

            html.AppendLine("</tr>");

            return html.ToString();
        }

        private string BuildRow(params string[] columns)
        {
            return BuildRow(false, columns);
        }

        

        private string WrapHtmlTags(string body)
        {
            var html = $"{GetHtmlStartBodyBlock()} {body} {GetHtmlEndBodyBlock()}";

            return html;
        }


        private string GetHtmlStartBodyBlock()
        {
            var html = "<!DOCTYPE html>\n"
                    + "<html>\n"
                    + "<head>\n    "
                    + "<meta charset=\"utf-8\" />\n    "
                    + GetStyles()
                    + "</head>"
                    + $"<body>\n<div id=\"reportContent\">\n";

            return html;
        }

        private string GetHtmlEndBodyBlock()
        {
            var html = "\n</body></div></html>";

            return html;
        }
        private string GetStyles()
        {
            var styles = @"

                body { font-size: 16px; } 
                .reportContent { }
                
                .sectionTitle { margin-bottom: 25px;margin-top:25px; }

                table.summary { border-collapse: collapse; width:100%; margin-top:10px;margin-bottom:10px; }
                table.summary td, th { border: solid 1px black; padding: 8px;}
                #table.summary th:nth-child(even) {background-color:#dcdcdc }
                table.summary th { padding-top:12px; padding-bottom:12px; text-align:left; background-color:#dcdcdc;color:#000000 }

                table.diffTable { border: 1px solid #a0a0a0; margin: 10px 0px 10px 0px; width: 100%;  }
                table.diffTable td { vertical-align: top; padding: 0; }
                .lineText { min-width:40% }
                .lineNumber { padding: 0 .3em; background-color: #FFFFFF; text-align: right; }
                .insertedLine { background-color: #78ca5f; }
                .modifiedLine { background-color: #a5b2f1; }
                .deletedLine { background-color: #E86565; }
                .unchangedLine {background-color: #FFFFFF; }
                .imaginaryLine { background-color: #C8C8C8; }
                .insertedCharacter { 
                    background-color: #FFFF96;
                    white-space: -moz-pre-wrap !important;  /* Mozilla, since 1999 */
                    white-space: -webkit-pre-wrap; /*Chrome & Safari */ 
                    white-space: -pre-wrap;      /* Opera 4-6 */
                    white-space: -o-pre-wrap;    /* Opera 7 */
                    white-space: pre-wrap;       /* css-3 */
                    word-wrap: break-word;       /* Internet Explorer 5.5+ */
                    word-break: break-all;
                    white-space: normal;
                }

                .deletedCharacter { background-color: #C86464; }

                .changedCharacter {  }

                .imaginaryCharacter {  }

                .clear { clear: both; }
                ";

            return $"<style>{styles}</style>";
        }

        

    }
}
