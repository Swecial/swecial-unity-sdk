using UnityEngine;
using System.Collections;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace com.swecial.unity {
	public class SwecialFile : SwecialObject {
		private byte[] _bytes;
		public SwecialFile() : base() {
			className = "_file";
		}
		public byte[] bytes {
			set {
				_bytes = value;
			}
			get {
				return _bytes;
			}
		}
		public new SwecialRequest<SwecialFile> load() {
			var sr = new SwecialRequest<SwecialFile>();
			sr.command = "load_file";
			sr.setObject(this);
			return sr.execute();
		}
		public new SwecialRequest<SwecialFile> save() {
			var sr = new SwecialRequest<SwecialFile>();
			sr.command = "save_file";
			sr.setObject(this);
            sr.addFile(bytes);
			return sr.execute();
		}
        public override void copyFrom(object o) {
            if (o is SwecialFile) {
                var sf = (SwecialFile)o;
                bytes = sf.bytes;
            }
            base.copyFrom(o);
        }
    }
}