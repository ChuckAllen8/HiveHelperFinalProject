﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HiveHelper.Models;
using HiveHelper.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HiveHelper.Controllers
{  
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IDataAccessor data;

        public UsersController(IDataAccessor dataAccessor)
        {
            data = dataAccessor;
        }
        private string ParsePassword(string password)
        {
            var hasher = SHA256.Create();
            byte[] hashed = hasher.ComputeHash(Encoding.UTF8.GetBytes(password));

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < hashed.Length; i++ )
            {
                builder.Append(hashed[i].ToString("x2"));
            }
            return builder.ToString();
        }
        [HttpGet("{username}/{password}")]
        public Object Login(string username, string password)
        {
            User found = data.GetUser(username);

            string result;

            if(found == null)
            {
                result = "fail";
            }
            else if (found.password == null || found.password == "")
            {
                result = "new";
            }
            else if (found.password == ParsePassword(password))
            {               
                result = "success";
                found.password = null;
            }
            else
            {
                result = "fail";
                found = null;
            }
            return new { result, user = found };
        }
        [HttpPost]
        public Object AddUser([FromForm]User user)
        {
            bool result = data.AddUser(user);
            return new { result };
        }
        [HttpPut("{username}/{password?}")]
        public Object UpdatePassword([FromForm]User user, string username, string password)
        {
            dynamic response = Login(username, password);
            bool result;
            if(response.result == "new" || response.result == "success")
            {
                user.password = ParsePassword(user.password);
                result = data.UpdateUser(user);
            }
            else
            {
                result = false;
            }
            return new { result };
        }
    }
}
