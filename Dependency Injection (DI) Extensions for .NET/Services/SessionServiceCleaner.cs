using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;

namespace DependencyInjection.Extensions;

public class SessionServiceCleaner : ISessionServiceCleaner
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private static readonly ConcurrentDictionary<string, IDisposable> _userSessionDisposables = new ConcurrentDictionary<string, IDisposable>();

    public SessionServiceCleaner(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Registers a disposable service associated with the session for cleanup.
    /// </summary>
    /// <param name="sessionId">The session ID associated with the service.</param>
    /// <param name="service">The disposable service to register.</param>
    public void RegisterServiceForCleanup(string sessionId, IDisposable service)
    {
        if (!string.IsNullOrEmpty(sessionId) && service != null)
        {
            _userSessionDisposables[sessionId] = service;
        }
    }

    /// <summary>
    /// Cleans up the services for the provided session ID, disposing of any registered disposable services.
    /// </summary>
    /// <param name="sessionId">The session ID whose services should be cleaned up.</param>
    public void CleanUp(string sessionId)
    {
        if (!string.IsNullOrEmpty(sessionId) && _userSessionDisposables.ContainsKey(sessionId))
        {
            var disposableService = _userSessionDisposables[sessionId];
            disposableService?.Dispose();
            _userSessionDisposables.TryRemove(sessionId, out _);
        }
    }
}
