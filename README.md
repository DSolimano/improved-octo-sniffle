# improved-octo-sniffle
A cache test with a suggested name

# Functional requirements

Please see test suite which exercises and provides examples for all functional requirements.  From a performance perspective, it also generates random test data and runs benchmarks to ensure we meet our performance goals.

# Tradeoffs in performance/memory usage

The implemenation that I've chosen is very lazy, and uses extra space.  I preallocate a hash table the maximum size that we know it can grow to.  What this ensures is that we will never have to hold up a read to do a rehash.
