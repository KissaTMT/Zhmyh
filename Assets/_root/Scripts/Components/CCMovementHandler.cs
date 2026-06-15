using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Components
{
    public class CCMovementHandler : UnitComponent, IMovementHandler
    {
        private readonly CharacterController _characterController;
        private List<IContributable<Vector3>> _contributables;

        public CCMovementHandler(CharacterController characterController)
        {
            _characterController = characterController;
            _contributables = new List<IContributable<Vector3>>(4);
        }

        public void Add(IContributable<Vector3> contributable)
        {
            _contributables.Add(contributable);
        }
        /// <summary>
        /// Use only via cashed, don't calls in update
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IContributable<Vector3> Get<T>() where T : IContributable<Vector3>
        {
            for(var i=0;i< _contributables.Count;i++)
            {
                if (_contributables[i].GetType() == typeof(T)) return _contributables[i];
            }
            return null;
        }

        public bool Remove(IContributable<Vector3> contributable)
        {
            return _contributables.Remove(contributable);
        }

        public void Tick(float dt)
        {
            var result = Vector3.zero;
            for (var i = 0; i < _contributables.Count; i++)
            {
                result += _contributables[i].Contribute;
            }
            _characterController.Move(result);
        }
    }
}
