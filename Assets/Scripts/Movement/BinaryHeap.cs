
using System;
using UnityEngine;

public class BinaryHeap
{
    private int[] heap;
    private int numberOfItens;

    public BinaryHeap(int numberOfElements)
    {
        heap = new int[numberOfElements];
        numberOfItens = 0;
    }

    public void Add(int fCost)
    {
        heap[numberOfItens] = fCost;
        
        int bubbleIndex = numberOfItens;

        while (bubbleIndex > 0)
        {
            int parentIndex = (bubbleIndex -1) / 2;

            if (heap[bubbleIndex] <= heap[parentIndex])
            {
                (heap[bubbleIndex], heap[parentIndex]) = (heap[parentIndex], heap[bubbleIndex]);
                bubbleIndex = parentIndex;
            }
            else
            {
                break;
            }
        }

        numberOfItens++;
    }

    public int Remove()
    {
        if (numberOfItens == 0)
        {
            return -1;
        }

        numberOfItens--;
        int removedValue = heap[0];

        heap[0] = heap[numberOfItens];

        SinkDown(0);

        return removedValue;
    }

    public void SinkDown(int index)
    {
        while (true)
        {
            int leftChild = 2* index +1;
            int rightChild = 2* index +2;
            int smallest = index;

            if (leftChild < numberOfItens && heap[leftChild] < heap[smallest])
            {
                smallest = leftChild;
            }

            if (rightChild < numberOfItens && heap[rightChild] < heap[smallest])
            {
                smallest = rightChild;
            }

            if (smallest != index)
            {
                (heap[index], heap[smallest]) = (heap[smallest], heap[index]);
                index = smallest;
            }
            else
            {
                break;
            }
        }
    }

    public void PrintHeap()
    {
        string s = "[Heap Atual]: ";
        for (int i = 0; i < numberOfItens; i++)
        {
            s += heap[i] + " ";
        }
    }
}
