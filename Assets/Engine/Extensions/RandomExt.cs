using System;
using UnityEngine;

public static class RandomExt {
	private static System.Random s_rand = new System.Random();
	private static byte[] s_buf = new byte[8];

	public static long Range(long _min, long _max) {
        if (_min == _max) {
            return _min;
        } else {
            s_rand.NextBytes(s_buf);
            long longRand = BitConverter.ToInt64(s_buf, 0);
            return (Math.Abs(longRand % (_max - _min)) + _min);
        }
	}
}
