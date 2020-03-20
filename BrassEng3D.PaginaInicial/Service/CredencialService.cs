using Autodesk.Forge;
using BrassEng3D.PaginaInicial.Models;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BrassEng3D.PaginaInicial.Service
{
    public class CredencialService
    {

        Credencial _credencial;
        public CredencialService(Credencial credencial)
        {
            _credencial = credencial;
        }

        public static async Task<Credencial> CreateFromCodeAsync(string code, IResponseCookies cookies)
        {
            ThreeLeggedApi oauth = new ThreeLeggedApi();

            dynamic credentialInternal = await oauth.GettokenAsync(
              GetAppSetting("FORGE_CLIENT_ID"), GetAppSetting("FORGE_CLIENT_SECRET"),
              oAuthConstants.AUTHORIZATION_CODE, code, GetAppSetting("FORGE_CALLBACK_URL"));

            dynamic credentialPublic = await oauth.RefreshtokenAsync(
              GetAppSetting("FORGE_CLIENT_ID"), GetAppSetting("FORGE_CLIENT_SECRET"),
              "refresh_token", credentialInternal.refresh_token, new Scope[] { Scope.ViewablesRead });

            Credencial credencial = new Credencial();
            credencial.FORGE_CLIENT_ID = GetAppSetting("FORGE_CLIENT_ID");
            credencial.TokenInternal = credentialInternal.access_token;
            credencial.TokenPublic = credentialPublic.access_token;
            credencial.RefreshToken = credentialPublic.refresh_token;
            credencial.ExpiresAt = DateTime.Now.AddSeconds(credentialInternal.expires_in);

            //cookies.Append(FORGE_COOKIE, JsonConvert.SerializeObject(credentials));

            var client = new MongoClient("mongodb://localhost:27017/auth_brass");
            var database = client.GetDatabase("auth_brass");

            var collection = database.GetCollection<BsonDocument>("credenciais");

            var list = await collection.Find(new BsonDocument("FORGE_CLIENT_ID", GetAppSetting("FORGE_CLIENT_ID"))).ToListAsync();

            if (list.Count > 0)
            {
                collection.DeleteOne(list[0]);
            }

            var ver = GetAppSetting("FORGE_CLIENT_ID");

            var bsonDocument = credencial.ToBsonDocument();
            await collection.InsertOneAsync(bsonDocument);

            return credencial;
        }


        public static void Signout(IResponseCookies cookies)
        {
            var client = new MongoClient("mongodb://localhost:27017/auth_brass");
            var database = client.GetDatabase("auth_brass");

            var collection = database.GetCollection<Credencial>("credenciais");

            collection.DeleteOne(Builders<Credencial>.Filter.Lt("FORGE_CLIENT_ID", GetAppSetting("FORGE_CLIENT_ID")));

            //cookies.Delete(FORGE_COOKIE);
        }


        public static async Task<Credencial> FromSessionAsync(IRequestCookieCollection requestCookie, IResponseCookies responseCookie)
        {
            var client = new MongoClient("mongodb://localhost:27017/auth_brass");
            var database = client.GetDatabase("auth_brass");

            var collection = database.GetCollection<Credencial>("credenciais");
            //if(collection == null)
            //{
            //database.CreateCollection("credentials");
            //collection = database.GetCollection<BsonDocument>("credenciais");
            //}
            //var filter = Builders<Credencial>.Filter.Eq(x => x.FORGE_CLIENT_ID, GetAppSetting("FORGE_CLIENT_ID"));
            //var list = collection.Find(filter).ToList();

            //await collection.Find(new BsonDocument("FORGE_CLIENT_ID", GetAppSetting("FORGE_CLIENT_ID"))).ToListAsync();

            var list = collection.AsQueryable<Credencial>().Where(x => x.FORGE_CLIENT_ID == GetAppSetting("FORGE_CLIENT_ID"));

            if (list.Count() <= 0)
            {
                return null;
            }

            Credencial credencial = list.First();

            CredencialService credencialService = new CredencialService(credencial);

            if (credencial.ExpiresAt < DateTime.Now)
            {
                await credencialService.RefreshAsync();
                await collection.DeleteOneAsync(Builders<Credencial>.Filter.Lt("FORGE_CLIENT_ID", GetAppSetting("FORGE_CLIENT_ID")));
                await collection.InsertOneAsync(credencial);

            }

            return credencial;



            /*
            if (requestCookie == null || !requestCookie.ContainsKey(FORGE_COOKIE)) return null;

            Credentials credentials = JsonConvert.DeserializeObject<Credentials>(requestCookie[FORGE_COOKIE]);
            if (credentials.ExpiresAt < DateTime.Now)
            {
                await credentials.RefreshAsync();
                responseCookie.Delete(FORGE_COOKIE);
                responseCookie.Append(FORGE_COOKIE, JsonConvert.SerializeObject(credentials));
            }

            return credentials;*/
        }


        public async Task RefreshAsync()
        {
            ThreeLeggedApi oauth = new ThreeLeggedApi();

            dynamic credentialInternal = await oauth.RefreshtokenAsync(
              GetAppSetting("FORGE_CLIENT_ID"), GetAppSetting("FORGE_CLIENT_SECRET"),
              "refresh_token", _credencial.RefreshToken, new Scope[] { Scope.DataRead, Scope.DataCreate, Scope.DataWrite, Scope.ViewablesRead });

            dynamic credentialPublic = await oauth.RefreshtokenAsync(
              GetAppSetting("FORGE_CLIENT_ID"), GetAppSetting("FORGE_CLIENT_SECRET"),
              "refresh_token", credentialInternal.refresh_token, new Scope[] { Scope.ViewablesRead });

            _credencial.TokenInternal = credentialInternal.access_token;
            _credencial.TokenPublic = credentialPublic.access_token;
            _credencial.RefreshToken = credentialPublic.refresh_token;
            _credencial.ExpiresAt = DateTime.Now.AddSeconds(credentialInternal.expires_in);
        }

        public static string GetAppSetting(string settingKey)
        {
            return Environment.GetEnvironmentVariable(settingKey);
        }
    }
}
