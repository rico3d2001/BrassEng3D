﻿/////////////////////////////////////////////////////////////////////
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

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Autodesk.Forge;
using Newtonsoft.Json.Linq;
using MongoDB.Driver;
using MongoDB.Bson;
using BrassEng3D.PaginaInicial.Models;
using System.Linq;
using Microsoft.AspNetCore.Cors;
using BrassEng3D.PaginaInicial.Service;

namespace bim360issues.Controllers
{
    [EnableCors("MyPolicy")]
    public class UserController : ControllerBase
    {
        [HttpGet]
        [Route("api/forge/user/profile")]
        public async Task<JObject> GetUserProfileAsync()
        {
            Credencial credencial = await CredencialService.FromSessionAsync(Request.Cookies, Response.Cookies);
            if (credencial == null)
            {
                return null;
            }


            // the API SDK
            UserProfileApi userApi = new UserProfileApi();
            userApi.Configuration.AccessToken = credencial.TokenInternal;


            // get the user profile
            dynamic userProfile = await userApi.GetUserProfileAsync();

            // prepare a response with name & picture
            dynamic response = new JObject();
            response.name = string.Format("{0} {1}", userProfile.firstName, userProfile.lastName);
            response.picture = userProfile.profileImages.sizeX40;
            response.token = credencial.TokenInternal;
            return response;
        }
    }
}