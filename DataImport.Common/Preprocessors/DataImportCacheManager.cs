// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;

namespace DataImport.Common.Preprocessors
{
    public class DataImportCacheManager
    {
        private static readonly Dictionary<string, Dictionary<string, object>> _caches = new Dictionary<string, Dictionary<string, object>>();

        private static readonly object _syncRoot = new object();

        private DataImportCacheManager() { }

        public static Dictionary<string, object> ResolveCache(string cacheIdentifier)
        {
            lock (_syncRoot)
            {
                if (_caches.ContainsKey(cacheIdentifier))
                {
                    return _caches[cacheIdentifier];
                }

                var cache = new Dictionary<string, object>();
                _caches[cacheIdentifier] = cache;

                return cache;
            }
        }

        public static void DestroyCache(string cacheIdentifier)
        {
            lock (_syncRoot)
            {
                if (_caches.ContainsKey(cacheIdentifier))
                {
                    _caches[cacheIdentifier] = null;
                }
            }
        }
    }
}
