Introducing CRISP v1.0.0: A Modern Architecture for Scalable Applications
I'm excited to announce the initial release of CRISP v1.0.0, a lightweight yet powerful architectural framework that brings structure and scalability to .NET applications. This first release marks an important milestone in our journey to simplify complex application development while maintaining flexibility and performance.
Photo by Sigmund on UnsplashFriendly-Link for non-members.
What is CRISP?
CRISP stands for Command, Response, Interface-driven, Service-oriented Pattern - a comprehensive architectural approach that combines several battle-tested design patterns:
Command Query Responsibility Segregation (CQRS): Separates read operations (queries) from write operations (commands)
Vertical Layers: Organizes functionality into independent modules with their own commands, queries, services, and endpoints
Request-Endpoint-Response (REPR): Standardizes application interactions for consistency and predictability

At its core, CRISP promotes modularity, maintainability, and scalability by organizing your application into focused, reusable components that each handle a specific concern.
Real-world Use Cases
CRISP excels in several common application scenarios:
Enterprise Applications: Where clear separation of concerns and maintainability are critical
Microservices Architecture: Providing structure for individual services with well-defined boundaries
Domain-rich Applications: Where complex business rules need organization and clarity
Growing Applications: Where initial simplicity needs to evolve without major refactoring

For example, in an e-commerce application, you might organize product catalog, shopping cart, and order processing into separate modules, each with their own commands and queries:
📂 ProductCatalog
  📂 Commands
    📄 CreateProduct.cs
    📄 UpdateProductPrice.cs
  📂 Queries
    📄 GetProductById.cs
    📄 SearchProducts.cs
📂 ShoppingCart
  📂 Commands
    📄 AddCartItem.cs
    📄 RemoveCartItem.cs
  📂 Queries
    📄 GetCartItems.cs
How CRISP v1.0.0 Helps Implement This Architecture
The CRISP v1.0.0 NuGet package provides the essential building blocks for implementing this architecture in your .NET applications:
Core Components
Mediator Pattern: A lightweight mediator implementation for routing commands and queries to their appropriate handlers
Basic Validation System: Built-in request validation with support for custom validators
Pipeline Behaviors: Extensible behaviors for cross-cutting concerns like validation and performance monitoring
Event Handling: Support for events with both synchronous and asynchronous processing options
Resilience Patterns: Basic circuit breaker and timeout implementations to improve application stability

Key Benefits
Reduced Boilerplate: Get started quickly with base abstractions for commands, queries, and handlers
Flexible Integration: Easily integrates with ASP.NET Core and other .NET frameworks
Testability: Design promotes clean separation of concerns, making unit testing straightforward
Consistent Patterns: Standardized approach to handling operations throughout your application

Code Example: Simple CRISP Implementation
Here's how to implement a simple query using CRISP:
// Define a query
public class GetUserByIdQuery : ByIdQuery<UserDto>
{
}
// Create the handler
public class GetUserByIdHandler : BaseHandler<GetUserByIdQuery, UserDto>
{
    private readonly IUserRepository _userRepository;
    
    public GetUserByIdHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<Response<UserDto>> HandleAsync(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(query.UserId, cancellationToken);
        if (user == null)
            return Response.NotFound<UserDto>($"User with ID {query.UserId} not found");
        return Response.Success(new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email
        });
    }
}
// Use it in your API controller
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var response = await _mediator.Send(new GetUserByIdQuery { Id = id });
        return response.ToActionResult();
    }
}
Comparison with Alternatives
How does CRISP compare to other popular CQRS frameworks?
| Feature | CRISP | MediatR | Other Frameworks |
| - - - - - - - - - | - - - - - - - - - - - - - | - - - - - - - - - - | - - - - - - - - - - - - - |
| Size | Lightweight | Lightweight | Often heavyweight |
| Response Type | Standardized `Response<T>` | Raw return type | Varies |
| Pipeline Behaviors | ✓ | ✓ | Varies |
| Validation | Built-in | Separate packages | Varies |
| Resilience | Basic patterns included | Requires extensions | Often requires extensions |
| Learning Curve | Moderate | Low | Often steep |
CRISP differentiates itself by providing more built-in functionality while maintaining a clean, focused API surface.
What's Next for CRISP?
Looking ahead, we have an exciting roadmap for expanding the CRISP ecosystem. Based on our development priorities, we're planning to introduce several specialized libraries and updates:
1. Expanding validation to be more like Fluent Validation
2. CRISP.Diagnostics: Enhanced logging, distributed tracing, and health checks for better observability
3. CRISP.Caching: Powerful caching mechanisms for improved performance, including support for distributed scenarios
4. CRISP.Resilience: Advanced fault tolerance with expanded circuit breaker implementations, bulkhead patterns, and robust retry policies
5. CRISP.Security: Comprehensive security features including resource-based authorization and field-level encryption
6. CRISP.Performance: Optimization techniques like projection mapping, pagination, and background task processing
Future releases will also focus on improving developer productivity with tools for code generation, analysis, and scaffolding of CRISP modules.
Performance Considerations
CRISP has been designed with performance in mind:
Lightweight Mediator: Minimal overhead for request dispatching
Efficient Pipeline: Optimized behavior execution path
Memory Allocation: Careful attention to object allocation patterns
Async by Default: Built for modern asynchronous workflows

In our benchmarks, CRISP performs comparably to raw handler invocation while providing significant architectural benefits.
Getting Started
To start using CRISP in your projects, install the NuGet package:
dotnet add package CRISP
Then, configure CRISP in your application:
// In Program.cs or Startup.cs
services.AddCrisp(options => 
{
    // Configure validation, events, and resilience options
    options.ConfigureValidation(validation => 
    {
        validation.MaxValidationDepth = 5;
        validation.LogValidationFailures = true;
    });
    // Enable channel-based event processing
    options.UseChannelEventProcessing();
});
Advanced Configuration
CRISP offers several advanced configuration options:
services.AddCrisp(options => 
{
    // Configure event processing
    options.ConfigureEvents(events => 
    {
        events.UseBackgroundProcessing = true;
        events.BackgroundQueueCapacity = 1000;
        events.ProcessingStrategy = EventProcessingStrategy.ParallelForEachEvent;
    });
    // Configure resilience
    options.ConfigureResilience(resilience => 
    {
        resilience.DefaultTimeoutMs = 30000;
        resilience.CircuitBreakerFailureThreshold = 0.5;
        resilience.CircuitBreakerSamplingDurationSec = 60;
    });
    // Register custom pipeline behaviors
    options.AddPipelineBehavior<LoggingBehavior>();
    options.AddPipelineBehavior<PerformanceBehavior>();
});
Join the Community
We believe the best frameworks are built with community input. Here's how you can get involved:
GitHub: Star and watch our repository for updates
Issues: Report bugs or suggest features through GitHub issues
Discussions: Join conversations about best practices and patterns
Contributions: Submit pull requests to improve CRISP
Samples: Share your implementation examples with others

Conclusion
CRISP v1.0.0 is just the beginning of our journey to simplify architectural complexity while promoting best practices in software development. By combining proven patterns like CQRS, vertical layers, and REPR into a cohesive framework, CRISP enables teams to build applications that are not only scalable and maintainable but also intuitive and consistent.
We're excited to see how developers will use CRISP in their projects and welcome feedback from the community to help shape future releases.
Contact us:
GitHub - https://github.com/Narcoleptic-Fox/CRISP
Nuget - https://www.nuget.org/packages/Crisp/
Narcoleptic Fox - https://narcolepticfox.com

Stay tuned for more blog posts diving deeper into specific aspects of CRISP and upcoming releases!