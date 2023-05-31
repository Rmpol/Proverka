using iTextSharp.text.pdf;
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

            // Получаем коллекцию строк со всех страниц PDF-файла
            List<string> mergeLinesAllPages = ExtractInformationFromPdfFile(filePath);

            // Получаем коллекцию опыта работы без лишней информации
            List<string> experience = GetExperience(mergeLinesAllPages);

            //Проверка на наличие стажа работы
            if (experience.Count > 0)
            {
                // Удаляем все элементы со значением "Резюме обновлено"
                RemoveResumeUpdates(experience);

                // Получаем общий стаж работы
                string totalWorkExperience = ProcessExperience(experience[0], experience);

                Console.WriteLine($"Общий стаж работы: {totalWorkExperience}\n");

                // Получаем информацию о последней работе
                List<string> informationOfWork = GetTimeAndPlaceOfWork(experience);

                // Получаем информацию о предпоследней работе
                List<string> informationOfWork2 = GetTimeAndPlaceOfWork(experience);

                // Получаем информацию о предпредпоследней работе
                List<string> informationOfWork3 = GetTimeAndPlaceOfWork(experience);
            }
            else
            {
                Console.WriteLine("Опыта работы нет, либо не был указан.");
            }
        }

        /// <summary>
        /// Извлекает информацию из PDF-файла.
        /// </summary>
        /// <param name="filePath">Путь к PDF-файлу.</param>
        /// <returns>Коллекция строк со всех страниц PDF-файла.</returns>
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
                Console.WriteLine("Ошибка при чтении PDF-файла: {0}", ex.Message);
            }

            return mergeLinesAllPages;
        }

        /// <summary>
        /// Получает коллекцию опыта работы без лишней информации.
        /// </summary>
        /// <param name="mergeLinesAllPages">Коллекция строк со всех страниц PDF-файла.</param>
        /// <returns>Коллекция опыта работы без лишней информации.</returns>
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
                    break; // Прерываем цикл, так как достигли элемента "Образование"
                }

                if (foundExperience && !foundEducation)
                {
                    experience.Add(line);
                }
            }

            return experience;
        }

        /// <summary>
        /// Обрабатывает строку с информацией об опыте работы и возвращает общий стаж работы.
        /// </summary>
        /// <param name="experienceLine">Строка с информацией об опыте работы.</param>
        /// <param name="experience">Коллекция опыта работы.</param>
        /// <returns>Общий стаж работы.</returns>
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

            // Удаляем строки из коллекции опыта работы
            experience = RemoveLines(experience, 0, 1);

            return totalWorkExperience;
        }

        /// <summary>
        /// Находит индекс первой цифры в строке.
        /// </summary>
        /// <param name="line">Строка для поиска индекса первой цифры.</param>
        /// <returns>Индекс первой цифры в строке, или -1, если цифра не найдена.</returns>
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

        /// <summary>
        /// Удаляет строки с информацией об обновлении резюме из коллекции опыта работы.
        /// </summary>
        /// <param name="experience">Коллекция опыта работы.</param>
        /// <returns>Коллекция опыта работы после удаления строк об обновлении резюме.</returns>
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

        /// <summary>
        /// Удаляет строки из коллекции строк.
        /// </summary>
        /// <param name="lines">Коллекция строк.</param>
        /// <param name="startIndex">Индекс начала удаления.</param>
        /// <param name="count">Количество строк для удаления.</param>
        /// <returns>Коллекция строк после удаления.</returns>
        static List<string> RemoveLines(List<string> lines, int startIndex, int count)
        {
            lines.RemoveRange(startIndex, count);
            return lines;
        }

        /// <summary>
        /// Получает информацию о времени и месте работы из коллекции опыта работы.
        /// </summary>
        /// <param name="experience">Коллекция опыта работы.</param>
        /// <returns>Коллекция строк с информацией о времени и месте работы.</returns>
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

        /// <summary>
        /// Находит индекс строки, содержащей год и месяц окончания работы.
        /// </summary>
        /// <param name="experience">Коллекция опыта работы.</param>
        /// <returns>Индекс строки с годом и месяцем окончания работы, или -1, если строка не найдена.</returns>
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
