using System;
using System.Collections.Generic;
using System.Threading;

class Program
{
    static Queue<string> kelimeKuyruk = new Queue<string>();
    static object lockObject = new object();
    static int finishedThreads = 0;
    static bool isReadingFinished = false;

    static void Main()
    {
        Console.Write("Yazan Thread sayısını girin (n): ");
        int n = int.Parse(Console.ReadLine());

        Thread okumaThread = new Thread(DosyaOkuyanThread);
        okumaThread.Start();

        Thread[] yazantThreadler = new Thread[n];
        for (int i = 0; i < n; i++)
        {
            yazantThreadler[i] = new Thread(YazanThread);
            yazantThreadler[i].Start();
        }

        okumaThread.Join();

        lock (lockObject)
        {
            isReadingFinished = true;
            Monitor.PulseAll(lockObject);
        }

        foreach (Thread workerThread in yazantThreadler)
        {
            workerThread.Join();
        }

        Console.WriteLine("Bütün thread'ler tamamlandı.");
    }

    static void DosyaOkuyanThread()
    {
        Console.WriteLine("Dosya okuma thread'i başladı.");

        string dosyaYolu = @"C:\denem\veri.txt";
        string[] kelimeler = System.IO.File.ReadAllLines(dosyaYolu);



        foreach (string kelime in kelimeler)
        {
            lock (lockObject)
            {
                kelimeKuyruk.Enqueue(kelime);
                Monitor.Pulse(lockObject);
            }
            Thread.Sleep(3000);
        }

        Console.WriteLine("Dosya okuma thread'i tamamlandı.");
    }

    static void YazanThread()
    {
        Console.WriteLine("Yazan thread başladı.");

        while (true)
        {
            string kelime = null;

            lock (lockObject)
            {

                while (kelimeKuyruk.Count == 0 && !isReadingFinished)
                {
                    Monitor.Wait(lockObject);
                }


                if (kelimeKuyruk.Count == 0 && isReadingFinished)
                {
                    break;
                }

                kelime = kelimeKuyruk.Dequeue();
            }


            Console.WriteLine($"{kelime}: {kelime.Length}");

            Thread.Sleep(3000);
        }

        lock (lockObject)
        {
            finishedThreads++;
            if (finishedThreads == kelimeKuyruk.Count)
            {
                Monitor.PulseAll(lockObject);
            }
        }

        Console.WriteLine("Yazan thread tamamlandı.");
        Console.ReadLine();
    }
}

