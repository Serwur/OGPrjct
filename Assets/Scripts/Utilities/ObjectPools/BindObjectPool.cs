using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ColdCry.Utility
{
    public class BindObjectPool<T> : ObjectPool<T> where T : MonoBehaviour
    {
        public BindObjectPool(T prefab, string parentName = null) : base( prefab, parentName )
        {
        }

        public BindObjectPool(T prefab, int size, string parentName = null) : base( prefab, size, parentName )
        {
        }

        public BindObjectPool(T prefab, int size, Transform parent) : base( prefab, size, parent )
        {
        }

        public BindObjectPool(T prefab, int size, bool canGrow, string parentName = null) : base( prefab, size, canGrow, parentName )
        {
        }

        public BindObjectPool(T prefab, int size, bool canGrow, Transform parent) : base( prefab, size, canGrow, parent )
        {
        }
    }
}
