using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo02TPL
{
    public class MyExeption : Exception
    {
        public MyExeption(string message) : base(message)
        {
        }
    }
    internal class ConcurrencyAndException
    {
        
        public static void RunMe()
        {
            try
            {
                var t1 = Task.Factory.StartNew(TException1);
                var t2 = Task.Factory.StartNew(TException2);
                var t3 = Task.Factory.StartNew(TException3);

                Task.WaitAll(t1, t2, t3);
            }
            catch (MyExeption ex)
            {
                Console.WriteLine("MyExeption!");
                Console.WriteLine(ex.ToString());
            }
            catch (AggregateException ex)
            {
                Console.WriteLine("AggregateExceptions!");
                foreach (var innerEx in ex.InnerExceptions)
                {
                    Console.WriteLine(innerEx.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception!");
                Console.WriteLine(ex.ToString());
            }
            //Task.WhenAll(t1, t2, t3).ContinueWith(t =>
            //{
            //    if (t.IsFaulted)
            //    {
            //        foreach (var ex in t.Exception.InnerExceptions)
            //        {
            //            Console.WriteLine(ex.Message);
            //        }
            //    }
            //});

        }
        public static async Task RunMeAsync()
        {
            try
            {
                await Task.Run(() => TException1());
                await Task.Run(() => TException2());
                await Task.Run(() => TException3());
            }
            catch (MyExeption ex)
            {
                Console.WriteLine("MyExeption!");
                Console.WriteLine(ex.ToString());
            }
            catch (AggregateException ex)
            {
                Console.WriteLine("AggregateExceptions!");
                foreach (var innerEx in ex.InnerExceptions)
                {
                    Console.WriteLine(innerEx.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception!");
                Console.WriteLine(ex.ToString());
            }
        }

        public static async Task RunMeAsync1()
        {
            try
            {
                var t1 = Task.Factory.StartNew(TException1);
                var t2 = Task.Factory.StartNew(TException2);
                var t3 = Task.Factory.StartNew(TException3);
                await Task.WhenAll(t1, t2, t3);
            }
            catch (MyExeption ex)
            {
                Console.WriteLine("MyExeption!");
                Console.WriteLine(ex.ToString());
            }
            catch (AggregateException ex)
            {
                Console.WriteLine("AggregateExceptions!");
                foreach (var innerEx in ex.InnerExceptions)
                {
                    Console.WriteLine(innerEx.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception!");
                Console.WriteLine(ex.ToString());
            }
        }

        public static void TException1()
        {
            Console.WriteLine("TException1");
            Thread.Sleep(2000);
            throw new NotSupportedException();
        }
        public static void TException2()
        {
            Console.WriteLine("TException2");
            Thread.Sleep(1000);
            throw new Exception();
        }
        public static void TException3()
        {
            Console.WriteLine("TException3");
            var t3a = Task.Factory.StartNew(TException3a);
            Thread.Sleep(500);
            var t3b = Task.Factory.StartNew(TException3b);
            t3b.Wait();
            Thread.Sleep(500);
            throw new MyExeption("MyMyException");
        }
        public static void TException3a()
        {
            Console.WriteLine("TException3a");
            Thread.Sleep(100);
            throw new MyExeption("MyMyException A");
        }
        public static void TException3b()
        {
            Console.WriteLine("TException3b");
            Thread.Sleep(100);
            throw new ArgumentNullException();
        }
    }
}
