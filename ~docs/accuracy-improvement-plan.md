# HashNow Accuracy & Test Coverage Improvement Plan

## Current Test Coverage

- **~95 test cases** across 3 test files
- FileHasherTests.cs: Unit tests for all 70 algorithms
- FileHashingIntegrationTests.cs: Integration tests (empty, small, large, concurrent)
- HashTestVectorTests.cs: NIST/reference test vectors

## Identified Coverage Gaps

### Priority 1: Additional Test Vectors

**Gap:** Only a few key algorithms (MD5, SHA-1, SHA-256, CRC32) have NIST/reference test vectors validated.

**Plan:** Add test vectors for ALL 70 algorithms from authoritative sources:

- **Cryptographic:** NIST FIPS test vectors for all SHA variants
- **BLAKE family:** Official test vectors from BLAKE spec authors
- **Non-crypto:** Reference implementations (SmHasher for MurmurHash, Google for CityHash/FarmHash)
- **Checksums:** RFC vectors for CRC32/CRC32C, reference for Adler-32/Fletcher

### Priority 2: Edge Case Testing

**Gap:** Limited edge case coverage.

**Tests to add:**

- Zero-length files (already covered, verify all 70)
- Single byte files (0x00, 0x01, 0x7f, 0x80, 0xff)
- Boundary size files (buffer boundary: 1MB-1, 1MB, 1MB+1)
- Files with all identical bytes (all zeros, all 0xff)
- Very large files (>2GB, tests int32 overflow in counters)
- Files on network drives (UNC paths)
- Read-only files
- Files with special characters in names (unicode, spaces, emoji)

### Priority 3: Cross-Reference Validation

**Gap:** Limited cross-validation between StreamHash and independent libraries.

**Plan:** For each algorithm, compute hash using:

1. StreamHash (production path)
2. Independent library (BouncyCastle, System.Security.Cryptography, etc.)
3. Compare byte-for-byte

### Priority 4: Streaming Consistency Tests

**Gap:** No tests verifying that streaming produces identical results to one-shot hashing.

**Plan:** For each algorithm, verify:

- `FileHasher.ComputeXxx(fullData)` == streaming chunked result
- Different chunk sizes produce same hash
- Chunk sizes: 1 byte, 7 bytes, 64 bytes, 1024 bytes, 1MB

### Priority 5: Regression Test Infrastructure

**Plan:** Create a golden test file with pre-computed hashes for reference files:

- `tests/HashNow.Core.Tests/ReferenceData/abc.bin` (3 bytes: "abc")
- `tests/HashNow.Core.Tests/ReferenceData/golden-hashes.json` (all 70 hashes pre-computed)
- Test validates all 70 hashes match golden file exactly

## Acceptance Criteria

1. All 70 algorithms have at least 3 independent test vectors each
2. Edge cases covered for all boundary conditions
3. Cross-reference validation against independent implementations
4. Golden file regression test prevents any hash output changes
5. Streaming consistency verified for all algorithms
