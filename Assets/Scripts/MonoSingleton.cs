using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EskiNottToolKit
{
    /// <summary>
    /// 单例模式，使用只需将某个类继承该类即可
    /// </summary>
    /// <typeparam name="T">单例类型</typeparam>
    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T instance;
        public static T Instance
        {
            get { return instance; }
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = (T)this;
            }
            else
            {
                UnityEngine.Debug.LogError("Get a second instance of this class??" + this.GetType());
            }
        }
    }
}
