using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace CoinbaseWebSocketClient.Utilities
{
    public static class JwtBuilder
    {
        public static string BuildJwt(string privateKey, string name)
        {
            using (ECDsa ecdsa = ECDsa.Create())
            {
                ecdsa.ImportFromPem(privateKey);
                var securityKey = new ECDsaSecurityKey(ecdsa);

                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.EcdsaSha256);

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, name),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                };

                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(5),
                    signingCredentials: credentials
                );

                var tokenHandler = new JwtSecurityTokenHandler();
                return tokenHandler.WriteToken(token);
            }
        }
    }
}