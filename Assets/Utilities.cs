using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public static class Utilities {
	private static System.Random rand = new System.Random();

	public static int GetRandomNumber(int max) {		
		return rand.Next (max);		
	}

	public static float GetRandomNumber(float max) {
		return (float)rand.NextDouble() * max;
	}
	public static int GetRandomNumber(int min, int max) {
		return rand.Next(min, max);
	}
	public static float GetRandomNumber(float min, float max) {
		return min + (float)rand.NextDouble() * (max - min);
	}

	public static Vector3 GetRandomVector(float min, float max){
		return new Vector3 (GetRandomNumber (min, max), GetRandomNumber (min, max), GetRandomNumber (min, max));
	}




    //    list: List<T> to resize
    //    size: desired new size
    // element: default value to insert
    public static void Resize<T>(this List<T> list, int size, T element) {
        int count = list.Count;

        if (size < count) {
            list.RemoveRange(size, count - size);
        }
        else if (size > count) {
            if (size > list.Capacity)   // Optimization
                list.Capacity = size;

            list.AddRange(Enumerable.Repeat(element, size - count));
        }
    }

    public static void Resize<T>(this List<T> list, int size) where T : new() {
        Resize(list, size, new T());
    }


    //    list: List<T> to assign
    //    size: desired new size
    // element: default value to replace all elements
    public static void Assign<T>(this List<T> list, int size, T element = default(T)) {
        list.Clear();
        list.AddRange(Enumerable.Repeat(element, size));
    }

}

