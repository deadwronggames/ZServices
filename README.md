<p align="center">
  <img src="https://raw.githubusercontent.com/deadwronggames/ZSharedAssets/main/Banner_Zombie.jpg" alt="ZCommon Banner" style="width: 100%; max-width: 1200px; height: auto;">
</p>

# ZServices Package

ZServices provides a lightweight, centralized **Service Locator system** and a collection of core game services.  
It is designed for modular, dependency-free systems that can be easily accessed across the entire project.

## Installation
- Install via Unity Package Manager using the Git URL: https://github.com/deadwronggames/ZServices
- **IMPORTANT**: copy the prefab `Runtime/PF_PersistentGO` to any of your project's `Resources` folders. 
- Include in your code (when needed) via the namespace: 
```csharp 
using DeadWrongGames.ZServices;
```

## Overview

The package includes:
- **GameBootstrapper**: auto-instantiates a persistent service prefab before any scene loads.
- **ServiceLocator**: a static, generic service management system.
- **Event Channel System**: an event broadcasting and listening system built on ScriptableObjects.
- **Pooling Service**: type-based object pooling system for efficient reuse of components and prefabs
- **Update Callback Service**: centralized system for managing safe and efficient update loops
- **Some Pre-Alpha Services**: will be added / refined soon.
- **Editor Tools**: menu actions for creating ScriptableObject-based service assets.

Any other custom service can be added, registered and accessed via the `ServiceLocator`. No change of any package code required. 

## Game Bootstrapper

GameBootstrapper ensures that the prefab named PF_PersistentGO is loaded from the Resources folder and marked as DontDestroyOnLoad.
All service components (e.g. EventBroadcastService, AudioService) should live on this prefab.

## Service Locator

The `ServiceLocator` is a static class providing global access to services implementing the `IService` interface that have registered themself.

### Key Features
- Register and retrieve any service type via generic methods.
- Safe type casting through `ZMethods.TryCast`.
- Editor auto-reset via `SubsystemRegistration` (prevents stale references on domain reload).
- Also provides a `DummyMB` when a `MonoBehaviour` context is needed (e.g. to run coroutines).

### Examples

Add any service you like. Have your custom services implement the IService interface, add them as a Monobehaviour to the persistant prefab and have them register themselves in their `Awake()` method:
```csharp
public class MyService : MonoBehaviour, IService
{
    private void Awake()
    {
        ServiceLocator.Register(this);
    }

    public void MyServiceMethod()
    {
        Debug.Log("MyServiceMethod was called");
    }
}
```

You can then use your services anywhere in your code like this:
```csharp
// If you are not sure if the service exists and are ok with silent fails:
if (ServiceLocator.TryGet(out MyService myService))
    myService.MyServiceMethod();

// Or usually better:    
ServiceLocator.Get<MyService>().MyServiceMethod();
```

You can also get any of the built in services (described in more detail below) in a similar way:
```csharp
EventBroadcastService eventBroadcastService = ServiceLocator.Get<EventBroadcastService>();
```


## Event Channel System

A flexible, decoupled event system using ScriptableObjects. It allows components to communicate without direct references.

### Architecture

- EventBroadcastService: discovers all event channels that were created and broadcasts events.
- EventChannelSO: instances define a single event channel. They keep track of listeners and handle invocation logic.
- EventListener: UnityEvent-based listener for a specific channel.
- EventListenerContainer: component that can be added to GameObjects and registers multiple listeners on enable/disable.
- Broadcaster and BroadcastInformation: optional structs to setup event triggers via the inspector.
- Editor Debug Inspector: provides buttons for manually invoking events in the editor and find listeners.

### Example Workflow
1. Create one file anywhere you like and add a line everytime you would like to create a new channel, e.g.
```csharp
public class MyEventChannel : EventBroadcastService.ChannelMarker { }
```
2. Copy the channel name. Use the editor menu `Create → EventChannelSO`, then rename the created asset to match the marker class name (e.g. MyEventChannel).
3. Now you can use the channel. Attach an EventListenerContainer to any GameObject and assign one or more EventListeners referencing the channel SO asset.
4. Broadcast Events. You can choose to add the sender as well as any data to the broadcast (both are optional). You can broadcast in different ways, either:
- directly from a reference to a channel SO asset
```csharp
myEventChannel.Invoke(sender: this, data: 5f);
```
- via the `ServiceLocator`:
```csharp
ServiceLocator.Get<EventBroadcastService>().Broadcast<MyEventChannel>(sender: this, data: 5f); // alternatively cache the service for repeated / regular use
```
- using the Broadcaster struct to set up the logic (channel, sender, data) on any component (e.g. a ZModularUI button) in the inspector. 
- directly via the channel SO inspector (one time, for debugging)

### Debugging
- Channels can print their listeners with PrintListeners().
- The custom editor allows manual event invocation with test data types.
- You can check the `Verbose` box on any channel SO instances to get helpful log messages at runtime
- Broadcasts from code can also be found by searching for broadcast commands in the git repo of your project


## Pooling Service

The PoolingService provides a centralized, type-based object pooling system built around Unity’s generic ObjectPool API. It helps minimize instantiation overhead by reusing objects such as AudioSources, ParticleSystems, projectiles, or other temporary GameObjects across the project.

### Architecture
Pools are defined via ScriptableObjects derived from `BasePoolDefinitionSO`. Each pool definition specifies a prefab, an optional maximum pool size, and a factory function that determines which component type the pool handles. Each pool type is identified by the component type it manages. If multiple prefab variants share the same component type, configuration and initialization should be handled after retrieval (see `ActionOnGet()` below). At runtime, the `PoolingService` automatically instantiates all defined pools and registers itself in the `ServiceLocator`.

The base class `BasePoolDefinitionSO` handles all pooling logic and provides virtual methods for customizing behavior:

- `CreateFunc()`: defines how new instances are created

- `ActionOnGet()`: called when an object is retrieved from the pool

- `ActionOnRelease()`: called when an object is returned

- `ActionOnDestroy()`: called when the pool is cleared

### Usage Example
To retrieve a pooled object, simply call:
```csharp
PoolingService.Poolable<MyComponent> pooled = ServiceLocator.Get<PoolingService>().Get<MyComponent>();
MyComponent instance = pooled.Component;
```
When done, release the object back to its pool:
```csharp
pooled.Release();
```
This safely deactivates the object and returns it for reuse. Pools automatically clear when the active scene changes.


### Adding custom pools
An example implementation, ExamplePoolDefinitionAudioSourceSO, demonstrates pooling for AudioSource components. It overrides ActionOnRelease to stop playback before returning the object to the pool.

Users can easily add their own pool definitions for any component type by creating new ScriptableObject classes derived from BasePoolDefinitionSO and assigning prefabs.

### Status
Stable core functionality. Additional preconfigured pool types (for particles, projectiles, etc.) will be added later.


## Update Callback Service

The `UpdateCallbackService` provides a centralized and safe update loop for MonoBehaviour as well as non-MonoBehaviour classes or systems that need regular Update, LateUpdate, or FixedUpdate calls.
Instead of scattering per-object updates across the scene, all update callbacks are managed and executed here, which also reduces overhead of lots of individual update calls.

### Usage

Any class can implement one or more of:
- IUpdatable → for Update()
- ILateUpdatable → for LateUpdate()
- IFixedUpdatable → for FixedUpdate()

Classes register themselves with the service using:

- Directly:
```csharp
// e.g. in Start() method
ServiceLocator.Get<UpdateCallbackService>().Register(this);

// e.g. in OnDestroy() method
if(ServiceLocator.TryGet(out UpdateCallbackService service)) service.Unregister(this);
```
- Or automatically by inheriting from `UpdatedMonoBehaviour`, which handles registration and deregistration in OnEnable() / OnDisable().

A full example of how to use the service could look like this:
```csharp
public class Mover : UpdatedMonoBehaviour, IUpdatable
{
    public void OnUpdate()
    {
        transform.position += Vector3.forward * Time.deltaTime;
    }
}

```

The service safely queues new registrations until after the current update iteration finishes, ensuring no collection modification errors occur.

## Placeholder / Pre-Alpha Services

The following services are currently not finalized or only included as stubs:

- **AudioService<br>**
Will provide centralized playback for SFX, music, and ambient loops.
Designed to handle pooled AudioSources, volume groups, and dynamic ducking.

- **InputService<br>**
Planned integration layer between Unity’s Input System and in-game systems.
Will expose simplified event-based access to gameplay input, UI input, and rebinding tools.

- **Task Services<br>**
Currently limited to a simple MainThreadDispatcher. Async and multithreading helpers will follow.

- **TimerService<br>**
Early refactored prototype for timed actions. Not production-ready.

All of these services already exists in the game "Immortal Zombiehunter" (more or less), and will be cleanly exported to ZServicces as soon as I find the time.


## Notes
- **Work in progress**, some functionality may change, features will be added.