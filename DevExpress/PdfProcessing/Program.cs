using System.IO;
using DevExpress.Pdf;


var pdfStream = File.OpenRead("./TestDokumentMit2Signaturfields.pdf");
var processor = new PdfDocumentProcessor();

// Load a PDF document with AcroForm data
processor.LoadDocument(pdfStream);

// Access the AcroForm and its fields
PdfDocumentFacade documentFacade = processor.DocumentFacade;
PdfAcroFormFacade acroForm = documentFacade.AcroForm;

var fields = acroForm.GetFields();
foreach (var formField in fields)
{
    Console.WriteLine($"Field Name: {formField.FullName}");
}


// using var PdfDocumentProcessor = new PdfDocumentProcessor();
// PdfDocumentProcessor.LoadDocument("blank.pdf");

// PdfAcroFormSignatureField signatureField = new("Signature1", 1, new PdfRectangle(100, 100, 200, 150));
// PdfAcroFormSignatureField signatureField2 = new("Signature2", 1, new PdfRectangle(100, 100, 200, 150));
// PdfAcroFormSignatureField signatureField3 = new("Signature3", 1, new PdfRectangle(100, 100, 200, 150));

// // Check whether new form fields' names already exist in the document
// IList<PdfAcroFormFieldNameCollision> collisions =
//      PdfDocumentProcessor.CheckFormFieldNameCollisions(signatureField, signatureField2, signatureField3);
// if (collisions.Count == 0)
//     Console.WriteLine("No name conflicts are detected");
// else
// {
//     foreach (var collision in collisions)
//     {
//         // Rename conflicting field
//         Console.WriteLine("The specified form field name ({0}) already exist in the document. Renaming...",
//            collision.Field.Name);
//         while (collision.ForbiddenNames.Contains(collision.Field.Name))
//             collision.Field.Name = Guid.NewGuid().ToString();
//     }
// }
// PdfDocumentProcessor.AddFormFields(signatureField);
// PdfDocumentProcessor.SaveDocument("3_signatures.pdf");
