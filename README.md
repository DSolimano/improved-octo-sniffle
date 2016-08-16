# improved-octo-sniffle
A cache test with a suggested name

The external interface is defined in IExpiringHashTable<K, V>, which is implemented by SpikeExpiringHashTable<K, V> as a first cut.  It makes certain performance/memory tradeoffs, so it should be hidden behind an interface for ease of swapping implementations.  If we were feeling fancy, we could create a factory where you passed in your use case via some paramaters and it returned the best implementation for your case.

# Functional requirements

Please see test suite which exercises and provides examples for all functional requirements.  From a performance perspective, it also generates random test data and runs benchmarks to ensure we meet our performance goals.

# Tradeoffs in performance/memory usage

The implemenation that I've chosen is very lazy, and uses extra space.  I preallocate a hash table the maximum size that we know it can grow to.  What this ensures is that we will never have to hold up a read on one thread while another thread is adding and causing an expensive rehash.  Probably, rehashes would not have impacted our 99% under 5ms requirement, but with this implementation it ceratinly will not.

Reads are slightly slower than they need to be because we virtually remove data from the hash table.  That is, we never remove actual values, but if we come across an expired value in a read, we throw a key not found exception, as trying to read from the underlying IDictionary would do.  This introduces another problem, insofar as reads of nonexistent keys are expensive as we have to use the exception mechanism.  If we were to define another mechanism here, such as only allowing non-null class objects to be values and returning null when a key isn't found, this would be faster.

There's another waste of memory going on with the structure we use to manage expiry.  Each value is wrapped in a class that stores the actual value and a datetime representing the expiration.  Ideally, for non-expiring values, we could have the wrapper class not store a datetime at all.  This simple optimization is left as an exercise to the reader.
