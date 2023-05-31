﻿using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Proverka
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string filePath = @"C:\Users\Admin\Desktop\Рабочая\for me\C# developer Resume.pdf";

            //Получаем коллекцию состоящую из строк со всех страниц pdf файла
            List<string> mergeLinesAllPages = ExtractInformationFromPdfFile(filePath);

            //Получаем коллекцию без лишней информации
            List<string> experience = GetExperience(mergeLinesAllPages);

            if(experience.Count > 0)
            {
                //удаляем все элементы со значением "Резюме обновлено"
                RemoveResumeUpdates(experience);

                //Получаем общий стаж работы
                string totalWorkExperience = ProcessExperience(experience[0], experience);

                Console.WriteLine($"Общий стаж работы: {totalWorkExperience}\n");

                //получаем информацию о последней работе
                List<string> informationOfWork = GetTimeAndPlaceOfWork(experience);

                //получаем информацию о предпоследней работе
                List<string> informationOfWork2 = GetTimeAndPlaceOfWork(experience);

                //получаем информацию о предпоследней работе
                List<string> informationOfWork3 = GetTimeAndPlaceOfWork(experience);
            }

            else

                Console.WriteLine("Опыта работы нет, либо не был указан.");
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

        static string ProcessExperience(string experienceLine, List<string> experience)
        {
            string totalWorkExperience = "";

            if (experienceLine.StartsWith("Опыт работы"))
            {
                int startIndex = FindFirstDigitIndex(experienceLine);

                if (startIndex != -1 && startIndex < experienceLine.Length)
                {
                    totalWorkExperience = experienceLine.Substring(startIndex);
                }
            }
            else
            {
                Console.WriteLine("Опыта работы нет, либо не был указан.");
                return null;
            }

            // Удаляем строки из списка experience
            experience = RemoveLines(experience, 0, 1);

            return totalWorkExperience;
        }


        static int FindFirstDigitIndex(string line)
        {
            for (int i = 0; i < line.Length; i++)
            {
                if (char.IsDigit(line[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        public static List<string> RemoveResumeUpdates(List<string> experience)
        {
            List<string> itemsToRemove = experience.Where(item => item.StartsWith("Резюме обновлено")).ToList();
            if (itemsToRemove.Count > 0)
            {
                foreach (string item in itemsToRemove)
                {
                    experience.Remove(item);
                }
            }
            return experience;
        }


        static List<string> RemoveLines(List<string> lines, int startIndex, int count)
        {
            lines.RemoveRange(startIndex, count);
            return lines;
        }

        public static List<string> GetTimeAndPlaceOfWork(List<string> experience)
        {
            List<string> informationOfWork = new List<string>();

            if (experience.Count >= 4)
            {
                string startWork = experience[0];
                string companyName = experience[1];
                string endWork = experience[2];
                string generalInformation = string.Empty;

                int endIndex = FindYearMonthIndex(experience);
                if (endIndex < 0)
                {
                    endIndex = experience.Count;
                }

                generalInformation = string.Join(" ", experience.GetRange(3, endIndex - 3));

                informationOfWork.Add(startWork);
                informationOfWork.Add(companyName);
                informationOfWork.Add(endWork);
                informationOfWork.Add(generalInformation);

                RemoveLines(experience, 0, endIndex);

                Console.WriteLine("Название компании: {0}\nНачало работы: {1}\nОкончание работы: {2}\nОбщая информация: {3}\n", companyName, startWork, endWork, generalInformation);

            }
            else
            {
                Console.WriteLine("Недостаточно данных для обработки опыта работы.");
            }

            return informationOfWork;
        }

        public static int FindYearMonthIndex(List<string> experience)
        {
            Regex regex = new Regex(@"^[А-Яа-я]+\s\d{4}");

            for (int i = 3; i < experience.Count; i++)
            {
                if (regex.IsMatch(experience[i]))
                {
                    return i;
                }
            }

            return -1;
        }

    }
}