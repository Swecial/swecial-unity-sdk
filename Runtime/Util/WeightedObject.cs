using UnityEngine;
using System.Collections;

namespace com.swecial.unity {
	public class WeightedObject<T> {
		public T obj;
		public float weight;
		public WeightedObject(T obj, float weight) {
			this.obj = obj;
			this.weight = weight;
		}
		public static WeightedObject<int> create(int obj, float weight) {
			return new WeightedObject<int>(obj, weight);
		}
        public static WeightedObject<C> create<C>(C obj, float weight) {
            return new WeightedObject<C>(obj, weight);
        }
	}
}