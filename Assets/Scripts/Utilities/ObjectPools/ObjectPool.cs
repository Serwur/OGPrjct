using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColdCry.Utility
{

    public class ObjectPool<T> : IEnumerable<T> where T : MonoBehaviour
    {
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

            this.Prefab = prefab;
            this.CanGrow = canGrow;

            PooledObjects = new List<T>( size );

            if (parentName != null) {
                Parent = new GameObject( parentName );
            }

            for (int i = 0; i < size; i++) {
                T objectPool = null;
                if (Parent != null) {
                    objectPool = GameObject.Instantiate( prefab, Parent.transform );
                } else {
                    objectPool = GameObject.Instantiate( prefab );
                }
                objectPool.gameObject.SetActive( false );
                PooledObjects.Add( objectPool );
            }
        }

        public ObjectPool(T prefab, int size, Transform parent) : this( prefab, size, false, parent )
        {

        }

        public ObjectPool(T prefab, int size, bool canGrow, Transform parent)
        {
            if (size <= 0) {
                throw new System.ArgumentException( "Size cannot be 0 or less" );
            }

            this.Prefab = prefab;
            this.CanGrow = canGrow;

            PooledObjects = new List<T>( size );

            for (int i = 0; i < size; i++) {
                T objectPool = null;
                if (parent != null) {
                    objectPool = GameObject.Instantiate( prefab, parent.transform );
                } else {
                    objectPool = GameObject.Instantiate( prefab );
                }
                objectPool.gameObject.SetActive( false );
                objectPool.transform.parent = parent;
                PooledObjects.Add( objectPool );
            }
        }

        public virtual T Get()
        {
            foreach (T pooledObject in PooledObjects) {
                if (!pooledObject.gameObject.activeInHierarchy) {
                    pooledObject.gameObject.SetActive( true );
                    return pooledObject;
                }
            }
            if (CanGrow) {
                T gameObject = GameObject.Instantiate( Prefab );
                PooledObjects.Add( gameObject );
                return gameObject;
            }
            return null;
        }

        public virtual T Get(Vector2 position)
        {
            return Get( position, Quaternion.identity );
        }

        public virtual T Get(Vector2 position, Quaternion rotation)
        {
            foreach (T pooledObject in PooledObjects) {
                if (!pooledObject.gameObject.activeInHierarchy) {
                    pooledObject.gameObject.SetActive( true );
                    pooledObject.transform.position = position;
                    pooledObject.transform.rotation = rotation;
                    return pooledObject;
                }
            }

            if (CanGrow) {
                T gameObject = GameObject.Instantiate( Prefab, position, rotation );
                PooledObjects.Add( gameObject );
                return gameObject;
            }
            return null;
        }

        public virtual void Return(T obj)
        {
            if (!PooledObjects.Contains( obj ))
                throw new System.ArgumentException( "Object doesn't belong to pool" );
            obj.gameObject.SetActive( false );
        }

        public virtual void ReturnToParent(T obj)
        {
            obj.transform.parent = Parent.transform;
            Return( obj );
        }

        public virtual void Clear()
        {
            foreach (T t in PooledObjects) {
                GameObject.Destroy( t.gameObject );
            }
            if (Parent) {
                GameObject.Destroy( Parent );
            }
            PooledObjects.Clear();
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            return PooledObjects.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (T t in PooledObjects) {
                yield return t;
            }
        }

        public int Size { get => PooledObjects.Count; }
        public T Prefab { get; protected set; }
        public bool CanGrow { get; protected set; } = true;
        public GameObject Parent { get; protected set; }
        protected List<T> PooledObjects { get; set; }
    }

}