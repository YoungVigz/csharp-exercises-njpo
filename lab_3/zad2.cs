using System;
using System.Collections.Generic;

public interface SortStrategy
{
    void Sort(List<int> list);
}

public class BubbleSort : SortStrategy
{
    public void Sort(List<int> list)
    {
        Console.WriteLine("Sortowanie z wykorzystaniem bubble sort: ");
        for (int i = 0; i < list.Count - 1; i++)
        {
            for (int j = 0; j < list.Count - i - 1; j++)
            {
                if (list[j] > list[j + 1])
                {
                    int temp = list[j];
                    list[j] = list[j + 1];
                    list[j + 1] = temp;
                }
            }
        }
    }
}

public class QuickSort : SortStrategy
{
    public void Sort(List<int> list)
    {
        Console.WriteLine("Sortowanie z wykorzystaniem quick sort: ");
        QuickSortInternal(list, 0, list.Count - 1);
    }

    private void QuickSortInternal(List<int> list, int left, int right)
    {
        if (left >= right) return;

        int pivot = Partition(list, left, right);
        QuickSortInternal(list, left, pivot - 1);
        QuickSortInternal(list, pivot + 1, right);
    }

    private int Partition(List<int> list, int left, int right)
    {
        int pivot = list[right];
        int i = left - 1;

        for (int j = left; j < right; j++)
        {
            if (list[j] <= pivot)
            {
                i++;
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        (list[i + 1], list[right]) = (list[right], list[i + 1]);
        return i + 1;
    }
}


public class SortedList
{
    private SortStrategy _sortStrategy;

    public SortedList(SortStrategy sortStrategy)
    {
        _sortStrategy = sortStrategy;
    }

    public void SetSortStrategy(SortStrategy sortStrategy)
    {
        _sortStrategy = sortStrategy;
    }

    public void Sort(List<int> list)
    {
        _sortStrategy.Sort(list);
    }
}

public class Program
{
    public static void Main()
    {
        var numbers = new List<int> { 5, 2, 9, 1, 5, 6, 10, 11, -5 };

        var sortedList = new SortedList(new BubbleSort());
        sortedList.Sort(numbers);
        Console.WriteLine(string.Join(", ", numbers));


        sortedList.SetSortStrategy(new QuickSort());
        sortedList.Sort(numbers);
        Console.WriteLine(string.Join(", ", numbers));

    }
}
