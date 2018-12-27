using Newtonsoft.Json.Linq;

namespace MixItUp.Base.Model
{
    public abstract class JSONObjectBase<T>
    {
        public T Copy()
        {
            JObject jobj = JObject.FromObject(this);
            return jobj.ToObject<T>();
        }
    }
}
