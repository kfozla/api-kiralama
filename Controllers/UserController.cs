using Microsoft.AspNetCore.Mvc;
using api_kiralama.Data;
using api_kiralama.Dtos;
using api_kiralama.Models;
using api_kiralama.Services;
using Microsoft.AspNetCore.Authorization;

namespace api_depo.Controllers;

[ApiController]
[Route("api/Users")]
[Authorize]
public class UserController: ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        try
        {
            var users = await _userService.GetAll();
            return Ok(users);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [HttpGet("{id}")]
    public async Task<ActionResult> GetUser(int id)
    {
        try
        {
            var user = await _userService.GetUser(id);
            if (user == null){ return NotFound();}
            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> AddUser([FromBody] UserRequest userRequest)
    {
        try
        {
            await _userService.AddUser(userRequest);
            return Ok("User added");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UpdateUser(int id,[FromBody] UserRequest userRequest)
    {
        try
        {
            var user = await _userService.GetUser(id);
            if (user == null){ return NotFound();}
            await _userService.UpdateUser(id,userRequest);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        try
        {
            var  user = await _userService.GetUser(id);
            if (user == null){ return NotFound();}
            await _userService.DeleteUser(id);
            return Ok();
        }
        catch(Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}