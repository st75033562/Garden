using System;
using UnityEngine;

namespace DataAccess
{
    public class ResourceDataSource : IDataSource
    {
        private ResourceDataSource()
        {
        }

        public static readonly ResourceDataSource instance = new ResourceDataSource();

        public string Get(string tableName)
        {
            var asset = Resources.Load<TextAsset>("Data/" + tableName);
            if (!asset)
            {
                throw new ArgumentException("invalid table: " + tableName);
            }
            return asset.text;
        }
    }
}
