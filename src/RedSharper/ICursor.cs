using System;
using RedSharper.Contracts;

namespace RedSharper
{
    public interface ICursor
    {
        RedSingleResult Get(string key);

        RedResult Set(string key, string value, TimeSpan? expiry = null);
    }
}