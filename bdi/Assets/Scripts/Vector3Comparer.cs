using System;
using UnityEngine;
using System.Collections.Generic;

public class Vector3Comparer : IEqualityComparer<Vector3> {
	public bool Equals (Vector3 v1, Vector3 v2)
	{
		return v1.x == v2.x && v1.z == v2.z;
	}

	public int GetHashCode (Vector3 obj)
	{
		throw new NotImplementedException ();
	}
}

