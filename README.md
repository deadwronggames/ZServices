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

You can also use any of the built in services (described in more detail below) in a similar way:
```csharp
ServiceLocator.Get<EventBroadcastService>().Broadcast<MyEventChannel>(sender: this, data: 5f); // alternatively cache the service for repeated / regular use
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


## Placeholder / Pre-Alpha Services

The following services are currently not finalized or only included as stubs:

- AudioService
Will provide centralized playback for SFX, music, and ambient loops.
Designed to handle pooled AudioSources, volume groups, and dynamic ducking.

- InputService
Planned integration layer between Unity’s Input System and in-game systems.
Will expose simplified event-based access to gameplay input, UI input, and rebinding tools.

- Task Services 
Currently limited to a simple MainThreadDispatcher. Async and multithreading helpers will follow-

- TimerService 
Early refactored prototype for timed actions. Not production-ready.

- UpdateCallbackService
Centralized `Update` that calls users. Avoids overhead.
Needs more testing.

All of these services already exists in the game "Immortal Zombiehunter" (more or less), and will be cleanly exported to ZServicces as soon as I find the time.


## Notes
- **Work in progress**, some functionality may change, features will be added.