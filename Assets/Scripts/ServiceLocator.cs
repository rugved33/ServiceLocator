using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityServiceLocator
{
    public class ServiceLocator : MonoBehaviour
    {
        static ServiceLocator global;
        static Dictionary<Scene, ServiceLocator> sceneContainers;
        static List<GameObject> tmpSceneGameObjects;

        private readonly ServiceManager _services = new ServiceManager();

        private const string GLOBAL_SERVICE_LOCATOR_NAME = "ServiceLocator Global";
        private const string SCENE_SERVICE_LOCATOR_NAME = "Service Locator Scene";

        internal void ConfigureAsGlobal(bool dontDestroyOnLoad)
        {
            if (global == this)
            {
                Debug.LogWarning("ServiceLocator.ConfigureAsGlobal: Already configured as global", this);
            }
            else if (global != null)
            {
                Debug.LogError("ServiceLocator.ConfigureAsGlobal: Another ServiceLocator is already configured as global", this);
            }
            else
            {
                global = this;
                if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);
            }
        }

        internal void ConfigureForScene()
        {
            Scene scene = gameObject.scene;

            if (sceneContainers.ContainsKey(scene))
            {
                Debug.LogError("ServiceLocator.ConfigureForScene : Another Service locator is already registered");
                return;
            }
            sceneContainers.Add(scene, this);
        }

        /// <summary>
        /// Gets the global service locator. Creates new if none exists
        /// </summary>
        /// <value></value>
        public static ServiceLocator Global
        {
            get
            {

                if (global != null) return global;

                if (FindFirstObjectByType<ServiceLocatorGlobal>() is { } found)
                {
                    found.BootstrapOnDemand();
                    return global;
                }

                var container = new GameObject(GLOBAL_SERVICE_LOCATOR_NAME, typeof(ServiceLocator));
                container.AddComponent<ServiceLocatorGlobal>().BootstrapOnDemand();

                return global;
            }
        }

        /// <summary>
        /// Returns the <see cref="ServiceLocator"/> configured for the scene of a MonoBehaviour. Falls back to the global instance.
        /// </summary>
        public static ServiceLocator ForSceneOf(MonoBehaviour mb)
        {
            Scene scene = mb.gameObject.scene;


            if (sceneContainers.TryGetValue(scene, out ServiceLocator container) && container != mb)
            {
                return container;
            }

            tmpSceneGameObjects.Clear();
            scene.GetRootGameObjects(tmpSceneGameObjects);


            foreach (GameObject go in tmpSceneGameObjects.Where(go => go.GetComponent<ServiceLocatorScene>() != null))
            {
                if (go.TryGetComponent(out ServiceLocatorScene bootStrapper) && bootStrapper.Container != mb)
                {
                    bootStrapper.BootstrapOnDemand();
                    return bootStrapper.Container;
                }
            }

            return global;
        }

        /// <summary>
        /// Gets the closest ServiceLocator instance to the provided 
        /// MonoBehaviour in hierarchy, the ServiceLocator for its scene, or the global ServiceLocator.
        /// </summary>
        public static ServiceLocator For(MonoBehaviour mb)
        {
            return mb.GetComponentInParent<ServiceLocator>().OrNull() ?? ForSceneOf(mb) ?? Global;
        }

        /// <summary>
        /// Registers a service to the ServiceLocator using the service's type.
        /// </summary>
        /// <param name="service">The service to register.</param>  
        /// <typeparam name="T">Class type of the service to be registered.</typeparam>
        /// <returns>The ServiceLocator instance after registering the service.</returns>
        public ServiceLocator Register<T>(T service)
        {
            _services.RegisterService(service);
            return this;
        }

        /// <summary>
        /// Registers a service to the ServiceLocator using a specific type.
        /// </summary>
        /// <param name="type">The type to use for registration.</param>
        /// <param name="service">The service to register.</param>  
        /// <returns>The ServiceLocator instance after registering the service.</returns>
        public ServiceLocator Register(Type type, object service)
        {
            _services.Register(type, service);
            return this;
        }

        /// <summary>
        /// Gets a service of a specific type. If no service of the required type is found, an error is thrown.
        /// </summary>
        /// <param name="service">Service of type T to get.</param>  
        /// <typeparam name="T">Class type of the service to be retrieved.</typeparam>
        /// <returns>The ServiceLocator instance after attempting to retrieve the service.</returns>
        public ServiceLocator Get<T>(out T service) where T : class
        {
            if (TryGetService(out service)) return this;

            if (TryGetNextInHierarchy(out ServiceLocator container))
            {
                container.Get(out service);
                return this;
            }

            throw new ArgumentException($"ServiceLocator.Get: Service of type {typeof(T).FullName} not registered");
        }

        /// <summary>
        /// Allows retrieval of a service of a specific type. An error is thrown if the required service does not exist.
        /// </summary>
        /// <typeparam name="T">Class type of the service to be retrieved.</typeparam>
        /// <returns>Instance of the service of type T.</returns>
        public T Get<T>() where T : class
        {
            Type type = typeof(T);
            T service = null;

            if (TryGetService(type, out service)) return service;

            if (TryGetNextInHierarchy(out ServiceLocator container))
                return container.Get<T>();

            throw new ArgumentException($"Could not resolve type '{typeof(T).FullName}'.");
        }

        /// <summary>
        /// Tries to get a service of a specific type. Returns whether or not the process is successful.
        /// </summary>
        /// <param name="service">Service of type T to get.</param>  
        /// <typeparam name="T">Class type of the service to be retrieved.</typeparam>
        /// <returns>True if the service retrieval was successful, false otherwise.</returns>
        public bool TryGet<T>(out T service) where T : class
        {
            Type type = typeof(T);
            service = null;

            if (TryGetService(type, out service))
                return true;

            return TryGetNextInHierarchy(out ServiceLocator container) && container.TryGet(out service);
        }

        bool TryGetService<T>(out T service) where T : class
        {
            return _services.TryGet(out service);
        }

        bool TryGetService<T>(Type type, out T service) where T : class
        {
            return _services.TryGet(out service);
        }

        bool TryGetNextInHierarchy(out ServiceLocator container)
        {
            if (this == global)
            {
                container = null;
                return false;
            }

            container = transform.parent.OrNull()?.GetComponentInParent<ServiceLocator>().OrNull() ?? ForSceneOf(this);
            return container != null;
        }

        void OnDestroy()
        {
            if (this == global)
            {
                global = null;
            }
            else if (sceneContainers.ContainsValue(this))
            {
                sceneContainers.Remove(gameObject.scene);
            }
        }

        // https://docs.unity3d.com/ScriptReference/RuntimeInitializeOnLoadMethodAttribute.html
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            global = null;
            sceneContainers = new Dictionary<Scene, ServiceLocator>();
            tmpSceneGameObjects = new List<GameObject>();
        }



#if UNITY_EDITOR

        [MenuItem("GameObjects/ServiceLocator/Add Global")]
        static void AddGlobal()
        {
            var go = new GameObject(GLOBAL_SERVICE_LOCATOR_NAME, typeof(ServiceLocatorGlobal));
        }

        [MenuItem("GameObject/ServiceLocator/Add Scene")]
        static void AddScene()
        {
            var go = new GameObject(SCENE_SERVICE_LOCATOR_NAME, typeof(ServiceLocatorScene));
        }

#endif
    }
}

public static class GameObjectExtensions
{
    /// <summary>
    /// Returns the object itself if it exists, null otherwise.
    /// </summary>
    /// <remarks>
    /// This method helps differentiate between a null reference and a destroyed Unity object. Unity's "== null" check
    /// can incorrectly return true for destroyed objects, leading to misleading behaviour. The OrNull method use
    /// Unity's "null check", and if the object has been marked for destruction, it ensures an actual null reference is returned,
    /// aiding in correctly chaining operations and preventing NullReferenceExceptions.
    /// </remarks>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="obj">The object being checked.</param>
    /// <returns>The object itself if it exists and not destroyed, null otherwise.</returns>
    public static T OrNull<T>(this T obj) where T : UnityEngine.Object => obj ? obj : null;
}