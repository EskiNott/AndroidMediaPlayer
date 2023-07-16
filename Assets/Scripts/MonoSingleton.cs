using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EskiNottToolKit
{
    /// <summary>
    /// ����ģʽ��ʹ��ֻ�轫ĳ����̳и��༴��
    /// </summary>
    /// <typeparam name="T">��������</typeparam>
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
