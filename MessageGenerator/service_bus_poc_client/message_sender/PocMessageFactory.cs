using System;
using System.Collections.Generic;
using System.Text;

namespace message_sender
{
    /// <summary>
    /// Message generator can be modified as needed to create messages and formats required for a more relaistic POC.
    /// </summary>
    public static class PocMessageFactory
    {
        private static StringBuilder sb = new StringBuilder();
        private static string GetUUID()
        {
            return Guid.NewGuid().ToString();
        }

        private static string GetTimeStamp()
        {
            // formats as ISO UTC 2021-01-13T09:53:22.430489Z
            return DateTime.UtcNow.ToString("o");
        }

        public static string GetSampleMessage()
        {
            sb.Clear();
            sb.Append("{");
            sb.Append("\"uuid\": \"");
            sb.Append(GetUUID());
            sb.Append("\",");
            sb.Append("\"timestampCreated\": \"");
            sb.Append(GetTimeStamp()); 
            sb.Append("\",");
            sb.Append("\"eventCode\": \"service-1:mdm:event:siteCreated:1\",");
            sb.Append("\"referenceObjectType\": \"myApp-1:mdm:types:site:1\",");
            sb.Append("\"referenceObjectId\": \"c67e67ab-cf3f-4c70-a9bf-fff61c9a1092\",");
            sb.Append("\"attributes\": []");
            sb.Append("}");

            return sb.ToString();
        }
    }
}
