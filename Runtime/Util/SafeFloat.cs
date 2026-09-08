using UnityEngine;

namespace com.swecial.unity {
	public struct SafeFloat {
		private int _offset;
		private float _value;

		public SafeFloat (float value) {
			_offset = Random.Range(-1000, +1000);
			_value = value + _offset;
		}
		public float value() {
			return _value - _offset;
		}
		public override string ToString() {
			return value().ToString();
		}
		public static SafeFloat operator +(SafeFloat l1, SafeFloat l2) {
			return new SafeFloat(l1.value() + l2.value());
		}
		public static SafeFloat operator +(SafeFloat sl, int i) {
			return new SafeFloat(sl.value() + i);
		}
		public static SafeFloat operator -(SafeFloat l1, SafeFloat l2) {
			return new SafeFloat(l1.value() - l2.value());
		}
		public static SafeFloat operator *(SafeFloat l1, SafeFloat l2) {
			return new SafeFloat(l1.value() * l2.value());
		}
		public static SafeFloat operator /(SafeFloat l1, SafeFloat l2) {
			return new SafeFloat(l1.value() / l2.value());
		}
		public static float operator /(SafeFloat sl, float f) {
			return sl.value() / f;
		}
		public static implicit operator float(SafeFloat sl) {
			return sl.value();
		}
		public static implicit operator int(SafeFloat sl) {
			return (int)sl.value();
		}
		public static implicit operator SafeFloat(float f) {
			return new SafeFloat(f);
		}
		public static implicit operator SafeFloat(long l) {
			return new SafeFloat((float)l);
		}
	}
}