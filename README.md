# Injector and RuntimeInjector

The Injector and RuntimeInjector scripts provide a simple dependency injection framework for Unity projects. The Injector script facilitates dependency injection, while the RuntimeInjector script allows for runtime injection of dependencies into MonoBehaviours.

## Injector

The Injector script is responsible for managing dependency injection within your Unity project. It allows you to define dependencies and inject them into MonoBehaviours at runtime. Key features of the Injector script include:

- **Injection Attributes**: Use attributes like `InjectAttribute` and `ProvideAttribute` to mark fields, methods, or properties for dependency injection.
- **Dependency Providers**: Implement the `IDependencyProvider` interface to register and provide dependencies.
- **Validation**: Validate dependencies to ensure all required dependencies are provided and injected correctly.
- **Clear Dependencies**: Clear all injected dependencies when needed.

## RuntimeInjector

The RuntimeInjector script is designed to be attached to a GameObject in your scene. It automatically injects dependencies into all MonoBehaviours attached to that GameObject and its children. Key features of the RuntimeInjector script include:

- **Automatic Injection**: Inject dependencies into all MonoBehaviours attached to the GameObject and its children during the `Awake` phase.
- **Manual Injection**: Use methods like `InjectDependencies` to manually inject dependencies into specific MonoBehaviours at runtime.
- **Fetching Dependencies**: Fetch dependencies from the Injector script attached to the scene.

## Usage

1. **Setup**: Attach the Injector script to a GameObject in your scene. Define dependencies using injection attributes and register providers as needed.
2. **Runtime Injection**: Attach the RuntimeInjector script to a GameObject in your scene where you want runtime dependency injection to occur.
3. **Automatic Injection**: Dependencies will be automatically injected into all MonoBehaviours attached to the GameObject and its children during the `Awake` phase.
4. **Manual Injection**: Use methods provided by the RuntimeInjector script to manually inject dependencies into specific MonoBehaviours at runtime.

## Example

```ruby

    public class Provider : MonoBehaviour, IDependencyProvider
    {
        [Provide]
        public ServiceA ProvideServiceA()
        {
            return new ServiceA();
        }

        [Provide]
        public ServiceB ProvideServiceB()
        {
            return new ServiceB();
        }

        [Provide]
        public FactoryA ProvideFactoryA()
        {
            return new FactoryA();
        }
    }



    public class MyComponent : MonoBehaviour
    {
        [Inject]
        private SomeDependencyClass myDependency;

        // This would be called after the injection is complete
        void Start()
        {
            myDependency.DoSomething();
        }
    }
```