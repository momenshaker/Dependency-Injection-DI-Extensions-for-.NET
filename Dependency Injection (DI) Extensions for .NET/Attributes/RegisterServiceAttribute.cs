using Microsoft.Extensions.DependencyInjection;
using System;

namespace DependencyInjection.Extensions.Attributes
{
    /// <summary>
    /// Custom attribute to specify the service lifetime for a given service.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class RegisterServiceAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the lifetime of the service.
        /// </summary>
        public ServiceLifetime Lifetime { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterServiceAttribute"/> class.
        /// </summary>
        /// <param name="lifetime">The lifetime of the service.</param>
        public RegisterServiceAttribute(ServiceLifetime lifetime)
        {
            Lifetime = lifetime;
        }
    }
}
