using Newtonsoft.Json;

namespace SlqStudio.Session;

public static class SessionExtensions
{
    // Сохранение объекта в сессию
    public static void SetObjectAsJson(this ISession session, string key, object value)
    {
        session.SetString(key, JsonConvert.SerializeObject(value));
    }

    // Получение объекта из сессии
    public static T? GetObjectFromJson<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonConvert.DeserializeObject<T>(value);
    }
}
