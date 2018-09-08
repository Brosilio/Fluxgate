using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fluxgate
{
    public class UserProperties
    {
        private Dictionary<string, object> _properties;

        public UserProperties() => _properties = new Dictionary<string, object>();

        public bool ValueExists(string name) => _properties.ContainsKey(name);

        public T GetValue<T>(string name)
        {
            return (T)_properties[name];
        }

        public void SetValue<T>(string name, T value)
        {
            if(ValueExists(name))
                _properties[name] = value;
            else
                _properties.Add(name, value);
        }
    }
}
