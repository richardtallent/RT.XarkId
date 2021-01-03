using System;
using RT;
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

/*
******************************************************************************
This is an example console application, showing a minimal-code use of the 
XarkId API. Run it with no arguments and it will generate a XarkId and show it
in compact, GUID, and timestamp-only form (UTC). Pass it a compact-form Xark
ID and it will decode the ID and do the same.
******************************************************************************
 */

enum ExitCode : int {
	Success = 0,
	Error = 1
}

class GetXarkId {

	public static int Main(string[] args) {

		XarkId x;

		if (args.Length == 0) {
			Console.WriteLine("Creating...");
			x = new XarkId();
		} else {
			Console.WriteLine("Parsing...");
			try {
				x = new XarkId(args[0]);
			} catch {
				Console.WriteLine("Not a valid XARK ID.");
				return (int)ExitCode.Error;
			}
		}

		Console.WriteLine(x.ToString());
		Console.WriteLine(x.ToString("b"));
		Console.WriteLine(x.ToString("g"));
		Console.WriteLine(x.GetTimestamp());
		return (int)ExitCode.Success;

	}

}