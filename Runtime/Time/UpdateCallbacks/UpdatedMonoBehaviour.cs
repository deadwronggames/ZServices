using UnityEngine;

namespace DeadWrongGames.ZServices.Time
{
    /// <summary>
    /// Inheritors automatically register and deregister
    /// </summary>
    public class UpdatedMonoBehaviour : MonoBehaviour, IBaseUpdatable
    {
        protected virtual void OnEnable()
        {
            ServiceLocator.Get<UpdateCallbackService>().Register(this);
        }
        
        protected virtual void OnDisable()
        {
            if(ServiceLocator.TryGet(out UpdateCallbackService service)) service.Unregister(this);
        }
    }
}