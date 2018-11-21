using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PdfConcatCLI
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                Console.WriteLine("PdfConcat Version {0}", version);
                Console.WriteLine("Usage: PdfConcatCLI <inputFile1> <inputFile2> <inputFile3> ... <outputFile>");
                Console.WriteLine("OR");
                Console.WriteLine("Usage: PdfConcatCLI -w <inputWildcard> <outputFile>");
                return 1;
            }

            string[] files;

            if (
                args.Length == 3
                &&
                args[0] == "-w"
            )
            {
                string currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                files = Directory.GetFiles(currentDirectory, args[1], SearchOption.TopDirectoryOnly);
            }
            else
                files = args.Take(args.Length - 1).ToArray();

            string outputFiles = args.Last();

            DoMerge(files, outputFiles);

            return 0;
        }

        static void CopyPages(PdfDocument from, PdfDocument to)
        {
            for (int i = 0; i < from.PageCount; i++)
                to.AddPage(from.Pages[i]);
        }

        static void DoMerge(string[] files, string outputFileName)
        {
            var docs = new List<PdfDocument>();
            var outputDoc = new PdfDocument();

            foreach (string file in files)
                docs.Add(PdfReader.Open(file, PdfDocumentOpenMode.Import));

            Console.WriteLine($"Total files: {files.Length}");

            int progress = 1;

            foreach (var doc in docs)
            {
                CopyPages(doc, outputDoc);
                doc.Dispose();
                Console.WriteLine("Processing file {0} of {1}: {2}", progress, files.Length, files[progress - 1]);
                progress++;
            }

            outputDoc.Save(outputFileName);
            outputDoc.Dispose();

            docs.Clear();
            docs = null;
            GC.Collect();

            Console.WriteLine($"Successfully merged {files.Length} PDF files to {outputFileName}!");
        }
    }
}
