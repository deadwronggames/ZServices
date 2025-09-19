using System;
using System.Collections.Generic;
using DeadWrongGames.ZUtils;
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
        private readonly List<IUpdatable> _usersUpdatable = new();
        private readonly List<ILateUpdatable> _usersLateUpdatable = new();
        private readonly List<IFixedUpdatable> _usersFixedUpdatable = new();
        
        // Don't modify lists while they are iterated. Have new users pending instead and then add them safely.
        private readonly List<IUpdatable> _usersUpdatablePending = new();
        private readonly List<ILateUpdatable> _usersLateUpdatablePending = new();
        private readonly List<IFixedUpdatable> _usersFixedUpdatablePending = new();
        
        private void Awake()
        {
            ServiceLocator.Register(this);
        }
        
        // Calling all users, then adding pending users
        private void Update() => CallbackUsers(_usersUpdatable, _usersUpdatablePending, u => u.OnUpdate());
        private void LateUpdate() => CallbackUsers(_usersLateUpdatable, _usersLateUpdatablePending, u => u.OnLateUpdate());
        private void FixedUpdate() => CallbackUsers(_usersFixedUpdatable, _usersFixedUpdatablePending, u => u.OnFixedUpdate());
        private static void CallbackUsers<T>(List<T> users, ICollection<T> pending, Action<T> callback)
        {
            for (int i = users.Count - 1; i >= 0; i--)
            {
                try {
                    callback(users[i]);
                }
                catch (Exception ex) {
                    ex.Log(level: ZMethodsDebug.LogLevel.Error);
                }
            }

            users.AddRange(pending);
            pending.Clear();
        }
        
        /// <summary>
        /// Users must implement the interfaces corresponding to the callbacks they want to receive
        /// </summary>
        /// <param name="user"></param>
        public void Register(IBaseUpdatable user)
        {
            if (user is IUpdatable updatable) _usersUpdatablePending.Add(updatable);
            if (user is ILateUpdatable lateUpdatable) _usersLateUpdatablePending.Add(lateUpdatable);
            if (user is IFixedUpdatable fixedUpdatable) _usersFixedUpdatablePending.Add(fixedUpdatable);
        }
        
        public void Unregister(IBaseUpdatable user)
        {
            if (user is IUpdatable) RemoveUser(user, _usersUpdatable);
            if (user is ILateUpdatable) RemoveUser(user, _usersLateUpdatable);
            if (user is IFixedUpdatable) RemoveUser(user, _usersFixedUpdatable);
        }
        
        private static void RemoveUser<T>(IBaseUpdatable user, IList<T> users) where T : class
        {
            for (int i = users.Count - 1; i >= 0; i--)
                if (users[i] == (T)user) users.RemoveAt(i);
        }
    }
}
