using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    public static T[] ShuffleArray<T>(T[] arr, int seed) {
        System.Random rng = new System.Random(seed);
        for (int i = 0; i < arr.Length - 1; i++) {
            int randIdx = rng.Next(i, arr.Length);
            T temp = arr[randIdx];
            arr[randIdx] = arr[i];
            arr[i] = temp;
        }
        return arr;
    }

}
