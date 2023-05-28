using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Proverka
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string filePath = @"C:\Users\User\Desktop\33.pdf";
            List<string> mergeLinesAllPages = ExtractInformationFromPdfFile(filePath);
            List<string> experience = GetExperience(mergeLinesAllPages);

            // Выводим значения коллекции experience
            for (int i = 0; i < experience.Count; i++)
            {
                Console.WriteLine("Index: " + i);
                Console.WriteLine("Value: " + experience[i]);
            }
        }

        static List<string> ExtractInformationFromPdfFile(string filePath)
        {
            List<string> mergeLinesAllPages = new List<string>();

            try
            {
                using (PdfReader reader = new PdfReader(filePath))
                {
                    int pageCount = reader.NumberOfPages;

                    for (int page = 1; page <= pageCount; page++)
                    {
                        string pageText = PdfTextExtractor.GetTextFromPage(reader, page);
                        string[] lines = pageText.Split(new[] { Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries);

                        mergeLinesAllPages.AddRange(lines);
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("Error reading the PDF file: {0}", ex.Message);
            }

            return mergeLinesAllPages;
        }

        static List<string> GetExperience(List<string> mergeLinesAllPages)
        {
            List<string> experience = new List<string>();
            bool foundExperience = false;
            bool foundEducation = false;

            foreach (var line in mergeLinesAllPages)
            {
                if (line.StartsWith("Опыт работы"))
                {
                    foundExperience = true;
                    foundEducation = false;
                    experience.Clear(); // Очищаем предыдущие элементы опыта работы
                }
                else if (line.StartsWith("Образование"))
                {
                    foundEducation = true;
                    break; // Прерываем цикл, так как мы достигли элемента "Образование"
                }

                if (foundExperience && !foundEducation)
                {
                    experience.Add(line);
                }
            }

            return experience;
        }

    }
}