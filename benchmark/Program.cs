using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace RT.XarkIdBenchmark;

[MemoryDiagnoser]
public class XarkIdBenchmarks {
	private readonly XarkId _xarkId;

	public XarkIdBenchmarks() {
		_xarkId = new XarkId();
	}

	[Benchmark]
	public string GenerateToString() {
		return _xarkId.ToString();
	}

	[Benchmark]
	public Guid GenerateToGuid() {
		return _xarkId.ToGuid();
	}
}

class Program {
	static void Main(string[] args) {
		BenchmarkRunner.Run<XarkIdBenchmarks>();
	}
}
