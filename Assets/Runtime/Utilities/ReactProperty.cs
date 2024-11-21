using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Gemity.Common
{
    public class ReactPropertyConverter<T> : JsonConverter<ReactProperty<T>>
    {
        public override ReactProperty<T> ReadJson(JsonReader reader, Type objectType, ReactProperty<T> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            T value = serializer.Deserialize<T>(reader);
            return new ReactProperty<T> { Value = value };
        }

        public override void WriteJson(JsonWriter writer, ReactProperty<T> value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.Value);
        }
    }

    public class ReactProperty<T>
    {
        private T _value;
        private event Action<T> _callbacks;

        public T Value
        {
            get => _value;
            set
            {
                if (Equals(value, _value))
                    return;

                _value = value;
                _callbacks?.Invoke(value);
            }
        }

        public IDisposable Subscription(Action<T> callback)
        {
            _callbacks += callback;
            return new CallbackDisposable(() => _callbacks -= callback);
        }
    }
}
