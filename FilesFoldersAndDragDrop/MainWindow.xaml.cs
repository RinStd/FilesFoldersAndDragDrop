using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FilesFoldersAndDragDrop
{
    public partial class MainWindow : Window
    {
        public string[] files;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void FiledropStackPanel_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                files = (string[])e.Data.GetData(DataFormats.FileDrop);

                FilesNameStackPanel.HorizontalAlignment = HorizontalAlignment.Left;
                FilesNameStackPanel.Content = $"Amount files: {files.Length} \n\r";
                int i = 1;
                foreach (string file in files)
                {
                    FilesNameStackPanel.Content += $"{i}) {System.IO.Path.GetFileName(file)} \n";
                    i++;
                }
            }
        }

        private void FileRewriteButton_Click(object sender, RoutedEventArgs e)
        {
            string dirTemp = @"C:\dirTemp";

            if (!Directory.Exists(dirTemp))
            {
                Directory.CreateDirectory(dirTemp);
            }

            foreach (string file in files)
            {
                string readPath = System.IO.Path.GetFullPath(file); // Полный путь к файлу
                string readFileName = System.IO.Path.GetFileName(file); // Имя файла

                ArrayList lineList = new ArrayList();
                WriteTextInArrayList(lineList, readPath); // Читаем файлы построчно и записываем в список lineList                

                ArrayList myNewList = new ArrayList();
                DataTraversal(lineList, myNewList); // Обход списка lineList и запись результата в myNewList - надо как то переписать эту часть.

                string writePath = $"{dirTemp}\\tmp_{readFileName}"; // Директория временного файла

                WriteTextInTempFile(myNewList, writePath); // Записываем во временные файлы данные из списка myNewList

                WriteTextInResultFile(readFileName, writePath, readPath); // Соединяем временный файл с загруженным и помещаем в текущую папку

                File.Delete(writePath); // Удалить временный файл               
            }

            Directory.Delete(dirTemp); // Удалить временную директорию
        }

        private void WriteTextInArrayList(ArrayList lineList, string readPath)
        {
            using (StreamReader sr = new StreamReader(readPath, Encoding.Default))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("("))
                    {
                        int startIndex = line.IndexOf('(') + 1;
                        int endIndex = line.IndexOf(')');

                        int length = endIndex - startIndex;
                        int lineLength = 55 - line.Length;

                        string probell = "";

                        for (int l = 0; l < lineLength; l++)
                        {
                            probell += ".";
                        }
                        lineList.Add(line.Substring(startIndex, length) + probell);
                    }
                    else if (line.StartsWith("T") || line.StartsWith("NAT"))
                    {
                        lineList.Add(line);
                    };
                }
            }
        }

        private void DataTraversal(ArrayList lineList, ArrayList myNewList)
        {
            ArrayList firstNumber = new ArrayList();
            ArrayList secondNumber = new ArrayList();

            for (int j = 0; j < lineList.Count; j++) // Заполняем списки firstNumber и secondNumber индексом начала и конца описания обработки
                                                     // например: Т9 название_инструмента NAT 1 2 3
            {
                string str = Convert.ToString(lineList[j]);
                if (str.StartsWith("NAT"))
                {
                    firstNumber.Add(j);
                    if (j != 0)
                    {
                        secondNumber.Add(j - 1);
                    }
                }
            }

            // Добавляем последнии строчек, т.к. предыдущий цикл их не записывает
            firstNumber.Add(lineList.Count - Convert.ToInt32(secondNumber[secondNumber.Count - 1]) + 1);
            secondNumber.Add(lineList.Count - 1);

            string exampleString = " "; // перем для разделения строк с NAT из lineList
            string exampleString2 = " "; // перем для разделения строк с NAT из lineList
            string exampleString3 = " "; // перем для разделения строк с NAT из lineList

            int firstRange;
            int secondRange;

            ArrayList myList = new ArrayList();

            for (int k = 0; k < firstNumber.Count - 1; k++)
            {
                firstRange = Convert.ToInt32(firstNumber[k]);
                secondRange = Convert.ToInt32(secondNumber[k]);

                exampleString2 = Convert.ToString(lineList[firstRange + 2]);

                if (exampleString2 != exampleString3)
                {
                    exampleString = "";

                    for (int n = secondRange; n >= firstRange; n--)
                    {
                        if (n == firstRange + 1 || n > firstRange + 3)
                        {
                            continue;
                        }
                        if (n == firstRange + 2)
                        {
                            exampleString3 = Convert.ToString(lineList[n]);
                        }
                        string loopString = Convert.ToString(lineList[n]);
                        if (loopString.Contains("M06"))
                        {
                            loopString = loopString.Substring(0, 3);
                        }
                        exampleString = exampleString + loopString + " ";
                    }
                    myList.Add(exampleString);
                }
                else
                {
                    myList.Add($"{lineList[firstRange]} ");
                }
            }

            string bString = " ";

            foreach (string aString in myList)
            {

                if (aString.StartsWith("NAT"))
                {
                    bString = bString + "_" + aString.Substring(3, 2);
                }
                else
                {
                    myNewList.Add(bString);
                    bString = " ";
                    bString = aString;
                }
            }
        }

        private void WriteTextInTempFile(ArrayList myNewList, string writePath)
        {
            using (StreamWriter sw = new StreamWriter(writePath, true, Encoding.Default))
            {
                foreach (string tmp in myNewList)
                {
                    if (tmp != " ")
                    {
                        sw.WriteLine($"({tmp})");
                    }
                }
            }
        }

        private void WriteTextInResultFile(string readFileName, string writePath, string readPath)
        {
            string currentPath = System.IO.Directory.GetCurrentDirectory();
            string newPath = $"{currentPath}\\{readFileName}";

            string writeVariable = File.ReadAllText(writePath, Encoding.Default);
            string probell02 = "\n";
            string readVariable = File.ReadAllText(readPath, Encoding.Default);

            File.WriteAllText(newPath, String.Concat(writeVariable, probell02, readVariable).Replace(",", "."), Encoding.Default);
        }
    }
}