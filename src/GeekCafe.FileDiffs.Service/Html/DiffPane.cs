using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DiffPlex.DiffBuilder.Model;

namespace GeekCafe.FileDiffs.Service.Html
{
    internal class DiffPane
    {

        private bool _includeUnchanged = false;

        public DiffPane(bool includeUnchanged)
        {
            _includeUnchanged = includeUnchanged;
        }

        public async Task BuildAsync(DiffPaneModel model, StreamWriter streamWriter)
        {
            streamWriter.WriteLine("<div class=\"diffPane\">");
            streamWriter.WriteLine("<table cellpadding = \"0\" cellspacing=\"0\" class=\"diffTable\">");

            await BuildLinesAsync(model, streamWriter);

            streamWriter.WriteLine("</table>");
            streamWriter.WriteLine("</div>");

        }

        public async Task BuildAsync(SideBySideDiffModel sideBySideDiffModel, StreamWriter streamWriter)
        {
            streamWriter.Write("<div class=\"diffPane\">"
                + "<table cellpadding = \"0\" cellspacing=\"0\" class=\"diffTable\">"
                    + "<colgroup>"
                        + "<col style=\"width: 5em;\">\n"
                        + "<col style=\"width: 1em;\">\n"
                        + "<col style=\"min-width:40%; max-width:50%;width:45%\">\n"
                        + "<col style=\"width: 1em;\">\n"
                        + "<col style=\"width: 5em;\">\n"
                        + "<col style=\"width: 1em;\">\n"
                        + "<col style=\"min-width:40%; max-width:50%;width:45%\">\n"
                        + "<col style=\"width: 1em;\">\n"
                    + "</colgroup>"
                    + $"<tbody>");


            await BuildLinesAsync(sideBySideDiffModel, streamWriter);

            await streamWriter.WriteLineAsync("</tbody>" + "</table>" + "</div>");


        }

        private async Task BuildLinesAsync(DiffPaneModel model, StreamWriter streamWriter)
        {

            foreach (var line in model.Lines)
            {
                var data = WrapRow(BuildTableCells(line));
                if (!string.IsNullOrWhiteSpace(data))
                {
                    await streamWriter.WriteLineAsync(data);
                }
            }
        }

        private async Task BuildLinesAsync(SideBySideDiffModel model, StreamWriter streamWriter)
        {

            for (var i = 0; i < model.OldText.Lines.Count; i++)
            {
                var data = WrapRow(BuildBlock(model.OldText.Lines[i], model.NewText.Lines[i]));
                if (!string.IsNullOrWhiteSpace(data))
                {
                    await streamWriter.WriteLineAsync(data);
                }
            }

        }

        private string BuildBlock(DiffPiece leftPiece, DiffPiece rightPiece)
        {
            var leftSet = BuildTableCells(leftPiece);
            var rightSet = BuildTableCells(rightPiece);

            var html = WrapRow(leftSet + rightSet);

            return html;
        }

        private string WrapRow(string tableCells)
        {
            if (string.IsNullOrWhiteSpace(tableCells))
            {
                return "";
            }

            return $"<tr>{tableCells}</tr>";
        }

        private string BuildTableCells(DiffPiece diffLine)
        {
            var lineNumber = (diffLine.Position.HasValue) ? diffLine.Position.ToString() : "&nbsp;";
            var lineText = BuildLineText(diffLine);

            if (string.IsNullOrWhiteSpace(lineText))
            {
                return "";
            }

            string html = ""
                    + $"<td class=\"lineNumber\">{lineNumber}</td>"
                    + "<td>&nbsp;</td>"
                    + $"<td class=\"line {ToCamelCase(diffLine.Type.ToString())}Line\">"
                        + "<span class=\"lineText\">"
                            + lineText
                        + "</span>"
                    + "</td>"
                    + "<td></td>";

            return html;
        }

        private string BuildLineText(DiffPiece diffLine)
        {
            var sb = new StringBuilder();

            string spaceValue = WebUtility.HtmlEncode("\u00B7");
            string tabValue = spaceValue + spaceValue;

            // adding some spacing so that word wrap works correctly
            spaceValue = $" {spaceValue} ";
            tabValue = $" {tabValue} ";

            if (!string.IsNullOrEmpty(diffLine.Text))
            {
                if (diffLine.Type == ChangeType.Deleted || diffLine.Type == ChangeType.Inserted || (diffLine.Type == ChangeType.Unchanged && _includeUnchanged))
                {


                    string raw = (WebUtility.HtmlEncode(diffLine.Text).Replace(" ", spaceValue).Replace("\t", tabValue));
                    sb.Append(raw);
                }
                else if (diffLine.Type == ChangeType.Modified)
                {
                    foreach (var character in diffLine.SubPieces)
                    {
                        if (character.Type == ChangeType.Imaginary) { continue; }

                        var html = $"<span class=\"{ToCamelCase(character.Type.ToString())}Character piece\">{character.Text.Replace(" ", spaceValue.ToString())}</span>";

                        sb.Append(html);
                    }
                }
            }

            return sb.ToString();
        }

        private string ToCamelCase(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (value.Length == 1)
                {
                    return char.ToLowerInvariant(value[0]).ToString();
                }
                return char.ToLowerInvariant(value[0]) + value.Substring(1);
            }
            return value;
        }
    }
}
