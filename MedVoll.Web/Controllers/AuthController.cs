using FluentValidation;
using MedVoll.Web.Dtos;
using MedVoll.Web.Models;
using MedVoll.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MedVoll.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController:ControllerBase
{
    private readonly UserManager<VollMedUser> userManager;
    private readonly SignInManager<VollMedUser> signInManager;
    private readonly IValidator<UsuarioDto> validator;
    private readonly TokenJWTService tokenJWTService;

    public AuthController(UserManager<VollMedUser> userManager, SignInManager<VollMedUser> signInManager, IValidator<UsuarioDto> validator, TokenJWTService tokenJWTService)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.validator = validator;
        this.tokenJWTService = tokenJWTService;
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

        var usuarioReg = await userManager.FindByEmailAsync(usuarioDto.Email!);
        if (usuarioReg is not null)
        {
            return BadRequest("Usuário já foi registrado na base de dados.");
        }

        var usuario = new VollMedUser
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

        return Ok(new {Mensagem="Usuario registrado com sucesso!",Token= tokenJWTService.GerarTokenDeUsuario(usuarioDto)});
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
        var usuario = await userManager.FindByEmailAsync(usuarioDto.Email!);
        if (usuario is null)
        {
            return BadRequest("usuário não encontrado.");
        }
        
        var refeshToken = tokenJWTService.GerarRefreshToken();
        
        var result = await signInManager.PasswordSignInAsync(usuarioDto.Email!, usuarioDto.Senha!, isPersistent: false, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            return BadRequest("Falha no login do usuário.");
        }
        var userTokenDto = tokenJWTService.GerarTokenDeUsuario(usuarioDto);
        userTokenDto.RefreshToken = refeshToken;

        return Ok(new { Mensagem = "Login realizado com sucesso!", Token = userTokenDto });
    }
}
