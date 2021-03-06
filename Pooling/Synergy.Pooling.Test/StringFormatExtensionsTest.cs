﻿// ------------------------------------------------------------------------
//
// WARN: This file is reused between Synergy projects - change it carefully
// 
// ------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;

// ReSharper disable once CheckNamespace

namespace Synergy.Extensions.Test
{
    [TestFixture]
    public class StringFormatExtensionsTest
    {
        [Test]
        public void FormatWith1Argument()
        {
            //ACT
            // ReSharper disable once HeapView.BoxingAllocation
            string formatted = "{0}ce".Formatted(1);

            //ASSERT
            Assert.That(formatted, Is.EqualTo("1ce"));
        }

        [Test]
        public void FormatWith1ArgumentPassingString()
        {
            //ACT


            // ReSharper disable once HeapView.BoxingAllocation
            string formatted = "{0}ce".Formatted("1");

            //ASSERT
            Assert.That(formatted, Is.EqualTo("1ce"));
        }

        [Test]
        public void FormatWith2Argument()
        {
            //ACT
            // ReSharper disable once HeapView.BoxingAllocation
            string formatted = "{0}ce {1}".Formatted(1, "upon");

            //ASSERT
            Assert.That(formatted, Is.EqualTo("1ce upon"));
        }

        [Test]
        public void FormatWith3Argument()
        {
            //ACT
            // ReSharper disable once HeapView.BoxingAllocation
            string formatted = "{0}ce {1} a {2}".Formatted(1, "upon", "time");

            //ASSERT
            Assert.That(formatted, Is.EqualTo("1ce upon a time"));
        }

        [Test]
        public void FormatWith4Argument()
        {
            //ACT
            // ReSharper disable once HeapView.BoxingAllocation
            string formatted = "{0}ce {1} a {2} {3}".Formatted(1, "upon", "time", "there was");

            //ASSERT
            Assert.That(formatted, Is.EqualTo("1ce upon a time there was"));
        }

        [Test]
        public void FormatWith5Argument()
        {
            //ACT
            // ReSharper disable once HeapView.BoxingAllocation
            string formatted = "{0}ce {1} a {2} {3} {4}".Formatted(1, "upon", "time", "there was", "a nasty robot");

            //ASSERT
            Assert.That(formatted, Is.EqualTo("1ce upon a time there was a nasty robot"));
        }

        [Test]
        public void FormatWith6Argument()
        {
            //ACT
            // ReSharper disable once HeapView.BoxingAllocation
            string formatted = "{0}ce {1} a {2} {3} {4} {5}".Formatted(1, "upon", "time", "there was", "a nasty robot", "living");

            //ASSERT
            Assert.That(formatted, Is.EqualTo("1ce upon a time there was a nasty robot living"));
        }

        [Test]
        public void FormatWith7Argument()
        {
            //ACT
            // ReSharper disable once HeapView.BoxingAllocation
            string formatted = "{0}ce {1} a {2} {3} {4} {5} {6}".Formatted(1, "upon", "time", "there was", "a nasty robot", "living", "in the neighbourhood");

            //ASSERT
            Assert.That(formatted, Is.EqualTo("1ce upon a time there was a nasty robot living in the neighbourhood"));
        }

        [Test]
        //[Explicit]
        public void performance_comparison()
        {
            var samples = 1234567;

            PerformanceComparison(false, samples);
            Console.WriteLine();
            PerformanceComparison(true, samples);
        }

        private  void PerformanceComparison(bool parallel, int samplesNo)
        {
            GC.Collect();
            //var m1 = dotMemory.Check();

            long memory0 = GC.GetTotalMemory(true);
            var s = "{0}{1}{2}{3}";

            Stopwatch pooled = Stopwatch.StartNew();
            if (parallel)
            {
                Parallel.For(0,
                    samplesNo,
                    i =>
                    {
                        // ReSharper disable once UnusedVariable
                        string formatted = "{0}{1}{2}{3}".Formatted(i + 1, i + 2, i + 3, i + 4);
                    });
            }
            else
            {
                for (var i = 0; i < samplesNo; i++)
                {
                    // ReSharper disable once UnusedVariable
                    string formatted = StringFormatExtensions.Formatted(s, "i + 1", "i + 2", "i + 3", "i + 4");

                    //if (i % 10000 == 0)
                    //    Console.WriteLine("      -- MEMORY DIFF: {0}", GC.GetTotalMemory(false) - memory0);
                }
            }

            pooled.Stop();
            GC.Collect();

            long memory1 = GC.GetTotalMemory(true);


            //long dead1 = 0;
            // dotMemory.Check(m=>
            // {
            //     dead1 = m.GetDifference(m1)
            //             .GetDeadObjects()
            //             .SizeInBytes;
            // });

            // ReSharper disable once HeapView.BoxingAllocation
            Console.WriteLine("{0}variable.Formatted()      : {1}ms    MEMORY-ALLOCATION: {2}  DEAD: {3}b",
                parallel ? "PARALLEL " : "",
                pooled.ElapsedMilliseconds.ToString(),
                (memory1 - memory0).ToString(),
                "?"//dead1.ToString()
                );

            //var m2 = dotMemory.Check();

            memory1 = GC.GetTotalMemory(true);

            Stopwatch ordinary = Stopwatch.StartNew();
            if (parallel)
            {
                Parallel.For(0,
                    samplesNo,
                    i =>
                    {
                        // ReSharper disable once UnusedVariable
                        // ReSharper disable once UseStringInterpolation
                        string formatted = string.Format("{0}{1}{2}{3}", i + 1, i + 2, i + 3, i + 4);

                        //if (i % 10000 == 0)
                        //    Console.WriteLine("      -- MEMORY DIFF: {0}", GC.GetTotalMemory(false) - memory1);
                    });
            }
            else
            {
                for (var i = 0; i < samplesNo; i++)
                {
                    // ReSharper disable once UnusedVariable
                    // ReSharper disable once UseStringInterpolation
                    string formatted = string.Format("{0}{1}{2}{3}", i + 1, i + 2, i + 3, i + 4);

                    //if (i % 10000 == 0)
                    //    Console.WriteLine("      -- MEMORY DIFF: {0}", GC.GetTotalMemory(false) - memory1);
                }
            }
            ordinary.Stop();

            long memory2 = GC.GetTotalMemory(false);

            //long dead2 = 0;
            //dotMemory.Check(m =>
            //{
            //    dead2 = m.GetDifference(m2)
            //            .GetDeadObjects()
            //            .SizeInBytes;
            //});

            Console.WriteLine("{0}String.Format(variable): {1}ms    MEMORY-ALLOCATION: {2} DEAD: {3}b",
                parallel ? "PARALLEL " : "",
                ordinary.ElapsedMilliseconds,
                memory2 - memory1,
                "?"//dead2
                );
        }

        [Test]
        public void memory_usage()
        {
            // Measure starting point memory use
            GC.Collect();
            var start = GC.GetTotalMemory(true);

            // Allocate a new byte array of 20000 elements (about 20000 bytes)
#pragma warning disable 219
            int no = 123;
#pragma warning restore 219
            object array = new object[] {123};

            // Obtain measurements after creating the new byte[]
            GC.Collect();
            var stop = GC.GetTotalMemory(true);

            // Ensure that the Array stays in memory and doesn't get optimized away
            GC.KeepAlive(array);

            Console.WriteLine("int no = 123;");
            Console.WriteLine("object array = new object[] { 123 };");
            Console.WriteLine();
            Console.WriteLine("usage: {0} bytes", stop-start);
        }
    }
}