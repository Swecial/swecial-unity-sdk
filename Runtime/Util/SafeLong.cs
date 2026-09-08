using UnityEngine;

namespace com.swecial.unity {
	public struct SafeLong {
		private long _offset;
		private long _value;

		public SafeLong (long value) {
			_offset = Random.Range(-1000, +1000);
			_value = value + _offset;
		}
		public long value() {
			return _value - _offset;
		}
		public override string ToString() {
			return value().ToString();
		}
		public static SafeLong operator +(SafeLong l1, SafeLong l2) {
			return new SafeLong(l1.value() + l2.value());
		}
		public static SafeLong operator +(SafeLong sl, int i) {
			return new SafeLong(sl.value() + i);
		}
		public static SafeLong operator -(SafeLong l1, SafeLong l2) {
			return new SafeLong(l1.value() - l2.value());
		}
		public static SafeLong operator *(SafeLong l1, SafeLong l2) {
			return new SafeLong(l1.value() * l2.value());
		}
		public static SafeLong operator /(SafeLong l1, SafeLong l2) {
			return new SafeLong(l1.value() / l2.value());
		}
		public static float operator /(SafeLong sl, float f) {
			return sl.value() / f;
		}
		public static implicit operator long(SafeLong sl) {
			return sl.value();
		}
		public static implicit operator int(SafeLong sl) {
			return (int)sl.value();
		}
		public static implicit operator SafeLong(float f) {
			return new SafeLong((long)f);
		}
		public static implicit operator SafeLong(long l) {
			return new SafeLong(l);
		}
	}
}