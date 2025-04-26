using UnityEngine;

public class BinaryHeapTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BinaryHeap heap = new BinaryHeap(10);

        heap.Add(15);
        heap.PrintHeap();
        heap.Add(10);
        heap.PrintHeap();
        heap.Add(30);
        heap.PrintHeap();
        heap.Add(5);
        heap.PrintHeap();
        heap.Add(20);
        heap.PrintHeap();

        heap.Remove();
        heap.PrintHeap();
        heap.Remove();
        heap.PrintHeap();
    }
}
