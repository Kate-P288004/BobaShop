// -----------------------------------------------------------------------------
// File: Helpers/SessionExtensions.cs
// Project: BobaShop.Web 
// Student: Kate Odabas (P288004)
// Date: November 2025
// Assessment: AT2 – MVC & NoSQL Project (ICTPRG554 / ICTPRG556)
// Description:
//   Provides extension methods for saving and retrieving complex objects in
//   ASP.NET Core session state. By default, session only supports primitive
//   data types. 
// -----------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace BobaShop.Web.Helpers
{
    // -------------------------------------------------------------------------
    // Class: SessionExtensions
    // Purpose:
    //   Adds helper methods to the ISession interface, allowing any object type
    //   to be saved or retrieved from session storage using JSON serialization.
    //   Used primarily by the CartService to persist user cart data between
    //   requests during an authenticated session.
    // Mapping: ICTPRG556 PE2.3 – MVC service layer and state management
    // -------------------------------------------------------------------------
    public static class SessionExtensions
    {
        // ---------------------------------------------------------------------
        // Method: SetObject<T>
        // Purpose:
        //   Serializes an object of any type (T) into a JSON string and stores it
        //   in session using the provided key. If the same key already exists,
        //   it overwrites the previous value.
        // Parameters:
        //   session – current user session 
        //   key     – unique session key 
        //   value   – the object to serialize and store
        // ---------------------------------------------------------------------
        public static void SetObject<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        // ---------------------------------------------------------------------
        // Method: GetObject<T>
        // Purpose:
        //   Retrieves and deserializes a JSON-encoded object from session by key.
        // Parameters:
        //   session – current user session
        //   key     – the key used to retrieve the stored object
        // ---------------------------------------------------------------------
        public static T? GetObject<T>(this ISession session, string key)
        {
            var str = session.GetString(key);
            return str is null ? default : JsonSerializer.Deserialize<T>(str);
        }
    }
}
