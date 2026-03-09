# HashNow Performance Optimization Plan

## Current State

- **70 algorithms** computed in single pass via StreamHash batch API
- **1MB buffer** with ArrayPool memory management
- **Throughput:** ~800 MB/s for all 70 algorithms on 10MB file
- **Memory:** Minimal allocations via ArrayPool

## Identified Optimization Opportunities

### Priority 1: JSON Output Performance

**Problem:** `SaveResultAsync` uses `Regex.Replace` 4x to format JSON output with tab indentation.

**Solution:** Use custom `JsonSerializerOptions` with `WriteIndented = true` and a custom `IndentCharacter = '\t'` instead of post-processing regex replacements.

**Expected Impact:** ~10-20% faster JSON serialization for large result files.

### Priority 2: Progress Callback Throttling

**Problem:** Progress callback is invoked on every 1MB chunk read, which means for a 100MB file, 100 callbacks are fired. For GUI mode with UI thread marshalling, this adds measurable overhead.

**Solution:** Throttle progress callbacks to max 10-20 per second, or only report when percentage changes by >= 1%.

**Expected Impact:** Reduced UI overhead on large files, especially in GUI mode.

### Priority 3: FileInfo Caching

**Problem:** FileInfo is accessed multiple times for the same file (size, creation time, modification time).

**Solution:** Create FileInfo once and reuse it across all property accesses.

**Expected Impact:** Minor I/O reduction.

### Priority 4: Benchmark Regression Suite

**Problem:** No automated benchmark regression tracking. Performance could degrade unnoticed across releases.

**Solution:** Add benchmark baseline tracking with BenchmarkDotNet comparison. Store baseline results and compare on each run.

**Expected Impact:** Prevents performance regressions.

## Benchmark Matrix

| Scenario | File Size | Metric |
|----------|-----------|--------|
| All 70 algorithms | 1KB | Latency (ms) |
| All 70 algorithms | 1MB | Throughput (MB/s) |
| All 70 algorithms | 10MB | Throughput (MB/s) |
| All 70 algorithms | 100MB | Throughput (MB/s) |
| Category: Checksums only | 10MB | Throughput (MB/s) |
| Category: Crypto only | 10MB | Throughput (MB/s) |
| Category: Non-crypto only | 10MB | Throughput (MB/s) |
| JSON serialization | N/A | Latency (ms) |
| Progress reporting overhead | 100MB | % overhead vs no-progress |

## Acceptance Criteria

1. All xUnit tests pass after every change
2. No hash output changes (accuracy is sacred)
3. Measurable improvement shown by BenchmarkDotNet
4. No new compiler warnings
