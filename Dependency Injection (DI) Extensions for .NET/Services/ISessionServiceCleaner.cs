using System;

namespace DependencyInjection.Extensions
{
    public interface ISessionServiceCleaner
    {
        /// <summary>
        /// Registers a disposable service associated with the session for cleanup.
        /// </summary>
        /// <param name="sessionId">The session ID associated with the service.</param>
        /// <param name="service">The disposable service to register.</param>
        void RegisterServiceForCleanup(string sessionId, IDisposable service);

        /// <summary>
        /// Cleans up the services for the provided session ID, disposing of any registered disposable services.
        /// </summary>
        /// <param name="sessionId">The session ID whose services should be cleaned up.</param>
        void CleanUp(string sessionId);
    }
}
