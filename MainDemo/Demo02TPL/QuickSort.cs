using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo02TPL
{
    internal class QuickSort
    {
        // Sequential quicksort
        public static void QuicksortSequential<T>(T[] arr) where T : IComparable<T>
        {
            QuicksortSequential(arr, 0, arr.Length - 1);
        }

        // Parallel quicksort
        public static void QuicksortParallel<T>(T[] arr) where T : IComparable<T>
        {
            QuicksortParallel(arr, 0, arr.Length - 1);
        }

        private static void QuicksortSequential<T>(T[] arr, int left, int right) where T : IComparable<T>
        {
            if (right > left)
            {
                int pivot = Partition(arr, left, right);
                QuicksortSequential(arr, left, pivot - 1);
                QuicksortSequential(arr, pivot + 1, right);
            }
        }

        private static void QuicksortParallel<T>(T[] arr, int left, int right) where T : IComparable<T>
        {
            const int SEQUENTIAL_THRESHOLD = 2048;
            if (right > left)
            {
                if (right - left < SEQUENTIAL_THRESHOLD)
                {
                    // There is overhead to using Parallel Extensions.  At some point in the sort,
                    // the overhead becomes more expensive than just sorting sequentially.  As such,
                    // we use a threshold to determine when in the sort we should switch
                    // from parallel to sequential.  This is just like how many quicksort implementations
                    // will switch over to an insertion sort when the quicksort overhead is more expensive
                    // than doing a naive sort.
                    QuicksortSequential(arr, left, right);
                    // 2024 - In reality, in small data sets (like 1000s elements), the overhead of QuickSort
                    // is greater than for example - simple insertion sort
                    // And by the way - we have Radix Sort as well
                }
                else
                {
                    int pivot = Partition(arr, left, right);
                    Parallel.Invoke(
                      () => QuicksortParallel(arr, left, pivot - 1),
                      () => QuicksortParallel(arr, pivot + 1, right));
                }
            }
        }

        private static void Swap<T>(T[] arr, int i, int j)
        {
            T tmp = arr[i];
            arr[i] = arr[j];
            arr[j] = tmp;
        }

        private static int Partition<T>(T[] arr, int low, int high) where T : IComparable<T>
        {
            // Simply partitioning implementation

            int pivotPos = (high + low) / 2;
            T pivot = arr[pivotPos];
            Swap(arr, low, pivotPos);

            int left = low;
            for (int i = low + 1; i <= high; i++)
            {
                if (arr[i].CompareTo(pivot) < 0)
                {
                    left++;
                    Swap(arr, i, left);
                }
            }

            Swap(arr, low, left);
            return left;
        }
    }
}
