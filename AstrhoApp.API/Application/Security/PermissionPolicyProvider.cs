using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace AstrhoApp.API.Security;

public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private const string Prefix = "perm:";
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
        if (policyName != null && policyName.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
        {
            var permission = policyName.Substring(Prefix.Length);
            var policy = new AuthorizationPolicyBuilder()
                .RequireClaim("permission", permission)
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return _fallbackProvider.GetPolicyAsync(policyName);
    }
}