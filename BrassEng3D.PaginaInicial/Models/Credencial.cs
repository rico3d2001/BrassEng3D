using Autodesk.Forge;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Threading.Tasks;

namespace BrassEng3D.PaginaInicial.Models
{
    
    public class Credencial
    {
    

        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public ObjectId _Id { get; set; }

        [BsonElement("FORGE_CLIENT_ID")]
        public string FORGE_CLIENT_ID { get; set; }

        [BsonElement("TokenInternal")]
        public string TokenInternal { get; set; } 

        [BsonElement("TokenPublic")]
        public string TokenPublic { get; set; }

        [BsonElement("RefreshToken")]
        public string RefreshToken { get; set; }

        [BsonElement("ExpiresAt")]
        public DateTime ExpiresAt { get; set; }

        private async Task RefreshAsync()
        {
            ThreeLeggedApi oauth = new ThreeLeggedApi();

            dynamic credentialInternal = await oauth.RefreshtokenAsync(
              GetAppSetting("FORGE_CLIENT_ID"), GetAppSetting("FORGE_CLIENT_SECRET"),
              "refresh_token", RefreshToken, new Scope[] { Scope.DataRead, Scope.DataCreate, Scope.DataWrite, Scope.ViewablesRead });

            dynamic credentialPublic = await oauth.RefreshtokenAsync(
              GetAppSetting("FORGE_CLIENT_ID"), GetAppSetting("FORGE_CLIENT_SECRET"),
              "refresh_token", credentialInternal.refresh_token, new Scope[] { Scope.ViewablesRead });

            TokenInternal = credentialInternal.access_token;
            TokenPublic = credentialPublic.access_token;
            RefreshToken = credentialPublic.refresh_token;
            ExpiresAt = DateTime.Now.AddSeconds(credentialInternal.expires_in);
        }

        public static string GetAppSetting(string settingKey)
        {
            return Environment.GetEnvironmentVariable(settingKey);
        }


    }
}
