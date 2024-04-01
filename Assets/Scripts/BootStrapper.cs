using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UnityServiceLocator
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ServiceLocator))]
    public abstract class BootStrapper : MonoBehaviour
    {
        private ServiceLocator _container;
        private bool _hasBeenBootStrapped;
        internal ServiceLocator Container => GetServiceLocator();

        private void Awake()
        {
            BootstrapOnDemand();
        }
        public void BootstrapOnDemand()
        {
            if (_hasBeenBootStrapped) return;

            _hasBeenBootStrapped = true;
            BootStrap();
        }
        protected abstract void BootStrap();
        private ServiceLocator GetServiceLocator()
        {
            if (_container == null)
            {
                _container = GetComponent<ServiceLocator>();
            }

            return _container;
        }
    }


    [AddComponentMenu("ServiceLocator/ServiceLocator Global")]
    public class ServiceLocatorGlobal : BootStrapper
    {
        [SerializeField] private bool _dontDestroyOnLoad = true;

        protected override void BootStrap()
        {
            Container.ConfigureAsGlobal(_dontDestroyOnLoad);
        }
    }
    [AddComponentMenu("ServiceLocator Scene")]
    public class ServiceLocatorScene : BootStrapper
    {
        protected override void BootStrap()
        {
            Container.ConfigureForScene();
        }
    }
}
