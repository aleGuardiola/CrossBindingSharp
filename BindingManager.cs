using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace CrossBindingSharp
{
    public class BindingManager
    {
        INotifyPropertyChanged notifyObject;
        Dictionary<string, List<Action<object>>> get = new Dictionary<string, List<Action<object>>>();
        Dictionary<string, KeyValuePair<string, Func<object>>> set = new Dictionary<string, KeyValuePair<string, Func<object>>>();

        public BindingManager(INotifyPropertyChanged notifyObject)
        {
            this.notifyObject = notifyObject ?? throw new ArgumentNullException(nameof(notifyObject));
            notifyObject.PropertyChanged += onPropertyChanged;
        }

        private void onPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                var actions = get[e.PropertyName];
                var value = getPropValue(notifyObject, e.PropertyName);

                foreach (var action in actions)
                    action(value);
            }
            catch { }
        }

        public void TwoWay<T>(string name, string nameBinding, Action<T> get, Func<T> set)
        {
            try
            {
                Func<object> f = () => set();
                Action<object> a = (value) => get((T)value);
                if (!this.get.ContainsKey(name))
                    this.get[name] = new List<Action<object>>();

                this.get[name].Add(a);
                this.set[nameBinding] = new KeyValuePair<string, Func<object>>(name, f);


                a(getPropValue(notifyObject, name));
            }
            catch { }
        }

        public void OneWay<T>(string name, Action<T> get)
        {
            try
            {
                Action<object> a = (value) => get((T)value);

                if (!this.get.ContainsKey(name))
                    this.get[name] = new List<Action<object>>();

                this.get[name].Add(a);



                a(getPropValue(notifyObject, name));
            }
            catch { }
        }

        public void Notify(string nameBinding)
        {
            try
            {
                var value = set[nameBinding];
                var returnedValue = value.Value();
                setPropValue(notifyObject, returnedValue, value.Key);
            }
            catch { }
        }

        static object getPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

        static void setPropValue(object src, object value, string name)
        {
            PropertyInfo prop = src.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (null != prop && prop.CanWrite)
            {
                prop.SetValue(src, value, null);
            }
            else
            {
                throw new Exception("Property" + name + "doesn't exist in " + nameof(src));
            }
        }

    }
}
