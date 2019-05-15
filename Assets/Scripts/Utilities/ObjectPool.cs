using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DoubleMMPrjc
{
    namespace Utility
    {
        public class ObjectPool<T> : IEnumerable<T> where T : MonoBehaviour
        {
            private bool canGrow = true;
            private GameObject parent;
            private T prefab;
            private List<T> pooledObjects;

            public ObjectPool(T prefab, string parentName = null) : this( prefab, 20, parentName )
            {
            }

            public ObjectPool(T prefab, int size, string parentName = null) : this( prefab, size, false, parentName )
            {
            }

            public ObjectPool(T prefab, int size, bool canGrow, string parentName = null)
            {
                if (size <= 0) {
                    throw new System.ArgumentException( "Size cannot be 0 or less" );
                }

                this.prefab = prefab;
                this.canGrow = canGrow;

                pooledObjects = new List<T>( size );
;
                if (parentName != null) {
                    parent = new GameObject( parentName );
                }

                for (int i = 0; i < size; i++) {
                    T objectPool = null;
                    if (parent != null) {
                        objectPool = GameObject.Instantiate( prefab, parent.transform );
                    } else {
                        objectPool = GameObject.Instantiate( prefab );
                    }
                    objectPool.gameObject.SetActive( false );
                    pooledObjects.Add( objectPool );
                }
            }

            public T GetPooledObject()
            {
                foreach (T pooledObject in pooledObjects) {
                    if (!pooledObject.gameObject.activeInHierarchy) {
                        pooledObject.gameObject.SetActive( true );
                        return pooledObject;
                    }
                }
                if (canGrow) {
                    T gameObject = GameObject.Instantiate( prefab );
                    pooledObjects.Add( gameObject );
                    return gameObject;
                }
                return null;
            }

            public T GetPooledObject(Vector2 position)
            {
                return GetPooledObject( position, Quaternion.identity );
            }

            public T GetPooledObject(Vector2 position, Quaternion rotation)
            {
                foreach (T pooledObject in pooledObjects) {
                    if (!pooledObject.gameObject.activeInHierarchy) {
                        pooledObject.gameObject.SetActive( true );
                        pooledObject.transform.position = position;
                        pooledObject.transform.rotation = rotation;
                        return pooledObject;
                    }
                }
                if (canGrow) {
                    T gameObject = GameObject.Instantiate( prefab, position, rotation );
                    pooledObjects.Add( gameObject );
                    return gameObject;
                }
                return null;
            }

            public void Clear()
            {
                foreach (T t in pooledObjects) {
                    GameObject.Destroy( t.gameObject );
                }
                if ( parent ) {
                    GameObject.Destroy( parent );
                }
                pooledObjects.Clear();
            }

            public IEnumerator<T> GetEnumerator()
            {
                return pooledObjects.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                foreach (T t in pooledObjects) {
                    yield return t;
                }
            }

            public int Size { get => pooledObjects.Count; }
            public bool CanGrow { get => canGrow; set => canGrow = value; }
            public T Prefab { get => prefab; }

            /* public class ObjectPoolEnum<T> : IEnumerator<T> where T : MonoBehaviour
             {
                 private ObjectPoolEnum<T>(List<T> list) {
                     }
             }*/
        }
    }
}