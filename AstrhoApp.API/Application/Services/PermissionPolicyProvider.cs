using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace AstrhoApp.API.Services
{
    // Proveedor dinámico que crea policies para permisos solicitados con el prefijo "perm:"
    // Uso: [Authorize(Policy = "perm:Servicio")]
    public class PermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        // Fallback al provider por defecto para políticas ya registradas
        private readonly DefaultAuthorizationPolicyProvider _fallbackProvider;

        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            _fallbackProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public Task<AuthorizationPolicy?> GetDefaultPolicyAsync()
            => _fallbackProvider.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
            => _fallbackProvider.GetFallbackPolicyAsync();

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            // Si la policy ya fue registrada, dejar que el fallback la devuelva
            var policy = _fallbackProvider.GetPolicyAsync(policyName);
            if (policy.Status == TaskStatus.RanToCompletion && policy.Result != null)
            {
                return policy;
            }

            // Si la policy comienza con "perm:" la construimos dinámicamente
            const string prefix = "perm:";
            if (policyName.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
            {
                var permiso = policyName[prefix.Length..].Trim();

                var policyBuilder = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme);
                policyBuilder.RequireAuthenticatedUser();

                policyBuilder.RequireAssertion(context =>
                    context.User.HasClaim(c =>
                        c.Type == "permission" &&
                        string.Equals(c.Value?.Trim(), permiso, StringComparison.OrdinalIgnoreCase)
                    )
                );
                var dynamicPolicy = policyBuilder.Build();
                return Task.FromResult<AuthorizationPolicy?>(dynamicPolicy);
            }

            // si no coincide, devolver fallback (null)
            return Task.FromResult<AuthorizationPolicy?>(null);
        }
    }
}