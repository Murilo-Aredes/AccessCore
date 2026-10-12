using System.Reflection;
using AuthService.Application.Autorizacao;
using AuthService.Application.Common;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddScoped<IExecutorRequisicoes, ExecutorRequisicoes>();
        services.AddScoped<IServicoAutorizacaoUsuario, ServicoAutorizacaoUsuario>();
        services.AddValidatorsFromAssembly(assembly);

        foreach (var tipo in assembly.GetTypes().Where(t => t is { IsAbstract: false, IsInterface: false }))
        {
            var interfacesManipulador = tipo.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IManipuladorRequisicao<,>));

            foreach (var interfaceManipulador in interfacesManipulador)
            {
                services.AddScoped(interfaceManipulador, tipo);
            }
        }

        return services;
    }
}
