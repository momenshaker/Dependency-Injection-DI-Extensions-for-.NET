Dependency Injection (DI) Extensions for .NET
=============================================

A set of advanced Dependency Injection (DI) extensions for .NET applications. This library includes enhancements to the standard DI framework, providing additional functionality for custom lifetimes, scoped services, session-based services, interceptors, and service decorators. It aims to simplify and extend the flexibility of service registration and management within the DI container.

Features
--------

-   **Custom Service Lifetimes**: Add services with custom lifetimes (per user session, per request, etc.).
-   **Method Interceptors**: Intercept method calls to services for custom behavior.
-   **Asynchronous Interceptors**: Intercept asynchronous method calls.
-   **Lazy-Loaded Services**: Delay service initialization until it's actually needed.
-   **Session-Based Services**: Handle services that should persist throughout a user's session.
-   **Decorator Pattern**: Add decorators to services for additional functionality.

Installation
------------

You can install this package via NuGet:

```bash 
dotnet add package DependencyInjection.Extensions
```

Or, via the NuGet package manager in Visual Studio:

1.  Right-click on your project in Solution Explorer.
2.  Click on "Manage NuGet Packages".
3.  Search for `DependencyInjection.Extensions` and click "Install".

Usage
-----

### 1\. Custom Service Lifetimes

You can register services with custom lifetimes using the extensions provided by this library.

```bash
public void ConfigureServices(IServiceCollection services)
{
    // Register a service with a custom lifetime (per user session)
    services.AddSingletonPerUserSession<MyService>();
}
```

### 2\. Method Interceptors

Add method-level interceptors to modify service behavior.
```bash 
public void ConfigureServices(IServiceCollection services)
{
    services.AddMethodInterceptor<IMyService>(service =>
    {
        // Custom logic to execute before or after method calls
    });
}
```

### 3\. Asynchronous Interceptors

Add asynchronous interceptors for asynchronous service methods.
```bash 
public void ConfigureServices(IServiceCollection services)
{
    services.AddAsyncInterceptor<IMyService>(async service =>
    {
        // Custom async logic to execute before or after asynchronous calls
    });
}
```

### 4\. Lazy Interceptors

Use lazy initialization for deferred service creation.

```bash
public void ConfigureServices(IServiceCollection services)
{
    services.AddLazyInterceptor<IMyService>(service =>
    {
        // Lazy initialization logic
        return new Lazy<IMyService>(() => service);
    });
}
```

### 5\. Session-Based Services

Register services that are tied to the user session, ensuring they are cleaned up when the session ends.

```bash
public void ConfigureServices(IServiceCollection services)
{
    services.AddPerUser<MyService>();
}
```

### 6\. Service Decorators

Add decorators to wrap existing services with additional behavior.

```bash
public void ConfigureServices(IServiceCollection services)
{
    services.AddDecorator<IMyService>(
        typeof(MyServiceDecorator1),
        typeof(MyServiceDecorator2)
    );
}
```

Contributing
------------

We welcome contributions to this project. If you'd like to contribute, please follow these steps:

1.  Fork the repository.
2.  Create a new branch for your feature or bug fix.
3.  Make your changes and commit them.
4.  Open a pull request to the `main` branch of this repository.

Please ensure that your code adheres to the existing style and includes tests for any new functionality or changes.

License
-------

This project is licensed under the MIT License - see the LICENSE file for details.

* * * * *

Example Code
------------

Here's an example of how to register and use session-based services:

```bash
public class MyService : IDisposable
{
    public void Dispose()
    {
        // Cleanup logic
    }
}

// In your ConfigureServices method
public void ConfigureServices(IServiceCollection services)
{
    services.AddPerUser<MyService>();
}

```

This will register `MyService` to be tied to the user session, and will ensure that the service is disposed of when the session ends.
