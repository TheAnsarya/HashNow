# Performance

HashNow computes **70 hash algorithms** in a single file read using parallel streaming. This page documents the architecture, benchmark results, and optimization strategies.

## Architecture

### Single-Pass Parallel Streaming

HashNow reads each file **once** and feeds all 70 algorithms concurrently:

1. **File read** — 1 MB buffer via `ArrayPool<byte>` (no heap allocation for buffers)
2. **Parallel update** — each buffer chunk is fed to all 70 streaming hash instances simultaneously
3. **Finalize** — all algorithms finalize and produce their digest
4. **JSON output** — results serialized with tab indentation

This architecture is powered by [StreamHash](https://www.nuget.org/packages/StreamHash), which provides native C# streaming implementations for all 70 algorithms with zero `unsafe` code.

### Key Design Choices

| Decision | Rationale |
|----------|-----------|
| **1 MB buffer** | Balances memory usage with I/O throughput |
| **ArrayPool** | Reuses buffers across hashing operations |
| **Parallel.ForEach** | Distributes hash computation across CPU cores |
| **Single file read** | Minimizes disk I/O — the main bottleneck |

## Benchmark Results

**Test System:** Intel Core i7-8700K 3.70GHz (6C/12T, Coffee Lake), Windows 10, .NET 10

### Throughput by File Size

| File Size | Mean Time | Throughput | Memory Allocated | Alloc Ratio |
|-----------|----------:|----------:|----------------:|:-----------:|
| 100 KB | 38 ms | 2.63 MB/s | 2.54 MB | 25.4x |
| 1 MB | 341 ms | 2.93 MB/s | 25.18 MB | 25.2x |
| 50 MB | 17.49 s | 2.86 MB/s | 1,255 MB | 25.1x |

### Finalize Overhead (Empty File)

| Metric | Value |
|--------|------:|
| Mean | 273.9 μs |
| Allocated | 62.58 KB |

The empty-file test isolates the cost of creating and finalizing all 70 hash instances with no data processing — useful for measuring framework overhead.

### v1.3.7 → v1.4.0 Improvement

The v1.4.0 release introduced the StreamHash Batch API for parallel processing:

| Metric | v1.3.7 | v1.4.0 | Change |
|--------|-------:|-------:|-------:|
| 50 MB throughput | 0.95 MB/s | 2.86 MB/s | **3.0x faster** |
| 50 MB wall time | ~52.6 s | 17.5 s | **3.0x faster** |
| Memory ratio | 24x file size | 25.1x file size | ~Same |

## Memory Profile

Memory allocations scale linearly with file size at approximately **25x** the input size. This overhead comes from 70 independent hash algorithm state buffers being maintained simultaneously.

| File Size | Gen0 | Gen1 | Gen2 |
|-----------|-----:|-----:|-----:|
| 100 KB | 357 | 0 | 0 |
| 1 MB | 4,000 | 0 | 0 |
| 50 MB | 218,000 | 19,000 | 16,000 |

## Known Bottlenecks

1. **I/O bound** — throughput plateaus at ~2.9 MB/s regardless of file size, suggesting disk read speed is the limiting factor rather than CPU computation
2. **Memory overhead** — 25x file size allocation ratio is higher than the theoretical minimum; tracked for optimization
3. **Partial parallelization** — 3x speedup on 6 cores indicates room for improvement in parallel scheduling

See [Issue #13](https://github.com/TheAnsarya/HashNow/issues/13) for the ongoing performance investigation.

## Running Benchmarks

```powershell
# Full benchmark suite
dotnet run --project benchmarks/HashNow.Benchmarks -c Release

# Quick finalize-heavy test (empty file)
dotnet run --project benchmarks/HashNow.Benchmarks -c Release -- --filter "*EmptyFile*" --job short

# Performance profiler (100KB, 1MB, 50MB)
dotnet run --project benchmarks/HashNow.Benchmarks -c Release -- --filter "*PerformanceProfiler*"
```

## StreamHash Benchmarks

For per-algorithm performance data (throughput, memory, comparison with reference libraries), see the [StreamHash Benchmark Results](https://github.com/TheAnsarya/StreamHash/blob/main/docs/benchmarks.md).
