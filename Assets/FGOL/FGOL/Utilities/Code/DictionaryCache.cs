using System.Collections.Generic;

public class DictionaryCache<T, U>
{
	//------------------------------------------------------------
	// Private Variables:
	//------------------------------------------------------------

	private int m_maxCacheDimension;
	private Dictionary<T, U> m_cache;
	private List<T> m_cachePriorityList;

	//------------------------------------------------------------
	// Constructors:
	//------------------------------------------------------------

	/// <summary>
	/// Create a cache of given dimensions.
	/// </summary>
	/// <param name="maxCacheDimension">If the max dimension of the cache is 0 or negative, the cache will never discard the oldest items.</param>
	public DictionaryCache(int maxCacheDimension)
	{
		// we want this cache to be used only with U being nullable types.
		FGOL.Assert.Fatal(default(U) == null, "U is required to be a nullable type !!!");

		m_maxCacheDimension = maxCacheDimension;
		m_cache = new Dictionary<T, U>();
		if(maxCacheDimension > 0)
		{
			m_cachePriorityList = new List<T>();
		}
	}

	//------------------------------------------------------------
	// Public Methods:
	//------------------------------------------------------------

	/// <summary>
	/// This methods caches the new item passed using the given key. It also assign to the out param the oldest discarded element (null if nothing has been removed).
	/// </summary>
	/// <param name="key">The key to use to store the item.</param>
	/// <param name="item">The item to store.</param>
	/// <param name="oldestItem">The oldest item removed, null if nothing was.</param>
	public void CacheItem(T key, U item, out U oldestItem)
	{
		// we want this cache to be used only with U being nullable types.
		FGOL.Assert.Fatal(default(U) == null, "U is required to be a nullable type !!!");

		// check the max size and remove oldest elements if necessary.
		oldestItem = default(U);
		if(m_maxCacheDimension > 0 && m_cache.Keys.Count >= m_maxCacheDimension)
		{
			T toRemove = m_cachePriorityList[m_cachePriorityList.Count - 1];
			m_cachePriorityList.Remove(toRemove);
			oldestItem = m_cache[toRemove];
			m_cache.Remove(toRemove);
		}
		// cache the new item.
		m_cache[key] = item;
		// add the new item into the list of priorities if necessary.
		if(m_maxCacheDimension > 0)
		{
			m_cachePriorityList.Remove(key);
			m_cachePriorityList.Insert(0, key);
		}
	}

	/// <summary>
	/// It will returns the item associated to the passed key. Null if not contained.
	/// </summary>
	/// <param name="key">The key used to store the item.</param>
	/// <returns>The item associated to the passed key. Null if nothing stored.</returns>
	public U GetItem(T key)
	{
		// we want this cache to be used only with U being nullable types.
		FGOL.Assert.Fatal(default(U) == null, "U is required to be a nullable type !!!");

		// let's check if the item is in the cache.
		U item = default(U);
		m_cache.TryGetValue(key, out item);
		// update the priority list only if the cache has a valid max dimension.
		if(m_maxCacheDimension > 0 && item != null)
		{
			// change the list priority.
			m_cachePriorityList.Remove(key);
			m_cachePriorityList.Insert(0, key);
		}
		return item;
	}

	/// <summary>
	/// This method will reset the cache returning a list with the discarded items.
	/// </summary>
	/// <returns>The list of the items contained in the cache.</returns>
	public List<U> Reset()
	{
		// get a collection of the items that need to be discarder.
		List<U> discarded = new List<U>();
		foreach(KeyValuePair<T, U> entry in m_cache)
		{
			// add to the list only the not null elements of the cache.
			if(entry.Value != null)
			{
				discarded.Add(entry.Value);
			}
		}
		// clear the cache.
		m_cachePriorityList.Clear();
		m_cache.Clear();
		// return the collection with the discarded items.
		return discarded;
	}
}