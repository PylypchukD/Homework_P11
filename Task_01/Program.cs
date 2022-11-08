using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace task_01
{
    internal class Program
    {
        static FileStream stream = File.Open("log.txt", FileMode.Create, FileAccess.Write, FileShare.None);
        static StreamWriter writer = new StreamWriter(stream);

        static void Main(string[] args)
        {
            /*
             * Создайте консольное приложение, которое в различных потоках сможет получить доступ к 2-м файлам. 
             * Считайте из этих файлов содержимое и попытайтесь записать полученную информацию в третий файл. 
             * Чтение/запись должны осуществляться одновременно в каждом из дочерних потоков. 
             * Используйте блокировку потоков для того, чтобы добиться корректной записи в конечный файл. 
             */

            Thread thread1 = new Thread(new ParameterizedThreadStart(Procedure));
            thread1.Start(@".\text1.txt");

            Thread thread2 = new Thread(new ParameterizedThreadStart(Procedure));
            thread2.Start(@".\text2.txt");

            //Console.ReadKey();
        }
        public static void Procedure(object obj)
        {
            string text;
            using (StreamReader reader = new StreamReader((string)obj))
            {
                text = reader.ReadToEnd();
            }
            Thread.Sleep(1000);
            lock (stream)
            {
                writer.WriteLine(text);
                writer.Flush();
            }
        }

    }


}
