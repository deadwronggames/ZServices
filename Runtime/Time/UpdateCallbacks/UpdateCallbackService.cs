using System;
using System.Collections.Generic;
using DeadWrongGames.ZServices.Diagnostics;
using UnityEngine;

namespace DeadWrongGames.ZServices.Time
{
    /// <summary>
    /// Any class can implement any of the <see cref="IUpdatable"/>, <see cref="ILateUpdatable"/>, <see cref="IFixedUpdatable"/>
    /// interfaces and then register with this service. Inheriting from <see cref="UpdatedMonoBehaviour"/> automated (de)registration
    /// </summary>
    public class UpdateCallbackService : MonoBehaviour, IService
    {
        // Keep track of users
        private readonly HashSet<IUpdatable> _usersUpdatable = new();
        private readonly HashSet<ILateUpdatable> _usersLateUpdatable = new();
        private readonly HashSet<IFixedUpdatable> _usersFixedUpdatable = new();
        
        // Don't modify lists while they are iterated. Have new users pending instead and then add them safely.
        private readonly HashSet<IUpdatable> _usersUpdatablePending = new();
        private readonly HashSet<ILateUpdatable> _usersLateUpdatablePending = new();
        private readonly HashSet<IFixedUpdatable> _usersFixedUpdatablePending = new();
        
        private void Awake()
        {
            ServiceLocator.Register(this);
        }
        
        // Calling all users, then adding pending users
        private void Update() => CallbackUsers(_usersUpdatable, _usersUpdatablePending, u => u.OnUpdate());
        private void LateUpdate() => CallbackUsers(_usersLateUpdatable, _usersLateUpdatablePending, u => u.OnLateUpdate());
        private void FixedUpdate() => CallbackUsers(_usersFixedUpdatable, _usersFixedUpdatablePending, u => u.OnFixedUpdate());
        private static void CallbackUsers<T>(HashSet<T> users, ICollection<T> pending, Action<T> callback)
        {
            foreach (T user in users)
            {
                try { callback(user); }
                catch (Exception ex) { LogService.Error(BuiltInLogCategories.ZSystems, ex.Message).Log(); }
            }
            
            users.UnionWith(pending);
            pending.Clear();
        }
        
        /// <summary>
        /// Users must implement the interfaces corresponding to the callbacks they want to receive
        /// </summary>
        /// <param name="user"></param>
        public void Register(IBaseUpdatable user)
        {
            if (user is not IUpdatable && user is not ILateUpdatable && user is not IFixedUpdatable)
            {
                LogService.Warning(BuiltInLogCategories.ZSystems, $"{user} does not implement any of the non-base updatable interfaces. Returning.").Log();
                return;
            }
            
            if (user is IUpdatable updatableUser) _usersUpdatablePending.Add(updatableUser);
            if (user is ILateUpdatable lateUpdatableUser) _usersLateUpdatablePending.Add(lateUpdatableUser);
            if (user is IFixedUpdatable fixedUpdatableUser) _usersFixedUpdatablePending.Add(fixedUpdatableUser);
        }
        
        public void Unregister(IBaseUpdatable user)
        {
            if (user is IUpdatable updatableUser) _usersUpdatable.Remove(updatableUser);
            if (user is ILateUpdatable lateUpdatableUser) _usersLateUpdatable.Remove(lateUpdatableUser);
            if (user is IFixedUpdatable fixedUpdatableUser) _usersFixedUpdatable.Remove(fixedUpdatableUser);
        }
    }
}