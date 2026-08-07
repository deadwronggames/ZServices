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
- **Logging System**: centralized logging with categories, configurable sinks, log levels, and automatic exception handling.
- **Timer System**: self-updating, reusable timers (countdown, stopwatch, ticker) 
- **Some Pre-Alpha Services**: will be added / refined soon.
- **Editor Tools**: menu actions for creating ScriptableObject-based service assets.

Any other custom service can be added, registered and accessed via the `ServiceLocator`. No change of any package code required. 

## Game Bootstrapper

GameBootstrapper handles initialization and ensures that the prefab named PF_PersistentGO is loaded from the Resources folder and marked as DontDestroyOnLoad.
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


## Timer System

The Timer system provides reusable, self-updating timers that run as MonoBehaviour components and automatically update via the UpdateCallbackService. They come in several variants:
- TimerCountdown: counts down from a specified time to zero.
- TimerStopwatch: counts up from zero, useful for measuring durations.
- TimerTicker: triggers an action every X seconds.

All timers are designed to be created via their static Create methods, which automatically add a Timer Component to a GameObject, and set initial parameters. All timers can be paused, resumed, stopped, or reset via their public methods. Timers register themselves with UpdateCallbackService when running, and unregister automatically when stopped or destroyed.

### Usage Example

Another MonoBehaviour can create and use a timer like this:
```csharp
public class MyMonoBehaviour : MonoBehaviour
{
    private TimerCountdown _timerCountdown;

    private void Start()
    {
        // Create a countdown timer of 5 seconds, with start/stop callbacks
        _timerCountdown = TimerCountdown.Create(
            userGO: this,
            initialTime: 5f,
            onStart: () => Debug.Log("Timer started"),
            onStop: () => Debug.Log("Timer finished")
        );

        _timerCountdown.StartTimer();
    }

    private void Update()
    {
        if (_timerCountdown.IsRunning)
        {
            Debug.Log($"Time remaining: {_timerCountdown.CurrentTime}");
        }
    }
}
```

### Status
Confident that it works as intended but the system in its current form has not really been tested / used yet.


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

## Logging System

The `LogService` provides a centralized logging system with configurable log levels, categories, and output sinks.

Unlike most services in ZServices, the logging system initializes automatically. No component needs to be added to `PF_PersistentGO`, and no manual registration is required.

### Features

- Multiple log levels:
  - Trace
  - Debug
  - Info
  - Warning
  - Error
  - Fatal
- Custom log categories
- Per-category minimum log level overrides
- Per-category sink overrides
- Multiple simultaneous log sinks
- Automatic capture of unhandled Unity exceptions
- `LogOnce()` support to prevent repeated messages from spamming the output

### Basic Usage

The simplest usage requires no configuration:

```csharp
LogService.Info("Game started").Log();
```
Logs without an explicitly provided category use `BuiltInLogCategories.General`.
Categories can also be specified: 
```csharp
LogService.Info(LogCategories.Economy, "Player bought an item").Log();
```
`LogOnce()` can be used for messages that should only appear once to prevent repeated warnings inside frequently executed code paths:
```csharp
LogService.Warning("Missing save data").LogOnce();
```

### Creating Log Categories
Custom categories can be defined by creating a `LogCategories.cs` file anywhere in the project:
```csharp
public static class LogCategories
{
    public static readonly LogCategory Economy = new("Economy");
    public static readonly LogCategory Combat = new("Combat");
    public static readonly LogCategory AI = new("AI");
}
```
Categories are shown in the log message and are also used as configuration keys that allow different logging rules for different systems.

### Configuration
By default, the logger uses:
- Minimum log level: Info
- Sink: `UnityConsoleSink`

Custom configuration that defines overrides per category, can be provided by implementing exactly one `ILoggerConfigurationProvider`:
```csharp
[LoggerConfigurationProvider]
public sealed class GameLoggerConfigurationProvider : ILoggerConfigurationProvider
{
    public LoggerConfiguration GetConfiguration()
    {
        return new LoggerConfiguration(
            minLogLevelOverridesByCategory: new Dictionary<LogCategory, LogLevel>
            {
                [LogCategories.Economy] = LogLevel.Debug,
            },

            sinkOverridesByCategory: new Dictionary<LogCategory, IReadOnlyList<ILogSink>>
            {
                [LogCategories.Economy] = new List<ILogSink> { new UnityConsoleSink(), new FileSink() }.AsReadOnly(),
            });
    }
}
```
Currently only `UnityConsoleSink` is implemented, `FileSink` is a stub. More sinks might get added at a later point. 

### Unhandled Exceptions
Unhandled Unity exceptions are automatically routed through the logging system with `LogLevel.Fatal` using `BuiltInLogCategories.UnhandledException`.


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

All of these services already exist in the game "Immortal Zombiehunter" (more or less), and will be cleanly exported to ZServices as soon as I find the time.


## Notes
- **Work in progress**, some functionality may change, features will be added.