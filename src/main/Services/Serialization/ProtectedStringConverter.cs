﻿using Newtonsoft.Json;
using PKISharp.WACS.Extensions;
using System;

namespace PKISharp.WACS.Services.Serialization
{

    /// <summary>
    /// forces a re-calculation of the protected data according to current machine setting in EncryptConfig when
    /// writing the json for renewals and options for plugins
    /// </summary>
    public class ProtectedStringConverter : JsonConverter<string>
    {
        public override void WriteJson(JsonWriter writer, string protectedStr, JsonSerializer serializer)
        {
            if (!protectedStr.StartsWith(DataProtectionExtensions.ErrorPrefix))
            {
                writer.WriteValue(protectedStr.Protect());
            }
            else
            {
                //couldn't unprotect string; keeping old value
                writer.WriteValue(protectedStr.Substring(DataProtectionExtensions.ErrorPrefix.Length));
                writer.WriteComment("This protected string cannot be decrypted on current machine. See instructions about migrating to new machine.");
            }
        }

        public override string ReadJson(JsonReader reader, Type objectType, string existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            //allows a user to manually edit the renewal file and enter a password in clear text
            string s = (string)reader.Value;
            if (s.StartsWith(DataProtectionExtensions.ClearPrefix))
            {
                return s.Substring(DataProtectionExtensions.ClearPrefix.Length);
            }
            try
            {
                s = s.Unprotect();
            }
            catch
            {
                //keep the saved value, but with indicator that it is not valid
                s = DataProtectionExtensions.ErrorPrefix + s;
            }
            return s;
        }
    }
}