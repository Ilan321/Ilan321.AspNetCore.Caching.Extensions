using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Ilan321.AspNetCore.Caching.Extensions
{
    public static class DistributedCacheExtensions
    {
        /// <summary>
        /// Gets the item associated with this key if present.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="distributedCache">The instance of the <see cref="IDistributedCache"/>.</param>
        /// <param name="key">A string identifying the requested entry.</param>
        /// <param name="jsonOptions">The <see cref="JsonSerializerOptions"/> to use when deserializing the value.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to use for canceling the task.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task<T> GetAsync<T>(
            this IDistributedCache distributedCache,
            string key,
            JsonSerializerOptions jsonOptions = default,
            CancellationToken cancellationToken = default
        )
        {
            if (distributedCache == null)
            {
                throw new ArgumentNullException(nameof(distributedCache));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var bytes = await distributedCache.GetAsync(key, cancellationToken);

            if (bytes is null)
            {
                return default;
            }

            var jsonString = Encoding.UTF8.GetString(bytes);

            return JsonSerializer.Deserialize<T>(jsonString, jsonOptions);
        }

        /// <summary>
        /// Creates or overwrites the specified entry in the cache.
        /// </summary>
        /// <typeparam name="T">The type of the value to set.</typeparam>
        /// <param name="distributedCache">The instance of the <see cref="IDistributedCache"/>.</param>
        /// <param name="key">The entry to create or overwrite.</param>
        /// <param name="value">The value or null.</param>
        /// <param name="options">The options for the entry.</param>
        /// <param name="jsonOptions">The <see cref="JsonSerializerOptions"/> to use when serializing the value.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to use for canceling the task.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Task SetAsync<T>(
            this IDistributedCache distributedCache,
            string key,
            T value,
            DistributedCacheEntryOptions options,
            JsonSerializerOptions jsonOptions = default,
            CancellationToken cancellationToken = default
        )
        {
            if (distributedCache == null)
            {
                throw new ArgumentNullException(nameof(distributedCache));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var jsonString = JsonSerializer.Serialize(value, jsonOptions);

            var bytes = Encoding.UTF8.GetBytes(jsonString);

            return distributedCache.SetAsync(key, bytes, options, cancellationToken);
        }

        /// <summary>
        /// Gets or creates a value from the <see cref="IDistributedCache"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="distributedCache">The instance of the <see cref="IDistributedCache"/>.</param>
        /// <param name="key">The entry to get or create.</param>
        /// <param name="factory">The asynchronous action to call when the entry isn't found.</param>
        /// <param name="jsonOptions">The <see cref="JsonSerializerOptions"/> to use when serializing the value (if not found).</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to use for canceling the task.</param>
        /// <returns>The value found in the <see cref="IDistributedCache"/> if found, otherwise the value returned by <paramref name="factory"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="distributedCache"/>, <paramref name="key"/>, or <paramref name="factory"/> are null.</exception>
        public static async Task<T> GetOrCreateAsync<T>(
            this IDistributedCache distributedCache,
            string key,
            Func<DistributedCacheEntryOptions, Task<T>> factory,
            JsonSerializerOptions jsonOptions = default,
            CancellationToken cancellationToken = default
        )
        {
            if (distributedCache is null)
            {
                throw new ArgumentNullException(nameof(distributedCache));
            }

            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (factory is null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            var value = await distributedCache.GetAsync<T>(key, jsonOptions, cancellationToken);

            if (value == null)
            {
                var entry = new DistributedCacheEntryOptions();

                value = await factory(entry);

                await distributedCache.SetAsync(key, value, entry, jsonOptions, cancellationToken);
            }

            return value;
        }
    }
}
