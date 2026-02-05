# Performance Benchmark Results - HashNow v1.4.0

**Date:** February 5, 2026  
**Version:** HashNow v1.4.0 with StreamHash v1.7.0 Batch API  
**Hardware:** Intel Core i7-8700K (6-core/12-thread, Coffee Lake, 3.70GHz)  
**OS:** Windows 10 (10.0.19045.6466/22H2/2022Update)  
**.NET:** .NET 10.0.2

## Summary

**Actual Performance: 3x speedup** (vs. expected 8-16x)

| Metric | v1.3.7 (Baseline) | v1.4.0 (Batch API) | Improvement |
|--------|-------------------|-------------------|-------------|
| 50MB Speed | 0.95 MB/s (~52.58s) | 2.86 MB/s (17.49s) | **3.0x faster** |
| Memory | 24x file size | 25.1x file size | ~Same |

**Conclusion:** Batch API provides significant improvement (3x), but falls short of expected 8-16x speedup. Created [Issue #13](https://github.com/TheAnsarya/HashNow/issues/13) to investigate.

## Detailed Benchmark Results

### BenchmarkDotNet Configuration
- **Framework:** .NET 10.0.2 (10.0.2, 10.0.225.61305)
- **Runtime:** X64 RyuJIT x86-64-v3
- **GC:** Concurrent Workstation
- **SIMD:** AVX2+BMI1+BMI2+F16C+FMA+LZCNT+MOVBE,AVX,SSE3+SSSE3+SSE4.1+SSE4.2+POPCNT,X86Base+SSE+SSE2,AES+PCLMUL VectorSize=256

### Performance Results

| Method          | Mean         | Error      | StdDev     | Throughput | Allocated  | Ratio  |
|-----------------|-------------:|-----------:|-----------:|-----------:|-----------:|-------:|
| Hash_100KB_File |     37.98 ms |   1.043 ms |   3.026 ms |   2.63 MB/s |    2.54 MB |  25.4x |
| Hash_1MB_File   |    340.90 ms |  19.137 ms |  52.708 ms |   2.93 MB/s |   25.18 MB |  25.2x |
| Hash_50MB_File  | 17,487.17 ms | 349.310 ms | 697.610 ms |   2.86 MB/s | 1255.84 MB |  25.1x |

### Memory Allocations

| File Size | Total Allocated | Allocation Ratio | Gen0 | Gen1 | Gen2 |
|-----------|----------------|------------------|------|------|------|
| 100KB | 2.54 MB | 25.4x | 357 | 0 | 0 |
| 1MB | 25.18 MB | 25.2x | 4,000 | 0 | 0 |
| 50MB | 1,255.84 MB | 25.1x | 218,000 | 19,000 | 16,000 |

### Statistical Analysis

#### 100KB File
- **Mean:** 37.98 ms
- **Median:** 37.42 ms
- **StdDev:** 3.03 ms (7.97% CV)
- **Range:** 33.23 ms - 46.19 ms
- **Throughput:** 2.63 MB/s
- **Warning:** Bimodal distribution detected (mValue = 4.14)

#### 1MB File
- **Mean:** 340.90 ms
- **Median:** 344.43 ms
- **StdDev:** 52.71 ms (15.46% CV)
- **Range:** 207.41 ms - 475.88 ms
- **Throughput:** 2.93 MB/s
- **Note:** High variance suggests external factors (background processes, I/O contention)

#### 50MB File
- **Mean:** 17.49 s
- **Median:** 17.44 s
- **StdDev:** 0.70 s (4.00% CV)
- **Range:** 15.89 s - 18.93 s
- **Throughput:** 2.86 MB/s
- **Consistency:** Lowest variance of all tests

## Comparison with Baseline

### v1.3.7 Baseline Performance
From earlier profiling (not BenchmarkDotNet):
- **50MB:** ~52.58 seconds (0.95 MB/s)
- **Memory:** 1.18GB allocations (24x file size)

### v1.4.0 Improvements
- **Speed:** 3.0x faster (52.58s → 17.49s)
- **Throughput:** 3.0x higher (0.95 MB/s → 2.86 MB/s)
- **Memory:** Slightly worse (24x → 25.1x file size)

## Performance Analysis

### ✅ Achievements
1. **Consistent 3x speedup** across all file sizes
2. **Linear scaling** - performance scales with file size
3. **Stable performance** - low variance on 50MB test
4. **All tests passing** - 108/108 unit tests, no regressions

### ❌ Concerns
1. **Below Expected Speedup** - 3x actual vs. 8-16x expected
2. **High Memory Overhead** - 25x file size (should be closer to 5-6x)
3. **No Parallel Benefit?** - Speedup doesn't match core count (6 cores)
4. **Similar Gen0 Collections** - Suggests parallel processing isn't reducing per-algorithm allocations

### 🔍 Investigation Hypothesis

The 3x speedup suggests **partial parallelization** but not full utilization:
- Expected on 6-core CPU: 5-6x speedup (accounting for overhead)
- Actual: 3x speedup
- Possible cause: I/O bottleneck (disk reads limiting CPU parallelization)

**Evidence:**
1. Throughput plateaus at ~2.9 MB/s across all file sizes
2. Memory allocations scale linearly with file size (25x ratio constant)
3. No significant difference between 100KB, 1MB, and 50MB throughput

**Next Steps:**
See [Issue #13](https://github.com/TheAnsarya/HashNow/issues/13) for investigation plan.

## Raw BenchmarkDotNet Output

### Hash_100KB_File Statistics
```
Mean = 37.984 ms, StdErr = 0.307 ms (0.81%), N = 97, StdDev = 3.026 ms
Min = 33.234 ms, Q1 = 35.581 ms, Median = 37.418 ms, Q3 = 39.709 ms, Max = 46.193 ms
IQR = 4.128 ms, LowerFence = 29.388 ms, UpperFence = 45.901 ms
ConfidenceInterval = [36.941 ms; 39.027 ms] (CI 99.9%), Margin = 1.043 ms (2.75% of Mean)
Skewness = 0.83, Kurtosis = 3.06, MValue = 4.14
```

### Hash_1MB_File Statistics
```
Mean = 340.901 ms, StdErr = 5.619 ms (1.65%), N = 88, StdDev = 52.708 ms
Min = 207.405 ms, Q1 = 321.484 ms, Median = 344.431 ms, Q3 = 366.040 ms, Max = 475.880 ms
IQR = 44.556 ms, LowerFence = 254.649 ms, UpperFence = 432.875 ms
ConfidenceInterval = [321.765 ms; 360.038 ms] (CI 99.9%), Margin = 19.137 ms (5.61% of Mean)
Skewness = -0.44, Kurtosis = 3.96, MValue = 2.32
```

### Hash_50MB_File Statistics
```
Mean = 17.487 s, StdErr = 0.100 s (0.57%), N = 49, StdDev = 0.698 s
Min = 15.887 s, Q1 = 17.069 s, Median = 17.441 s, Q3 = 17.963 s, Max = 18.926 s
IQR = 0.895 s, LowerFence = 15.727 s, UpperFence = 19.306 s
ConfidenceInterval = [17.138 s; 17.836 s] (CI 99.9%), Margin = 0.349 s (2.00% of Mean)
Skewness = 0.03, Kurtosis = 2.43, MValue = 2
```

## Recommendations

### Immediate Actions
1. **Accept 3x improvement** - Significant gain over v1.3.7 baseline
2. **Document actual performance** - Update README with real-world numbers (3x, not 8-16x)
3. **Release v1.4.0 as-is** - Batch API provides value despite lower-than-expected speedup

### Future Improvements (v1.5.0)
1. **Profile with ETW** - Identify actual bottleneck (I/O vs. CPU vs. synchronization)
2. **Test sequential vs. parallel** - Measure actual benefit of Parallel.ForEach
3. **Optimize memory** - Investigate 25x allocation ratio (should be ~5x)
4. **Consider alternative strategies:**
   - Chunked parallel processing (process multiple buffers in parallel)
   - Pipeline parallelism (overlap I/O with computation)
   - Per-algorithm caching (reduce repeated allocations)

---

**Benchmark Report Generated:** February 5, 2026  
**Tool:** BenchmarkDotNet v0.15.8  
**Full Results:** `BenchmarkDotNet.Artifacts/results/HashNow.Benchmarks.PerformanceProfiler-report.html`

