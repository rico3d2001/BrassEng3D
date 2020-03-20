/////////////////////////////////////////////////////////////////////
// Copyright (c) Autodesk, Inc. All rights reserved
// Written by Forge Partner Development
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC.
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
/////////////////////////////////////////////////////////////////////

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Autodesk.Forge;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Net;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Cors;
using MongoDB.Driver;
using MongoDB.Bson;
using BrassEng3D.PaginaInicial.Models;
using System.Linq;
using BrassEng3D.PaginaInicial.Service;

namespace bim360issues.Controllers
{
    [EnableCors("MyPolicy")]
    public class OAuthController : ControllerBase
    {
        [HttpGet]
        [Route("api/forge/oauth/token")]
        public async Task<AccessToken> GetPublicTokenAsync()
        {

            Credencial credencial = await CredencialService.FromSessionAsync(Request.Cookies, Response.Cookies);

            if (credencial == null)
            {
                base.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return new AccessToken();
            }

            // return the public (viewables:read) access token
            return new AccessToken()
            {
                access_token = credencial.TokenPublic,
                expires_in = (int)credencial.ExpiresAt.Subtract(DateTime.Now).TotalSeconds
            };


            //Credentials credentials = await Credentials.FromSessionAsync(Request.Cookies, Response.Cookies);

            //if (credentials == null)
            //{
            //    base.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            //    return new AccessToken();
            //}

            //// return the public (viewables:read) access token
            //return new AccessToken()
            //{
            //    access_token = credentials.TokenPublic,
            //    expires_in = (int)credentials.ExpiresAt.Subtract(DateTime.Now).TotalSeconds
            //};
        }



        /// <summary>
        /// Response for GetPublicToken
        /// </summary>
        public struct AccessToken
        {
            public string access_token { get; set; }
            public int expires_in { get; set; }
        }

        [HttpGet]
        [Route("api/forge/oauth/signout")]
        public IActionResult Singout()
        {
            // finish the session
            CredencialService.Signout(base.Response.Cookies);

            return Redirect("/");
        }




        [HttpGet]
        [Route("api/forge/oauth/url")]
        public string GetOAuthURL()
        {
            // prepare the sign in URL
            Scope[] scopes = { Scope.DataRead };
            ThreeLeggedApi _threeLeggedApi = new ThreeLeggedApi();
            string oauthUrl = _threeLeggedApi.Authorize(
              CredencialService.GetAppSetting("FORGE_CLIENT_ID"),
              oAuthConstants.CODE,
              CredencialService.GetAppSetting("FORGE_CALLBACK_URL"),
              new Scope[] { Scope.DataRead, Scope.DataCreate, Scope.DataWrite, Scope.ViewablesRead });

            return oauthUrl;
        }

        [HttpGet]
        [Route("api/forge/callback/oauth")] // see Web.Config FORGE_CALLBACK_URL variable
        public async Task<IActionResult> OAuthCallbackAsync(string code)
        {
            // create credentials form the oAuth CODE
            Credencial credencial = await CredencialService.CreateFromCodeAsync(code, Response.Cookies);


            return Redirect("http://localhost:4200");
        }

        //[HttpGet]
        //[Route("api/forge/checkToken360")]
        //public async Task<IActionResult> CheckToken360(string code)
        //{
        //    var client = new MongoClient("mongodb://localhost:27017/auth_brass");
        //    var database = client.GetDatabase("auth_brass");

        //    var collection = database.GetCollection<BsonDocument>("credenciais");


        //    var list = await collection.Find(new BsonDocument("FORGE_CLIENT_ID", Credentials.GetAppSetting("FORGE_CLIENT_ID"))).ToListAsync();

        //    if (list.Count > 0)
        //    {
        //        base.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        //    }

        //    return Redirect("http://localhost:4200");
        //}

        [HttpGet]
        [Route("api/forge/clientid")] // see Web.Config FORGE_CALLBACK_URL variable
        public dynamic GetClientID()
        {
            return new { id = CredencialService.GetAppSetting("FORGE_CLIENT_ID") };
        }
    }

    /// <summary>
    /// Store data in session
    /// </summary>
    //public class Credentials
    //{
        //private const string FORGE_COOKIE = "ForgeApp";

        //private Credentials() { }

        //public string TokenInternal { get; set; }
        //public string TokenPublic { get; set; }
        //public string RefreshToken { get; set; }
        //public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Perform the OAuth authorization via code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        //public static async Task<Credencial> CreateFromCodeAsync(string code, IResponseCookies cookies)
        //{
        //    ThreeLeggedApi oauth = new ThreeLeggedApi();

        //    dynamic credentialInternal = await oauth.GettokenAsync(
        //      GetAppSetting("FORGE_CLIENT_ID"), GetAppSetting("FORGE_CLIENT_SECRET"),
        //      oAuthConstants.AUTHORIZATION_CODE, code, GetAppSetting("FORGE_CALLBACK_URL"));

        //    dynamic credentialPublic = await oauth.RefreshtokenAsync(
        //      GetAppSetting("FORGE_CLIENT_ID"), GetAppSetting("FORGE_CLIENT_SECRET"),
        //      "refresh_token", credentialInternal.refresh_token, new Scope[] { Scope.ViewablesRead });

        //    //Credentials credentials = new Credentials();
        //    //credentials.TokenInternal = credentialInternal.access_token;
        //    //credentials.TokenPublic = credentialPublic.access_token;
        //    //credentials.RefreshToken = credentialPublic.refresh_token;
        //    //credentials.ExpiresAt = DateTime.Now.AddSeconds(credentialInternal.expires_in);
        //    Credencial credencial = new Credencial();
        //    credencial.FORGE_CLIENT_ID = GetAppSetting("FORGE_CLIENT_ID");
        //    credencial.TokenInternal = credentialInternal.access_token;
        //    credencial.TokenPublic = credentialPublic.access_token;
        //    credencial.RefreshToken = credentialPublic.refresh_token;
        //    credencial.ExpiresAt = DateTime.Now.AddSeconds(credentialInternal.expires_in);

        //    //cookies.Append(FORGE_COOKIE, JsonConvert.SerializeObject(credentials));

        //    var client = new MongoClient("mongodb://localhost:27017/auth_brass");
        //    var database = client.GetDatabase("auth_brass");

        //    var collection = database.GetCollection<BsonDocument>("credenciais");
        //    //if(collection == null)
        //    //{
        //    //database.CreateCollection("credentials");
        //    //collection = database.GetCollection<BsonDocument>("credenciais");
        //    //}

        //    var list = await collection.Find(new BsonDocument("FORGE_CLIENT_ID", GetAppSetting("FORGE_CLIENT_ID"))).ToListAsync();

        //    if(list.Count > 0)
        //    {
        //        collection.DeleteOne(list[0]);
        //    }

        //    var ver = GetAppSetting("FORGE_CLIENT_ID");

        //    //Credencial credencial = new Credencial();
        //    //credencial.FORGE_CLIENT_ID = GetAppSetting("FORGE_CLIENT_ID");
        //    //credencial.TokenInternal = credentials.TokenInternal;
        //    //credencial.TokenPublic = credentials.TokenPublic;
        //    //credencial.RefreshToken = credentials.RefreshToken;
        //    //credencial.ExpiresAt = credentials.ExpiresAt;






        //    var bsonDocument = credencial.ToBsonDocument();
        //    await collection.InsertOneAsync(bsonDocument);






        //    return credencial;//credentials;
        //}

        /// <summary>
        /// Restore the credentials from the session object, refresh if needed
        /// </summary>
        /// <returns></returns>
        //public static async Task<Credencial> FromSessionAsync(IRequestCookieCollection requestCookie, IResponseCookies responseCookie)
        //{
        //    var client = new MongoClient("mongodb://localhost:27017/auth_brass");
        //    var database = client.GetDatabase("auth_brass");

        //    var collection = database.GetCollection<Credencial>("credenciais");
        //    //if(collection == null)
        //    //{
        //    //database.CreateCollection("credentials");
        //    //collection = database.GetCollection<BsonDocument>("credenciais");
        //    //}
        //    //var filter = Builders<Credencial>.Filter.Eq(x => x.FORGE_CLIENT_ID, GetAppSetting("FORGE_CLIENT_ID"));
        //    //var list = collection.Find(filter).ToList();

        //    //await collection.Find(new BsonDocument("FORGE_CLIENT_ID", GetAppSetting("FORGE_CLIENT_ID"))).ToListAsync();

        //    var list = collection.AsQueryable<Credencial>().Where(x => x.FORGE_CLIENT_ID == GetAppSetting("FORGE_CLIENT_ID"));

        //    if (list.Count() <= 0)
        //    {
        //        return null;
        //    }

        //    Credencial credencial = list.First();

        //    CredencialService credencialService = new CredencialService(credencial);

        //    if(credencial.ExpiresAt < DateTime.Now)
        //    {
        //        await credencialService.RefreshAsync();
        //        await collection.DeleteOneAsync(Builders<Credencial>.Filter.Lt("FORGE_CLIENT_ID", GetAppSetting("FORGE_CLIENT_ID")));
        //        await collection.InsertOneAsync(credencial);

        //    }

        //    return credencial;



        //    /*
        //    if (requestCookie == null || !requestCookie.ContainsKey(FORGE_COOKIE)) return null;

        //    Credentials credentials = JsonConvert.DeserializeObject<Credentials>(requestCookie[FORGE_COOKIE]);
        //    if (credentials.ExpiresAt < DateTime.Now)
        //    {
        //        await credentials.RefreshAsync();
        //        responseCookie.Delete(FORGE_COOKIE);
        //        responseCookie.Append(FORGE_COOKIE, JsonConvert.SerializeObject(credentials));
        //    }

        //    return credentials;*/
        //}

        //public static void Signout(IResponseCookies cookies)
        //{
        //    cookies.Delete(FORGE_COOKIE);
        //}

        /// <summary>
        /// Refresh the credentials (internal & external)
        /// </summary>
        /// <returns></returns>
        //private async Task RefreshAsync()
        //{
        //    ThreeLeggedApi oauth = new ThreeLeggedApi();

        //    dynamic credentialInternal = await oauth.RefreshtokenAsync(
        //      GetAppSetting("FORGE_CLIENT_ID"), GetAppSetting("FORGE_CLIENT_SECRET"),
        //      "refresh_token", RefreshToken, new Scope[] { Scope.DataRead, Scope.DataCreate, Scope.DataWrite, Scope.ViewablesRead });

        //    dynamic credentialPublic = await oauth.RefreshtokenAsync(
        //      GetAppSetting("FORGE_CLIENT_ID"), GetAppSetting("FORGE_CLIENT_SECRET"),
        //      "refresh_token", credentialInternal.refresh_token, new Scope[] { Scope.ViewablesRead });

        //    TokenInternal = credentialInternal.access_token;
        //    TokenPublic = credentialPublic.access_token;
        //    RefreshToken = credentialPublic.refresh_token;
        //    ExpiresAt = DateTime.Now.AddSeconds(credentialInternal.expires_in);
        //}

        /// <summary>
        /// Reads appsettings from web.config
        /// </summary>
        //public static string GetAppSetting(string settingKey)
        //{
        //    return Environment.GetEnvironmentVariable(settingKey);
        //}
    //}
}

