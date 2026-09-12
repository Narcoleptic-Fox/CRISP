// Integration tests share one MySQL database, so they must not run concurrently — a reset in one
// test would wipe data another is mid-way through. The unit tests are fast enough that losing
// parallelism across the assembly does not matter.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
