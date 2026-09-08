using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace com.swecial.unity {
	public class SwecialResponse<T> : SwecialObject {
		
		public string status {
			get {
				return get<string>("status");
			}
			set {
				set("status", value);
			}
		}
		public bool confirm {
			get {
				return get<bool>("confirm");
			}
		}
		public string error {
			get {
				return get<string>("error");
			}
			set {
				set("error", value);
			}
		}
		public string message {
			get {
				return get<string>("message");
			}
		}
		public string token {
			get {
				return get<string>("token");
			}
		}
		public int time {
			get {
				return get<int>("time");
			}
		}
		public int count {
			get {
				return get<int>("count");
			}
		}
		public long cpuTime {
			get {
				return get<long>("cpuTime");
			}
		}
		public T result {
			get {
				return get<T>("result");
			}
		}
        public List<int> fileLengths {
            get {
                return get<List<int>>("fileLengths");
            }
        }
		public List<byte[]> files;
	}
}