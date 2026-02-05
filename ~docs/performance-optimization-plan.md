# HashNow Performance Optimization Plan

**Created:** January 26, 2026  
**Status:** Planning / Investigation  
**GitHub Issues:** #12 (HashNow), #17 (StreamHash)

## 📊 Current Performance Baseline

### Benchmark Results (Hardware: Older system with onboard graphics)

| File Size | Mean Time | Throughput | Allocated Memory | Memory Overhead |
|-----------|-----------|------------|------------------|-----------------|
| 100 KB    | ~0.010s   | ~10 MB/s   | ~2.4 MB          | 24x |
| 1 MB      | 1.027s    | ~0.97 MB/s | 24.14 MB         | 24x |
| 50 MB     | 52.58s    | ~0.95 MB/s | 1.18 GB          | 24x |

**Problems:**
- ❌ **Memory overhead:** 24x file size (1.18 GB for 50 MB file)
- ❌ **Throughput:** ~0.95 MB/s vs documented 200-300 MB/s (200-300x slower)
- ❌ **GC pressure:** Gen0=201,000, Gen1=7,000, Gen2=1,000 collections for 50MB file
- ❌ **CPU overhead:** 70 sequential `Update()` calls per 1MB buffer chunk

## 🔍 Root Cause Analysis

### Current Architecture

```csharp
// StreamingHasher.cs - ProcessChunk() method
public void ProcessChunk(ReadOnlySpan<byte> data) {
	// PROBLEM: 70 sequential Update() calls with shared buffer
	foreach (var hasher in _hashers.Values) {
		hasher.Update(data);  // Called 70 times per chunk!
	}
}
```

**Why This Is Slow:**
1. **70x Function Call Overhead:** Each `Update()` has stack frame setup/teardown
2. **70x Memory Copies:** Each hasher may copy the span internally
3. **70x State Updates:** Sequential state mutations prevent CPU optimizations
4. **Cache Thrashing:** Switching between 70 hasher states evicts L1/L2 cache
5. **No Parallelization:** Sequential loop prevents SIMD/multi-core use

### Memory Allocation Breakdown

```
Total Allocation = 70 hashers × (internal state + buffers)
                 = 70 × ~345 KB
                 = ~24 MB base + file size worth of copying
```

## 🎯 Optimization Goals

| Metric | Current | Target | Improvement |
|--------|---------|--------|-------------|
| 50MB File Time | 52.58s | < 5s | 10x faster |
| Throughput | 0.95 MB/s | > 50 MB/s | 50x faster |
| Memory Overhead | 24x | < 4x | 6x reduction |
| Gen0 Collections | 201,000 | < 5,000 | 40x reduction |
| Gen2 Collections | 1,000 | < 10 | 100x reduction |

## 🚀 Proposed Solutions

### Phase 1: StreamHash Batch API (Issue #17)

**Add `HashFacade.UpdateAllStreaming()` for batch updates:**

```csharp
// New API in StreamHash
public static class HashFacade {
	/// <summary>
	/// Creates a batch streaming context for multiple algorithms.
	/// Updates all algorithms with a single memory pass.
	/// </summary>
	public static IMultiStreamingHashBytes CreateAllStreaming(
		HashAlgorithmSet algorithms = HashAlgorithmSet.All) {
		// Returns a single object that updates 70+ algorithms efficiently
	}
}

public interface IMultiStreamingHashBytes : IDisposable {
	void Update(ReadOnlySpan<byte> data);  // Single call updates ALL hashers
	Dictionary<string, string> FinalizeAll();  // Returns all hashes at once
	void Reset();
}

// Internal implementation
internal class MultiStreamingHashBytes : IMultiStreamingHashBytes {
	private readonly List<IStreamingHashBytes> _hashers;
	
	public void Update(ReadOnlySpan<byte> data) {
		// OPTIMIZATION: Parallel.ForEach with shared buffer
		Parallel.ForEach(_hashers, hasher => hasher.Update(data));
		
		// OR: SIMD-optimized single-pass multi-hash
		// UpdateAllSimd(data, _hashers);
	}
}
```

**Benefits:**
- ✅ Single function call overhead instead of 70
- ✅ Parallel processing on multi-core CPUs
- ✅ Shared buffer reduces memory copies
- ✅ Cache-friendly (process all hashers in parallel batches)

### Phase 2: HashNow Integration (Issue #12)

**Update `StreamingHasher` to use batch API:**

```csharp
// StreamingHasher.cs - Updated implementation
public class StreamingHasher : IDisposable {
	private readonly IMultiStreamingHashBytes _batchHasher;
	
	public StreamingHasher() {
		// Use new batch API from StreamHash
		_batchHasher = HashFacade.CreateAllStreaming();
	}
	
	public void ProcessChunk(ReadOnlySpan<byte> data) {
		// Single call updates all 70 hashers efficiently!
		_batchHasher.Update(data);
	}
	
	public FileHashResult GetResult(FileInfo fileInfo) {
		var hashes = _batchHasher.FinalizeAll();
		return new FileHashResult { Hashes = hashes, ... };
	}
}
```

**Benefits:**
- ✅ Minimal code changes in HashNow
- ✅ Automatic performance improvements
- ✅ Future StreamHash optimizations benefit HashNow for free

### Phase 3: Advanced Optimizations (Future)

#### Option A: SIMD Multi-Hash

```csharp
// Process 4-8 hashers simultaneously using AVX2/AVX-512
unsafe void UpdateAllSimd(ReadOnlySpan<byte> data, Span<IStreamingHashBytes> hashers) {
	fixed (byte* ptr = data) {
		for (int i = 0; i < hashers.Length; i += 8) {
			// Load 8x hashers' states into SIMD registers
			// Process data in parallel using AVX2
			// Store updated states back
		}
	}
}
```

#### Option B: GPU Acceleration (HashNow.Cuda)

```csharp
// Offload hash computation to GPU for massive files (>100MB)
// Use CUDA/OpenCL to compute all 70 hashes in parallel
```

#### Option C: Memory-Mapped Files

```csharp
// For files > 100 MB, use memory-mapped I/O
// Let OS handle paging instead of loading entire file
using var mmf = MemoryMappedFile.CreateFromFile(filePath);
using var accessor = mmf.CreateViewAccessor();
// Process in 1MB chunks without allocating huge buffers
```

## 📋 Implementation Roadmap

### Week 1: StreamHash Batch API (Issue #17)
- [ ] Design `IMultiStreamingHashBytes` interface
- [ ] Implement `MultiStreamingHashBytes` with parallel processing
- [ ] Add `HashFacade.CreateAllStreaming()` method
- [ ] Write unit tests (all 71 algorithms return correct results)
- [ ] Benchmark: Measure memory reduction and throughput improvement
- [ ] Document new API in README

### Week 2: HashNow Integration (Issue #12)
- [ ] Update `StreamingHasher` to use batch API
- [ ] Remove old sequential loop in `ProcessChunk()`
- [ ] Update NuGet dependency on StreamHash
- [ ] Run benchmarks: Verify 10x+ speedup and 6x memory reduction
- [ ] Update README with accurate performance numbers

### Week 3: Testing & Release
- [ ] Full regression testing (108+ tests)
- [ ] Performance validation on different hardware
- [ ] Update CHANGELOG.md with performance improvements
- [ ] Release HashNow v1.4.0 (major performance release)
- [ ] Release StreamHash v1.7.0 (batch API)

## 🧪 Testing Strategy

### Unit Tests
- All 71 algorithms produce correct hashes (compare to v1.6.3 baseline)
- Batch API matches individual hasher results
- Parallel processing doesn't corrupt state
- Memory cleanup (no leaks, proper Dispose)

### Performance Tests
- 100KB file: < 0.005s (50% faster)
- 1MB file: < 0.1s (10x faster)
- 50MB file: < 5s (10x faster)
- Memory: < 200MB for 50MB file (6x reduction)

### Hardware Validation
- Test on: Modern PC (12+ cores), older PC (4 cores), laptop (2 cores)
- Verify scaling: More cores = faster hashing
- Verify no regression on single-core systems

## 📊 Success Metrics

| Metric | Before | After | Status |
|--------|--------|-------|--------|
| 50MB Time | 52.58s | < 5s | 🚧 Pending |
| Throughput | 0.95 MB/s | > 50 MB/s | 🚧 Pending |
| Memory | 1.18 GB | < 200 MB | 🚧 Pending |
| Gen0 GC | 201,000 | < 5,000 | 🚧 Pending |
| Tests Passing | 108/108 | 108/108 | ✅ |

## 🔗 Related Issues

- **HashNow #12:** [Performance] Optimize streaming hasher to reduce 24x memory overhead
- **StreamHash #17:** [Performance] Add HashFacade.UpdateAllStreaming() for batch updates
- **HashNow #11:** [Performance] 50MB file takes 52.58s to hash (original report)

## 📝 Notes

- **No new releases until optimization complete** - User requested batching fixes instead of incremental releases
- **Benchmark on older hardware** - Current benchmarks are on old PC with onboard graphics
- **Document accurate performance** - Remove "200-300 MB/s" claim from README until achieved
- **Backward compatibility** - Keep existing APIs, add new batch API as opt-in

---

**Last Updated:** January 26, 2026  
**Next Review:** After Phase 1 (StreamHash batch API) completion
