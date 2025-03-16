using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using DnsClient.Protocol;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using NetServer.Models;
using BCrypt.Net;
using NetServer.Repositories;
using NetServer.Abstractions;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Cors;

namespace NetServer.Controllers;

[EnableCors("_myAllowSpecificOrigins")]
[ApiController]
[Route("api/user")]
public class UserController : Controller
{
    // token handling
    private readonly ITokenProvider _tokenProvider;
    private readonly IUserRepository _userRepository;

    // connect to db collection 
    public UserController(IUserRepository userRepository, ITokenProvider tokenProvider)
    {
        _userRepository = userRepository;
        _tokenProvider = tokenProvider;
    }

    //signup
    [HttpPost("signup")]
    public async Task<ActionResult> Signup([FromBody] User user)
    {

        if (string.IsNullOrWhiteSpace(user.Email) ||
            string.IsNullOrWhiteSpace(user.Password) ||
            string.IsNullOrWhiteSpace(user.Username))
        {
            return BadRequest(new { success = false, message = "All fields are required" });
        }

        var existingUser = await _userRepository.GetByEmailOrUsernameAsync(user.Email, user.Username);

        if (existingUser != null)
        {
            return BadRequest(new { success = false, message = "User with this email or username already exists" });
        }

        // Hash password
        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

        await _userRepository.CreateUserAsync(user);

        // Generate JWT Token for immediate login
        string token = _tokenProvider.GenerateToken(user);

        return Ok(new { success = true, message = "User registered successfully!", token = token, userId = user.Id });
    }

    [HttpPost("login")] // explicitly state the loginreques comes from body
    public async Task<ActionResult> Login([FromBody] LoginRequest request)
    {
        // first user to match both store
        var user = await _userRepository.GetByEmailAsync(request.Email);

        // user doesnt exist or the passwords dont hash and match
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
        {
            return Unauthorized("Invalid email or password");
        }

        // gen jwt
        string token = _tokenProvider.GenerateToken(user);

        return Ok(new
        {
            success = true,  
            message = "Login Successful",
            userId = user.Id,
            username = user.Username,
            token = token
        });
    }

    [HttpGet("{userId}/logged")]
    public async Task<ActionResult> GetUserLogged([FromRoute] string userId) { 
        if (string.IsNullOrWhiteSpace(userId)) return BadRequest("User ID required");

        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null) return NotFound("User not found");

        return Ok(new { success = true, loggedItems = user.Logged});
    }

    // get users info -- test endpoint
    [HttpGet("{userId}")]
    public async Task<ActionResult> GetUserInfo([FromRoute] String userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("User ID required");
        }

        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            return NotFound("User not found");
        }

        return Ok(new
        {
            user.Id,
            user.Username,
            user.Email,
            user.Movies,
            user.Music,
            user.Books,
            user.Games
        });
    }
}