using System;

using RT;

using Xunit;
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

namespace test {
	public class UnitTest1 {

		private static XarkId EmptyXarkId() => new(Guid.Empty);

		[Fact]
		public void TestEmptyXarkId() {
			var result = EmptyXarkId();
			Assert.Equal("00000000-0000-4000-8000-000000000000", result.ToGuid().ToString("d"));
		}

		[Fact]
		public void TestEmptyXarkIdBase64() {
			var enc = EmptyXarkId().ToString();
			Assert.Equal("AAAAAAAAAAAAAAAAAAAA", enc);
		}

		[Fact]
		public void TestBase64ToEmptyComb() {
			var g1 = new XarkId("AAAAAAAAAAAAAAAAAAAA");
			var g2 = EmptyXarkId();
			Assert.Equal(g2, g1);
		}

		[Fact]
		public void RoundTrip() {
			for(var x = 1; x < 100; x++) {
				var g1 = new XarkId();
				var gs = g1.ToString();
				var g2 = new XarkId(gs);
				Assert.Equal(g1, g2);
			}
		}

		[Fact]
		public void StartsWithLetter() {
			for(var x = 1; x < 100; x++) {
				var g1 = new XarkId();
				var gs = g1.ToString();
				Assert.Matches("^[A-Za-z]", gs);
			}
		}

		[Fact]
		public void TestTimestamp() {
			var now = DateTime.UtcNow;
			var xarkId = new XarkId();
			xarkId.SetTimestamp(now);
			var retrievedTimestamp = xarkId.GetTimestamp();
			// Check that the difference between the timestamps is less than or equal to 1 millisecond
			Assert.Equal(0, (retrievedTimestamp - now).Duration().Milliseconds);
		}

		[Fact]
		public void TestBinaryConversion() {
			var xarkId = new XarkId();
			var binary = xarkId.ToBinary();
			Assert.Equal(15, binary.Length);
		}

		[Fact]
		public void TestEquality() {
			var xarkId1 = new XarkId();
			var xarkId2 = new XarkId(xarkId1.ToString());
			Assert.Equal(xarkId1, xarkId2);
		}

		[Fact]
		public void TestComparison() {
			var xarkId1 = new XarkId();
			var xarkId2 = new XarkId();
			Assert.NotEqual(0, xarkId1.CompareTo(xarkId2));
		}

		[Fact]
		public void TestInvalidInput() {
			Assert.Throws<ArgumentException>(() => new XarkId("InvalidString"));
			Assert.Throws<ArgumentNullException>(() => new XarkId((byte[]) null));
		}

	}
}
