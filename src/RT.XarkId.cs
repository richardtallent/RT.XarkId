using System;
using System.Linq;
/*
	Copyright 2017-2021 Richard S. Tallent, II

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

		private byte[] bytes = new byte[15];
		private static DateTime MinDateTimeValue { get => new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc); }
		private static Random rnd = new Random();

		public XarkId() {
			bytes = new byte[15];
			SetTimestamp(DateTime.UtcNow);
			rnd.NextBytes(new Span<byte>(bytes, 6, 9));
		}

		private XarkId(byte[] value) {
			if (value == null) throw new ArgumentNullException(nameof(value));
			if (value.Length != 16)
				throw new ArgumentException($"Cannot convert byte[{value.Length}] to byte[16]");
			bytes = value;
		}

		public XarkId(string s) {
			if (s == null || s.Length != 20) {
				throw new ArgumentException("Invalid XarkId string.");
			}
			var base64 = s.Replace('-', '+').Replace('_', '/');
			bytes = System.Convert.FromBase64String(base64);
		}

		public XarkId(Guid g) {
			bytes = new byte[15];
			var gbytes = g.ToByteArray();
			if (BitConverter.IsLittleEndian) {
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
			bytes[6] = (byte)((gbytes[6] << 4) | (gbytes[7] >> 4));
			// Set byte 7 from least-significant nybble of byte 7 and least-significant nybble of byte 8.
			bytes[7] = (byte)((gbytes[7] << 4) | (gbytes[8] & 15));
		}

		public void SetTimestamp(DateTime timestamp) {
			var unix = new DateTimeOffset(timestamp).ToUnixTimeMilliseconds();
			Console.WriteLine(unix);
			bytes[5] = (byte)(unix & 255);
			bytes[4] = (byte)(unix >> 8 & 255);
			bytes[3] = (byte)(unix >> 16 & 255);
			bytes[2] = (byte)(unix >> 24 & 255);
			bytes[1] = (byte)(unix >> 32 & 255);
			bytes[0] = (byte)(unix >> 40 & 255);
		}

		public DateTime GetTimestamp() {
			long unix =
				((long)bytes[0] << 40)
				+ ((long)bytes[1] << 32)
				+ ((long)bytes[2] << 24)
				+ ((long)bytes[3] << 16)
				+ ((long)bytes[4] << 8)
				+ ((long)bytes[5]);
			return DateTimeOffset.FromUnixTimeMilliseconds(unix).UtcDateTime;
		}

		public byte[] ToBytes() => bytes;

		public Guid ToGuid() {
			// 00112233 4455 6677 8899 AABBCCDDEEFF GUID
			// MMMMMMMM MMMM 4xxx Vxxx xxxxxxxxxxxx
			// 00112233 4455 -667 -788 99AABBCCDDEE XarkId
			var gbytes = new byte[16];
			// Copy bytes 0-5 (timestamp) to bytes 0-5
			Array.Copy(bytes, gbytes, 6);
			// Copy bytes 8-14 (random) to bytes 9-15
			Array.Copy(bytes, 8, gbytes, 9, 7);
			// Byte 6 is split between destination bytes 6 and 7. MSB of byte 6 is the version (4, 0100b)
			// Byte 7 is split between destination bytes 7 and 8. MSB of byte 7 is the variant (8, 1000b)
			gbytes[6] = (byte)(bytes[6] >> 4 | 0b0100_0000);
			gbytes[7] = (byte)(bytes[6] << 4 | bytes[7] >> 4);
			gbytes[8] = (byte)(bytes[7] & 15 | 0b1000_0000);
			if (BitConverter.IsLittleEndian) {
				// System.Guid is expecting a byte array in the form int, short, short, byte[].
				// On little-Endian systems, Guid's conversion from byte to Guid will end up swapping
				// the byte order on the first 4 parts above. So, we swap them ahead of time so they
				// end up with the expected GUID value.
				Array.Reverse(gbytes, 0, 4);
				Array.Reverse(gbytes, 4, 2);
				Array.Reverse(gbytes, 6, 2);
			}
			return new Guid(gbytes);
		}

		public override string ToString() => ToString(null);

		public string ToString(string format = null) =>
			(format ?? string.Empty) switch
			{
				"b" => string.Join(' ', from b in bytes select b.ToString("x2")),
				"g" => ToGuid().ToString("d"),
				_ => System.Convert.ToBase64String(bytes)
					.Replace('+', '-').Replace('/', '_')    // Base64Url alternate characters
					[..20]                                  // remove any trailing `=` padding
			};

		public int CompareTo(XarkId o) {
			if (o == null) return 1;
			for (var i = 0; i <= 14; i++) {
				var result = bytes[i].CompareTo(o.bytes[i]);
				if (result != 0) return result;
			}
			return 0;
		}

		public bool Equals(XarkId o) => this.CompareTo(o) == 0;
		public override bool Equals(object o) => this.CompareTo(o) == 0;
		public int CompareTo(object o) => CompareTo(o as XarkId);
		public override int GetHashCode() => bytes.GetHashCode();

	}

}