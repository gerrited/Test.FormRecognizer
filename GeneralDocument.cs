using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;

public class GeneralDocument
{
    private FormRecognizerOptions _formRecognizerOptions;

    public GeneralDocument(FormRecognizerOptions formRecognizerOptions)
    {
        _formRecognizerOptions = formRecognizerOptions;
    }

    public async Task Analyze(string url)
    {
        AzureKeyCredential credential = new AzureKeyCredential(_formRecognizerOptions.Key);
        DocumentAnalysisClient client = new DocumentAnalysisClient(new Uri(_formRecognizerOptions.Endpoint), credential);

        Uri fileUri = new Uri(url);

        AnalyzeDocumentOperation operation = await client.StartAnalyzeDocumentFromUriAsync("prebuilt-document", fileUri);

        await operation.WaitForCompletionAsync();

        AnalyzeResult result = operation.Value;

        Console.WriteLine("Detected key-value pairs:");

        foreach (DocumentKeyValuePair kvp in result.KeyValuePairs)
        {
            if (kvp.Value == null)
            {
                Console.WriteLine($"  Found key with no value: '{kvp.Key.Content}'");
            }
            else
            {
                Console.WriteLine($"  Found key-value pair: '{kvp.Key.Content}' and '{kvp.Value.Content}'");
            }
        }

        foreach (DocumentPage page in result.Pages)
        {
            Console.WriteLine($"Document Page {page.PageNumber} has {page.Lines.Count} line(s), {page.Words.Count} word(s),");
            Console.WriteLine($"and {page.SelectionMarks.Count} selection mark(s).");

            for (int i = 0; i < page.Lines.Count; i++)
            {
                DocumentLine line = page.Lines[i];
                Console.WriteLine($"  Line {i} has content: '{line.Content}'.");

                Console.WriteLine($"    Its bounding box is:");
                Console.WriteLine($"      Upper left => X: {line.BoundingPolygon[0].X}, Y= {line.BoundingPolygon[0].Y}");
                Console.WriteLine($"      Upper right => X: {line.BoundingPolygon[1].X}, Y= {line.BoundingPolygon[1].Y}");
                Console.WriteLine($"      Lower right => X: {line.BoundingPolygon[2].X}, Y= {line.BoundingPolygon[2].Y}");
                Console.WriteLine($"      Lower left => X: {line.BoundingPolygon[3].X}, Y= {line.BoundingPolygon[3].Y}");
            }

            for (int i = 0; i < page.SelectionMarks.Count; i++)
            {
                DocumentSelectionMark selectionMark = page.SelectionMarks[i];

                Console.WriteLine($"  Selection Mark {i} is {selectionMark.State}.");
                Console.WriteLine($"    Its bounding box is:");
                Console.WriteLine($"      Upper left => X: {selectionMark.BoundingPolygon[0].X}, Y= {selectionMark.BoundingPolygon[0].Y}");
                Console.WriteLine($"      Upper right => X: {selectionMark.BoundingPolygon[1].X}, Y= {selectionMark.BoundingPolygon[1].Y}");
                Console.WriteLine($"      Lower right => X: {selectionMark.BoundingPolygon[2].X}, Y= {selectionMark.BoundingPolygon[2].Y}");
                Console.WriteLine($"      Lower left => X: {selectionMark.BoundingPolygon[3].X}, Y= {selectionMark.BoundingPolygon[3].Y}");
            }
        }

        foreach (DocumentStyle style in result.Styles)
        {
            // Check the style and style confidence to see if text is handwritten.
            // Note that value '0.8' is used as an example.

            bool isHandwritten = style.IsHandwritten.HasValue && style.IsHandwritten == true;

            if (isHandwritten && style.Confidence > 0.8)
            {
                Console.WriteLine($"Handwritten content found:");

                foreach (DocumentSpan span in style.Spans)
                {
                    Console.WriteLine($"  Content: {result.Content.Substring(span.Offset, span.Length)}");
                }
            }
        }

        Console.WriteLine("The following tables were extracted:");

        for (int i = 0; i < result.Tables.Count; i++)
        {
            DocumentTable table = result.Tables[i];
            Console.WriteLine($"  Table {i} has {table.RowCount} rows and {table.ColumnCount} columns.");

            foreach (DocumentTableCell cell in table.Cells)
            {
                Console.WriteLine($"    Cell ({cell.RowIndex}, {cell.ColumnIndex}) has kind '{cell.Kind}' and content: '{cell.Content}'.");
            }
        }
    }
}