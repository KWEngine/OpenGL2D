using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OpenGL2D.Core
{

    /// <summary>
    /// World template class
    /// </summary>
    abstract class World
    {
        private int _worldActorId = 0;
        private List<Actor> _actors = new List<Actor>();
        private Dictionary<Type, int> _drawPriority = new Dictionary<Type, int>();

        internal void SortActorList()
        {
            _actors.Sort();
        }

        public int GetDrawPriorityForType(Type t)
        {
            if(_drawPriority.ContainsKey(t))
            {
               
                return _drawPriority[t];
            }
            else
            {
                //Debug.WriteLine("Draw priority not found for Type " + t.ToString());
                return 0;
            }
        }

        public ReadOnlyCollection<Actor> GetCurrentObjects()
        {
            return _actors.AsReadOnly();
        }

        public void AddObject(Actor a)
        {
            if (a != null && !_actors.Contains(a))
            {
                a.SetNumber(_worldActorId);
                _worldActorId = (_worldActorId + 1) % int.MaxValue;
                a.SetWorld(this);
                _actors.Add(a);
            }
        }

        public bool RemoveObject(Actor a)
        {
            a.SetWorld(null);
            return _actors.Remove(a);
        }


        public void SetDrawPriority(params Type[] types)
        {
            List<Type> prioritizedTypes = new List<Type>();

            int value = 0;
            for(int i = types.Length - 1; i >= 0; i--)
            {
                Type t = types[i];
                if (prioritizedTypes.Contains(t))
                    continue;
                _drawPriority[t] = value;
                value++;
            }
            prioritizedTypes.Clear();
        }

        public abstract void Prepare();

    }
}
