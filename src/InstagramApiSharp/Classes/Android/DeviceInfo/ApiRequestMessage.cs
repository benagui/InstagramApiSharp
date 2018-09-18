﻿using System;
using InstagramApiSharp.API;
using InstagramApiSharp.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace InstagramApiSharp.Classes.Android.DeviceInfo
{
    internal class ApiRequestChallengeMessage : ApiRequestMessage
    {
        [JsonProperty("_csrftoken")]
        public string CsrtToken { get; set; }
    }
    public class ApiRequestMessage
    {
        static Random rnd = new Random();
        [JsonProperty("phone_id")]
        public string PhoneId { get; set; }
        [JsonProperty("username")]
        public string Username { get; set; }
        [JsonProperty("guid")]
        public Guid Guid { get; set; }
        [JsonProperty("device_id")]
        public string DeviceId { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
        [JsonProperty("login_attempt_count")]
        public string LoginAttemptCount { get; set; } = "0";
        [JsonProperty("adid")]
        public string AdId { get; set; }
        public static ApiRequestMessage CurrentDevice { get; private set; }
        internal string GetMessageString()
        {
            var json = JsonConvert.SerializeObject(this);
            return json;
        }
        internal string GetChallengeMessageString(string csrfToken)
        {
            var api = new ApiRequestChallengeMessage
            {
                CsrtToken = csrfToken,
                DeviceId = DeviceId,
                Guid = Guid,
                LoginAttemptCount = "1",
                Password = Password,
                PhoneId = PhoneId,
                Username = Username,
                AdId = AdId
            };
            var json = JsonConvert.SerializeObject(api);
            return json;
        }
        internal string GetMessageStringForChallengeVerificationCodeSend(int Choice = 1)
        {
            return JsonConvert.SerializeObject(new { choice = Choice.ToString(), _csrftoken = "ReplaceCSRF", Guid, DeviceId });
        }
        internal string GetChallengeVerificationCodeSend(string verify)
        {
            return JsonConvert.SerializeObject(new { security_code = verify.ToString(), _csrftoken = "ReplaceCSRF", Guid, DeviceId });
        }
        internal string GenerateSignature(string signatureKey, out string deviceid)
        {
            if (string.IsNullOrEmpty(signatureKey))
                signatureKey = InstaApiConstants.IG_SIGNATURE_KEY;
            var res = CryptoHelper.CalculateHash(signatureKey,
                JsonConvert.SerializeObject(this));
            deviceid = DeviceId;
            return res;
        }
        internal string GenerateChallengeSignature(string signatureKey,string csrfToken, out string deviceid)
        {
            if (string.IsNullOrEmpty(signatureKey))
                signatureKey = InstaApiConstants.IG_SIGNATURE_KEY;
            var api = new ApiRequestChallengeMessage
            {
                CsrtToken = csrfToken,
                DeviceId = DeviceId,
                Guid = Guid,
                LoginAttemptCount = "1",
                Password = Password,
                PhoneId = PhoneId,
                Username = Username,
                AdId = AdId
            };
            var res = CryptoHelper.CalculateHash(signatureKey,
                JsonConvert.SerializeObject(api));
            deviceid = DeviceId;
            return res;
        }
        internal bool IsEmpty()
        {
            if (string.IsNullOrEmpty(PhoneId)) return true;
            if (string.IsNullOrEmpty(DeviceId)) return true;
            if (Guid.Empty == Guid) return true;
            return false;
        }

        internal static string GenerateDeviceId()
        {
            return GenerateDeviceIdFromGuid(Guid.NewGuid());
        }

        internal static string GenerateUploadId()
        {
            var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
            var uploadId = (long) timeSpan.TotalSeconds;
            return uploadId.ToString();
        }
        internal static string GenerateRandomUploadId()
        {
            var total = GenerateUploadId();
            var uploadId = total + rnd.Next(11111, 99999).ToString("D5");
            return uploadId;
        }
        public static ApiRequestMessage FromDevice(AndroidDevice device)
        {
            var requestMessage = new ApiRequestMessage
            {
                PhoneId = device.PhoneGuid.ToString(),
                Guid = device.DeviceGuid,
                DeviceId = device.DeviceId
            };
            return requestMessage;
        }

        public static string GenerateDeviceIdFromGuid(Guid guid)
        {
            var hashedGuid = CryptoHelper.CalculateMd5(guid.ToString());
            return $"android-{hashedGuid.Substring(0, 16)}";
        }
    }
}