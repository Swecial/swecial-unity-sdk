using UnityEngine;
using System.Collections;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace com.swecial.unity {
	public class SwecialUser : SwecialObject {
		private byte[] _bytes;
		public SwecialUser() : base() {
			className = "_user";
		}
    }
}