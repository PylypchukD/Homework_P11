using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Task_02
{
    internal class Program
    {
        static Random random = new Random();
        static SpinLock block = new SpinLock(10); // Интервал 10 млск.

        static FileStream stream = File.Open("log.txt", FileMode.Create, FileAccess.Write, FileShare.None);
        static StreamWriter writer = new StreamWriter(stream);

        static void Procedure(object obj)
        {
            string text;
            using (StreamReader reader = new StreamReader((string)obj))
            {
                text = reader.ReadToEnd();
            }

            using (new SpinLockManager(block))
            {
                writer.WriteLine("Поток {0} запускается.", Thread.CurrentThread.GetHashCode());
                writer.WriteLine(text);
                writer.Flush();
            }

            int time = random.Next(10, 200);
            Thread.Sleep(time);

            using (new SpinLockManager(block))
            {
                writer.WriteLine("Поток [{0}] завершается.", Thread.CurrentThread.GetHashCode());
                writer.Flush();
            }
        }

        static void Main(string[] args)
        {
            /*
             * Используя конструкции блокировки, модифицируйте последний пример урока таким образом, чтобы получить возможность поочередной работы 3-х потоков.
             */

            Thread thread1 = new Thread(new ParameterizedThreadStart(Procedure));
            thread1.Start(@".\text1.txt");

            Thread thread2 = new Thread(new ParameterizedThreadStart(Procedure));
            thread2.Start(@".\text2.txt");
            
            Thread thread3 = new Thread(new ParameterizedThreadStart(Procedure));
            thread3.Start(@".\text3.txt");
        }
    }

    public class SpinLock
    {
        // Флаг [0 - блок свободен. 1 - блок занят].
        int block;

        //  Интервал через который потоки проверяют переменную block.
        int wait;

        public SpinLock(int wait)
        {
            this.wait = wait;
        }

        // Установить блокировку (аналог - Monitor.Enter).
        public void Enter()
        {
            // Метод CompareExchange() [ Алгоритм работы ]
            // 1. Сравнивает начальное значение первого аргумента с третьим аргументом.
            // 2. Если первый аргумент равен третьему аргументу, то в первый аргумент записывается значение второго аргумента.
            // 3. Иначе, если первый аргумент не равен третьему аргументу, то первый аргумент остается без изменения.
            // 4. Возвращает начальное значение первого аргумента (каждый раз).
            int result = Interlocked.CompareExchange(ref block, 1, 0);

            while (result == 1)
            {
                // Блокировка занята, ожидать.
                Thread.Sleep(wait);
                result = Interlocked.CompareExchange(ref block, 1, 0);
            }
        }

        // Сбросить блокировку (аналог - Monitor.Exit).
        public void Exit()
        {
            Interlocked.Exchange(ref block, 0);
        }
    }

    public class SpinLockManager : IDisposable
    {
        SpinLock block;

        public SpinLockManager(SpinLock block)
        {
            this.block = block;
            block.Enter();
        }

        public void Dispose()
        {
            block.Exit();
        }
    }

}
