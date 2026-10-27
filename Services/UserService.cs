using api_kiralama.Dtos;
using api_kiralama.Models;
using api_kiralama.Repositories;

namespace api_kiralama.Services;

public class UserService 
{
    private UserRepository _userRepository;

    public UserService(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<List<User>> GetAll()
    {
        var list = await _userRepository.GetAll();
        var users = new List<User>();
        foreach (var user in list)
        {
            if (user.IsDeleted == false)
            {
                users.Add(user);
            }
        }
        return users;
    }

    public async Task<User?> GetUser(int id)
    {
        return await _userRepository.GetUser(id);
    }
    public async Task<User?> GetUserByUserName(string userName)
    {
        return await _userRepository.GetUserByUserName(userName);
    }

    public async Task AddUser(UserRequest userRequest)
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userRequest.Password);
        var newUser = new User
        {
            Username = userRequest.Username,
            Password = hashedPassword,
            Role = userRequest.Role,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        await _userRepository.AddUser(newUser);
    }

    public async Task DeleteUser(int id)
    {
        await _userRepository.DeleteUser(id);
    }

    public async Task UpdateUser(int id,UserRequest userRequest)
    {
        var user = await _userRepository.GetUser(id);
        
        user.Username = userRequest.Username;
        user.Password = BCrypt.Net.BCrypt.HashPassword(userRequest.Password);
        user.Role = userRequest.Role;
        user.UpdatedAt = DateTime.Now;
        
        await _userRepository.UpdateUser(user);
    } 
    
    
}