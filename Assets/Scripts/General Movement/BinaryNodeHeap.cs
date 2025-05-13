
using System;
using System.Collections.Generic;
using UnityEngine;

public class BinaryNodeHeap
{
    private List<Node> heap = new List<Node>();
    public int Count => heap.Count;

    public void Add(Node node)
    {
        heap.Add(node);
        
        int bubbleIndex = heap.Count -1;

        while (bubbleIndex > 0)
        {
            int parentIndex = (bubbleIndex -1) / 2;

            if (heap[bubbleIndex]._fCost <= heap[parentIndex]._fCost)
            {
                (heap[bubbleIndex], heap[parentIndex]) = (heap[parentIndex], heap[bubbleIndex]);
                bubbleIndex = parentIndex;
            }
            else
            {
                break;
            }
        }
    }

    public Node Remove()
    {
        if (heap.Count == 0)
        {
            return null;
        }
    
        Node removedNode = heap[0];
        heap[0] = heap[heap.Count -1];
        heap.RemoveAt(heap.Count -1);

        SinkDown(0);
        return removedNode;
    }

    public void SinkDown(int index)
    {
        while (true)
        {
            int leftChild = 2* index +1;
            int rightChild = 2* index +2;
            int smallest = index;

            if (leftChild < heap.Count && heap[leftChild]._fCost < heap[smallest]._fCost)
            {
                smallest = leftChild;
            }

            if (rightChild < heap.Count && heap[rightChild]._fCost < heap[smallest]._fCost)
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

    public bool Contains(Node node)
    {
        return heap.Contains(node);
    }

    public void PrintHeap()
    {
        string s = "[Heap Atual]: ";
        for (int i = 0; i < heap.Count; i++)
        {
            s += heap[i]._fCost + " ";
        }
    }
}
