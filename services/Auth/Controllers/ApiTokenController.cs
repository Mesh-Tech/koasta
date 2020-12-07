using Microsoft.AspNetCore.Mvc;
using Koasta.Service.Auth.Utils;
using Koasta.Shared.Middleware;
using Koasta.Shared.Repository;
using System;
using System.Threading.Tasks;

namespace Koasta.Service.Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(AuthorisationMiddleware))]
    public class ApiTokenController : ControllerBase
    {
        private const int MaxTokenExpiryInDays = 30;

        private readonly TokenRepository _tokenRepository;
        private readonly TokenUtil _tokenUtil;

        public ApiTokenController(TokenRepository tokenRepository, TokenUtil tokenUtil)
        {
            _tokenRepository = tokenRepository;
            _tokenUtil = tokenUtil;
        }

        [HttpPost]
        [ActionName("createToken")]
        public async Task<IActionResult> CreateToken(string description)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(description))
                {
                    return BadRequest();
                }

                var token = _tokenUtil.GenerateToken();
                var expiry = DateTime.Now.AddDays(MaxTokenExpiryInDays);

                var result = await _tokenRepository.InsertApiAuthToken(token, description, expiry).ConfigureAwait(false);

                if (result.IsSuccess)
                {
                    return Ok();
                }

                return BadRequest();
            }
            catch (Exception)
            {
                // Use a logging solution to log error
                return StatusCode(500);
            }
        }

        [HttpGet]
        [ActionName("getToken")]
        public async Task<IActionResult> GetApiToken(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    return BadRequest();
                }

                var result = await _tokenRepository.GetApiAuthToken(token).ConfigureAwait(false);

                if (result.IsSuccess)
                {
                    return Ok();
                }

                return NotFound();
            }
            catch (Exception)
            {
                // Use a logging solution to log error
                return StatusCode(500);
            }
        }

        [HttpPost]
        [ActionName("updateToken")]
        [Route("update")]
        public async Task<IActionResult> UpdateToken(int id, string description)
        {
            try
            {
                var token = _tokenUtil.GenerateToken();

                var result = await _tokenRepository.UpdateApiAuthToken(id, token, description, DateTime.Now.AddDays(MaxTokenExpiryInDays)).ConfigureAwait(false);

                if (result.IsSuccess)
                {
                    return Ok();
                }

                return NotFound();
            }
            catch (Exception)
            {
                // Use a logging solution to log error
                return StatusCode(500);
            }
        }

        [HttpDelete]
        [ActionName("deleteToken")]
        public async Task<IActionResult> RemoveToken(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    return BadRequest();
                }

                var tokenResult = await _tokenRepository.GetApiAuthToken(token).ConfigureAwait(false);

                if (tokenResult.IsSuccess)
                {
                    var tokenToRemove = tokenResult.Value.HasValue ? tokenResult.Value.Value : null;

                    var result = await _tokenRepository.RemoveApiAuthToken(tokenToRemove.ApiTokenId).ConfigureAwait(false);

                    if (result.IsSuccess)
                    {
                        return Ok();
                    }

                    return BadRequest();
                }

                return NotFound();
            }
            catch (Exception)
            {
                // Use a logging solution to log error
                return StatusCode(500);
            }
        }
    }
}
