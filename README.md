<p align="center">
  <img src="assets/icon.png" alt="CRISP Framework Logo" width="200" height="200"/>
</p>

<p align="center">
  <img src="assets/comapny_logo.png" alt="Company Logo" height="50"/>
</p>

# CRISP: Core Reusable Infrastructure for Structured Programming

[![Build Status](https://img.shields.io/github/actions/workflow/status/Dieshen/CRISP/build.yml?branch=main&style=flat-square)](https://github.com/Dieshen/CRISP/actions)
[![NuGet](https://img.shields.io/nuget/v/CRISP.Core.svg?style=flat-square)](https://www.nuget.org/packages/CRISP.Core/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/CRISP.Core.svg?style=flat-square)](https://www.nuget.org/packages/CRISP.Core/)
[![License](https://img.shields.io/github/license/Dieshen/CRISP?style=flat-square)](LICENSE.txt)
[![Target Frameworks](https://img.shields.io/badge/targets-net9.0%20|%20netstandard2.1-blue?style=flat-square)](https://github.com/Dieshen/CRISP/)
[![Test Coverage](https://img.shields.io/codecov/c/github/Dieshen/CRISP/main?style=flat-square)](https://codecov.io/gh/Dieshen/CRISP)

## Table of Contents

- [CRISP: Core Reusable Infrastructure for Structured Programming](#crisp-core-reusable-infrastructure-for-structured-programming)
  - [Table of Contents](#table-of-contents)
  - [Overview](#overview)
  - [Key Features](#key-features)
  - [Prerequisites](#prerequisites)
  - [Getting Started](#getting-started)
    - [Installation](#installation)
  - [Quick Start](#quick-start)
    - [Basic Setup](#basic-setup)
  - [Framework Compatibility](#framework-compatibility)
  - [Usage Examples](#usage-examples)
    - [Creating Commands](#creating-commands)
    - [Creating Queries](#creating-queries)
    - [Using Channel-Based Event Processing](#using-channel-based-event-processing)
    - [Adding Validation](#adding-validation)
    - [Using Domain Events](#using-domain-events)
    - [Using Resilience Patterns](#using-resilience-patterns)
    - [Organizing code into modules](#organizing-code-into-modules)
  - [Architecture](#architecture)
  - [Extending CRISP](#extending-crisp)
    - [Creating a Custom Behavior](#creating-a-custom-behavior)
  - [License](#license)
  - [Acknowledgments](#acknowledgments)

## Overview

CRISP is a lightweight, modular .NET framework designed to provide a clean architecture foundation for building scalable and maintainable applications. It implements modern software architecture patterns like Mediator, CQRS (Command Query Responsibility Segregation), Domain Events, and Resilience Patterns to help you build robust applications with minimal boilerplate.

## Key Features

- **Mediator Pattern**: Decouple components through a central messaging system
- **CQRS Implementation**: Separate command and query responsibilities
- **Domain Events**: Enable event-driven architecture with loose coupling
- **Resilience Patterns**: Add robustness with retry, circuit breaker, and timeout strategies
- **Validation Pipeline**: Automatic validation of requests using a pipeline behavior
- **Modular Architecture**: Organize code into cohesive modules
- **Configurable Options**: Fine-tune framework behavior through comprehensive options
- **Minimal Dependencies**: Focused core with minimal external dependencies
- **Channel-Based Event Processing**: High-throughput event processing using System.Threading.Channels
- **Multi-Targeting**: Supports both .NET 9.0 and .NET Standard 2.1

## Prerequisites

- .NET 9.0 SDK or later for .NET 9.0 target
- .NET Core 3.0 SDK or later for .NET Standard 2.1 target

## Getting Started

### Installation

Add the CRISP.Core package to your project:

```bash
dotnet add package CRISP.Core
```

## Quick Start

```csharp
// 1. Add using statement
using CRISP.Core.Extensions;

// 2. Register CRISP in your DI container
services.AddCrispFromAssemblies(typeof(Program).Assembly);

// 3. Inject and use the mediator
public class MyController
{
    private readonly IMediator _mediator;
    
    public MyController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    public async Task<IActionResult> GetUser(Guid id)
    {
        var result = await _mediator.Send(new GetUserQuery { UserId = id });
        return Ok(result);
    }
}
```

### Basic Setup

Register CRISP services in your application:

```csharp
// Program.cs or Startup.cs
using CRISP.Core.Extensions;

// Add CRISP with default options
services.AddCrispFromAssemblies(typeof(Program).Assembly);

// Or with custom options
services.AddCrispFromAssemblies(options => {
    options.ConfigureResilience(resilience => {
        resilience.Retry.MaxRetryAttempts = 5;
        resilience.CircuitBreaker.FailureThreshold = 3;
    });
    
    options.ConfigureEvents(events => {
        events.ProcessEventsInParallel = true;
        events.MaxDegreeOfParallelism = 4;
    });
}, 
typeof(Program).Assembly);
```

## Framework Compatibility

CRISP.Core supports multiple target frameworks:

| Framework | Version |
|-----------|---------|
| .NET | 9.0+ |
| .NET Standard | 2.1+ |
| .NET Core | 3.0+ |
| Mono | 6.4+ |
| Xamarin | iOS 12.16+, Android 10.0+ |

This multi-targeting approach ensures that CRISP can be used in a wide range of applications, from the latest .NET 9.0 projects to older .NET Core 3.x applications.

## Usage Examples

### Creating Commands

```csharp
public class CreateUserCommand : Command
{
    public string Username { get; set; }
    public string Email { get; set; }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IEventDispatcher _eventDispatcher;

    public CreateUserCommandHandler(
        IUserRepository userRepository, 
        IEventDispatcher eventDispatcher)
    {
        _userRepository = userRepository;
        _eventDispatcher = eventDispatcher;
    }

    public async ValueTask Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User(request.Username, request.Email);
        
        await _userRepository.AddAsync(user, cancellationToken);
        
        await _eventDispatcher.Dispatch(new UserCreatedEvent(user.Id), cancellationToken);
    }
}
```

### Creating Queries

```csharp
public class GetUserQuery : Query<UserDto>
{
    public Guid UserId { get; set; }
}

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserDto>
{
    private readonly IUserRepository _userRepository;

    public GetUserQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async ValueTask<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        return user != null 
            ? new UserDto { Id = user.Id, Username = user.Username, Email = user.Email }
            : null;
    }
}
```

### Using Channel-Based Event Processing

For applications that need high-throughput event processing, you can use CRISP's channel-based event dispatcher, which leverages System.Threading.Channels for efficient asynchronous event handling:

```csharp
// Program.cs or Startup.cs
using CRISP.Core.Extensions;

services.AddCrispFromAssemblies(options => {
    // Enable channel-based event processing with custom configuration
    options.UseChannelEventProcessing(channelOptions => {
        // Configure a bounded channel with capacity of 10,000 events
        channelOptions.ChannelCapacity = 10000;
        
        // Set the number of consumers processing events from the channel
        // (Default is Environment.ProcessorCount)
        channelOptions.ConsumerCount = 8;
        
        // Configure how long to wait when the channel is full (in milliseconds)
        channelOptions.FullChannelWaitTimeMs = 5000;
        
        // Wait for all events to be processed during application shutdown
        channelOptions.WaitForChannelDrainOnDispose = true;
        channelOptions.ChannelDrainTimeoutMs = 30000; // 30 seconds
    });
    
    // Additional event configuration
    options.ConfigureEvents(events => {
        // Enable parallel processing of events within each consumer
        events.ProcessEventsInParallel = true;
        events.MaxDegreeOfParallelism = 4;
    });
}, typeof(Program).Assembly);
```

This configuration creates a high-performance event processing pipeline that:

1. Buffers events in a bounded channel with capacity for 10,000 events
2. Processes events using 8 concurrent consumers
3. Waits up to 5 seconds when the channel is full
4. Processes events in parallel within each consumer
5. Ensures events are fully processed during application shutdown

The channel-based event dispatcher is ideal for scenarios with high event volume or when you need to decouple event dispatching from processing for better performance.

### Adding Validation

```csharp
public class CreateUserCommandValidator : IValidator<CreateUserCommand>
{
    public ValidationResult Validate(CreateUserCommand request)
    {
        var errors = new List<ValidationError>();
        
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            errors.Add(new ValidationError("Username", "Username is required"));
        }
        
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors.Add(new ValidationError("Email", "Email is required"));
        }
        else if (!IsValidEmail(request.Email))
        {
            errors.Add(new ValidationError("Email", "Email is not valid"));
        }
        
        return errors.Count > 0
            ? ValidationResult.Failure(errors.ToArray())
            : ValidationResult.Success();
    }
    
    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
```

### Using Domain Events

```csharp
public class UserCreatedEvent : DomainEvent
{
    public Guid UserId { get; }

    public UserCreatedEvent(Guid userId)
    {
        UserId = userId;
    }
}

public class EmailNotificationHandler : IEventHandler<UserCreatedEvent>
{
    private readonly IEmailService _emailService;

    public EmailNotificationHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async ValueTask Handle(UserCreatedEvent @event, CancellationToken cancellationToken)
    {
        await _emailService.SendWelcomeEmailAsync(@event.UserId, cancellationToken);
    }
}
```

### Using Resilience Patterns

```csharp
public class ExternalApiService
{
    private readonly IResilienceStrategy _resilienceStrategy;
    
    public ExternalApiService(IResilienceStrategy resilienceStrategy)
    {
        _resilienceStrategy = resilienceStrategy;
    }
    
    public async Task<ApiResult> CallExternalApiAsync(string endpoint, CancellationToken cancellationToken)
    {
        return await _resilienceStrategy.Execute(async (ct) => {
            // Make an HTTP call that might fail transiently
            using var client = new HttpClient();
            var response = await client.GetAsync(endpoint, ct);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<ApiResult>(content);
        }, cancellationToken);
    }
}
```

### Organizing code into modules

```csharp
public class UsersModule : ModuleBase
{
    public override void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
    }
}
```

## Architecture

CRISP follows a clean, modular architecture that separates concerns into distinct components:

- **Commands**: Write operations that change state
- **Queries**: Read operations that retrieve data
- **Events**: Notifications that something has occurred
- **Handlers**: Business logic for processing commands, queries, and events
- **Behaviors**: Cross-cutting concerns like validation and logging
- **Resilience**: Patterns to handle transient failures and exceptions
- **Modules**: Organizational units that group related functionality

## Extending CRISP

CRISP is designed to be extensible. You can create custom behaviors, validators, or event handlers to add new functionality.

### Creating a Custom Behavior

```csharp
public class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IAuditService _auditService;
    
    public AuditBehavior(IAuditService auditService)
    {
        _auditService = auditService;
    }
    
    public async ValueTask<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        await _auditService.RecordRequest(request, cancellationToken);
        
        var response = await next();
        
        await _auditService.RecordResponse(response, cancellationToken);
        
        return response;
    }
}
```

## License

This project is licensed under the MIT License

## Acknowledgments

CRISP draws inspiration from several established patterns and libraries, including MediatR, CQRS, and the Resilience pattern. It aims to provide a lightweight, focused implementation that adheres to .NET best practices.