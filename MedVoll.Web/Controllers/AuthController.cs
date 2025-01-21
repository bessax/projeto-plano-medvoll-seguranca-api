using FluentValidation;
using MedVoll.Web.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MedVoll.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController:ControllerBase
{
    private readonly UserManager<IdentityUser> userManager;
    private readonly SignInManager<IdentityUser> signInManager;
    private readonly IValidator<UsuarioDto> validator;

    public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IValidator<UsuarioDto> validator)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.validator = validator;
    }

    //Endpoints
    [HttpPost("registrar-usuario")]
    public async Task<IActionResult> RegistrarUsuarioAsync([FromBody] UsuarioDto usuarioDto)
    {
        var validationResult = await validator.ValidateAsync(usuarioDto);       
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors.GroupBy(x => x.PropertyName)
              .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.ErrorMessage).ToArray()
              ));
        }
        var usuario = new IdentityUser 
        {
            UserName = usuarioDto.Email,
            Email = usuarioDto.Email, 
            EmailConfirmed = true 
        };
        var result = await userManager.CreateAsync(usuario, usuarioDto.Senha);
        if (!result.Succeeded)
        {
            return BadRequest($"Falha ao registrar usuário : {result.Errors}");
        }
        await signInManager.SignInAsync(usuario, isPersistent: false);

        return Ok("Usuário Criado com sucesso!");
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] UsuarioDto usuarioDto)
    {
        var validationResult = await validator.ValidateAsync(usuarioDto);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors.GroupBy(x => x.PropertyName)
              .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.ErrorMessage).ToArray()
              ));
        }
        var result = await signInManager.PasswordSignInAsync(usuarioDto.Email!, usuarioDto.Senha!, isPersistent: false, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            return BadRequest("Falha no login do usuário.");
        }
        return Ok("Logado com sucesso!");
    }
}
