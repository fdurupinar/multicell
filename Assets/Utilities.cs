using System;
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

}

