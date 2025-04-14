using System;
using System.Linq;
/*
	Copyright 2017-2025 Richard S. Tallent, II

	Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files
	(the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge,
	publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to
	do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
	MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
	LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
	CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

namespace RT {

	[Serializable]
	[System.Runtime.InteropServices.ComVisible(true)]
	public class XarkId : IComparable, IComparable<XarkId>, IEquatable<XarkId> {

		private readonly byte[] bytes = new byte[15];
		private static readonly Random Rnd = new();

		// Cached hash code to avoid recomputation
		private int? cachedHashCode;

		// Cached timestamp to avoid redundant calculations
		private long? cachedUnixTimestamp;

		public XarkId() {
			SetTimestamp(DateTime.UtcNow);
			Rnd.NextBytes(bytes.AsSpan(6, 9));
		}

		public XarkId(byte[] value) {
			if(value == null) throw new ArgumentNullException(nameof(value));
			if(value.Length != 15)
				throw new ArgumentException($"Cannot convert byte[{value.Length}] to byte[15]");
			value.CopyTo(bytes, 0);
		}

		public XarkId(string s) {
			if(s == null || s.Length != 20) {
				throw new ArgumentException("Invalid XarkId string.");
			}
			Span<byte> decodedBytes = stackalloc byte[15];
			var base64 = s.Replace('-', '+').Replace('_', '/');
			Convert.TryFromBase64String(base64, decodedBytes, out _);
			decodedBytes.CopyTo(bytes);
		}

		public XarkId(Guid g) {
			var gbytes = g.ToByteArray();
			if(BitConverter.IsLittleEndian) {
				// Swap some bytes if System.Guid is running on little-Endian systems.
				Array.Reverse(gbytes, 0, 4);
				Array.Reverse(gbytes, 4, 2);
				Array.Reverse(gbytes, 6, 2);
			}
			// TODO: Swap some bytes to ensure network order?
			// Copy bytes 0-5 (timestamp) to bytes 0-5
			Array.Copy(gbytes, bytes, 6);
			// Copy bytes 9-15 (random) to bytes 8-14
			Array.Copy(gbytes, 9, bytes, 8, 7);
			// Set byte 6 from least-significant nybble of byte 6 and most-significant nybble of byte 7.
			bytes[6] = (byte) ((gbytes[6] << 4) | (gbytes[7] >> 4));
			// Set byte 7 from least-significant nybble of byte 7 and least-significant nybble of byte 8.
			bytes[7] = (byte) ((gbytes[7] << 4) | (gbytes[8] & 15));
		}

		public void SetTimestamp(DateTime timestamp) {
			cachedUnixTimestamp = new DateTimeOffset(timestamp).ToUnixTimeMilliseconds();
			long unix = cachedUnixTimestamp.Value;
			bytes[5] = (byte) (unix & 255);
			bytes[4] = (byte) (unix >> 8 & 255);
			bytes[3] = (byte) (unix >> 16 & 255);
			bytes[2] = (byte) (unix >> 24 & 255);
			bytes[1] = (byte) (unix >> 32 & 255);
			bytes[0] = (byte) (unix >> 40 & 255);
		}

		public DateTime GetTimestamp() {
			if(!cachedUnixTimestamp.HasValue) {
				cachedUnixTimestamp =
					((long) bytes[0] << 40)
					+ ((long) bytes[1] << 32)
					+ ((long) bytes[2] << 24)
					+ ((long) bytes[3] << 16)
					+ ((long) bytes[4] << 8)
					+ bytes[5];
			}
			return DateTimeOffset.FromUnixTimeMilliseconds(cachedUnixTimestamp.Value).UtcDateTime;
		}

		public byte[] ToBytes() => bytes;

		public byte[] ToBinary() {
			var result = new byte[15];
			bytes.CopyTo(result, 0);
			return result;
		}

		public Guid ToGuid() {
			Span<byte> gbytes = stackalloc byte[16];
			bytes.AsSpan(0, 6).CopyTo(gbytes);
			bytes.AsSpan(8, 7).CopyTo(gbytes[9..]);
			gbytes[6] = (byte) (bytes[6] >> 4 | 0b0100_0000);
			gbytes[7] = (byte) (bytes[6] << 4 | bytes[7] >> 4);
			gbytes[8] = (byte) (bytes[7] & 15 | 0b1000_0000);
			if(BitConverter.IsLittleEndian) {
				gbytes[..4].Reverse();
				gbytes.Slice(4, 2).Reverse();
				gbytes.Slice(6, 2).Reverse();
			}
			return new Guid(gbytes);
		}

		public override string ToString() => ToString(null);

		public string ToString(string format = null) {
			if(format == "b") return string.Join(' ', bytes.Select(b => b.ToString("x2")));
			if(format == "g") return ToGuid().ToString("d");
			Span<char> base64Chars = stackalloc char[20];
			Convert.TryToBase64Chars(bytes, base64Chars, out _);
			for(var i = 0; i < base64Chars.Length; i++) {
				if(base64Chars[i] == '+') base64Chars[i] = '-';
				else if(base64Chars[i] == '/') base64Chars[i] = '_';
			}
			return new string(base64Chars);
		}

		public int CompareTo(XarkId o) {
			if(o == null) return 1;
			for(var i = 0; i <= 14; i++) {
				var result = bytes[i].CompareTo(o.bytes[i]);
				if(result != 0) return result;
			}
			return 0;
		}

		public bool Equals(XarkId o) => CompareTo(o) == 0;
		public override bool Equals(object o) => CompareTo(o as XarkId) == 0;

		public int CompareTo(object o) {
			if(o is XarkId other) return CompareTo(other);
			throw new ArgumentException("Object is not a XarkId");
		}

		public override int GetHashCode() {
			if(cachedHashCode.HasValue) return cachedHashCode.Value;
			int hash = 17;
			foreach(var b in bytes) {
				hash = hash * 31 + b;
			}
			cachedHashCode = hash;
			return hash;
		}

	}

}