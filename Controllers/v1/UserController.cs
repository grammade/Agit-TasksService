using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaskService.Entities;
using TaskService.Persistence;
using TaskService.Dtos;
using WyHash;
using Microsoft.AspNetCore.Identity;
using TaskService.Helpers;

namespace TaskService.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly Context _context;
    private readonly IConfiguration _configuration;

    public UsersController(Context context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserRec>> Register(UserRec user)
    {
        var exists = await _context.Users
            .Where(u => u.Username.Equals(user.Username))
            .FirstOrDefaultAsync();
        if (exists is not null)
            return BadRequest("Username already exists");

        var hasher = WyHash64.Create();
        var hashedPass = hasher.ComputeHash(Encoding.UTF8.GetBytes(user.Password));

        await _context.Users.AddAsync(new User
        {
            Username = user.Username,
            PasswordHash = ByteHelper.ByteArrayToHexViaLookup32(hashedPass)
        });
        await _context.SaveChangesAsync();

        return Ok(user);
    }

    [HttpPost("login")]
    public async Task<ActionResult<string>> login(LoginRec loginRec)
    {
        var key = _configuration.GetValue<string>("Encryption:Key");
        var issuer = _configuration.GetValue<string>("JWT:Issuer");
        var aud = _configuration.GetValue<string>("JWT:Audience");

        var hasher = WyHash64.Create();
        var hashedPass = hasher.ComputeHash(Encoding.UTF8.GetBytes(loginRec.Password));
        var hashedPassString = ByteHelper.ByteArrayToHexViaLookup32(hashedPass);
        var user = await _context.Users
            .Where(u => u.Username.Equals(loginRec.Username) && u.PasswordHash.Equals(hashedPassString))
            .FirstOrDefaultAsync();

        if (user is null)
            return BadRequest("Invalid Cred");

        var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration.GetValue<string>("JWT:Key")!));
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("UserId", user.UserId.ToString())
        };
        var cred = new SigningCredentials(jwtKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: cred,
            issuer: issuer,
            audience: aud
        );

        return Ok(new JwtSecurityTokenHandler().WriteToken(token));
    }
}
