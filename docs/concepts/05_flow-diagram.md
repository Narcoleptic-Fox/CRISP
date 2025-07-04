# CRISP Flow Diagrams

Visual guide to how requests flow through CRISP.

## 🔄 Basic Request Flow

```mermaid
graph TD
    A[HTTP Request] --> B{Route Matching}
    B --> C[Deserialize to Command/Query]
    C --> D[Pipeline Start]
    D --> E[Validation Behavior]
    E --> F{Valid?}
    F -->|No| G[400 Bad Request]
    F -->|Yes| H[Logging Behavior]
    H --> I[Handler Execution]
    I --> J[Business Logic]
    J --> K[Return Result]
    K --> L[Pipeline End]
    L --> M[Serialize Response]
    M --> N[HTTP Response]
```

## 📊 Command Flow (Write Operation)

```mermaid
sequenceDiagram
    participant Client
    participant Endpoint
    participant Pipeline
    participant Validator
    participant Handler
    participant Database
    
    Client->>Endpoint: POST /api/users/create
    Endpoint->>Pipeline: CreateUserCommand
    Pipeline->>Validator: Validate Command
    Validator-->>Pipeline: Valid ✓
    Pipeline->>Handler: Handle(command)
    Handler->>Database: INSERT User
    Database-->>Handler: User Created
    Handler-->>Pipeline: User Result
    Pipeline-->>Endpoint: User
    Endpoint-->>Client: 201 Created + User JSON
```

## 🔍 Query Flow (Read Operation)

```mermaid
sequenceDiagram
    participant Client
    participant Endpoint
    participant Pipeline
    participant Cache
    participant Handler
    participant Database
    
    Client->>Endpoint: GET /api/users/get?id=123
    Endpoint->>Pipeline: GetUserQuery(123)
    Pipeline->>Cache: Check Cache
    Cache-->>Pipeline: Miss ✗
    Pipeline->>Handler: Handle(query)
    Handler->>Database: SELECT User
    Database-->>Handler: User Data
    Handler-->>Pipeline: User
    Pipeline->>Cache: Store Result
    Pipeline-->>Endpoint: User
    Endpoint-->>Client: 200 OK + User JSON
```

## 🛡️ Pipeline Behavior Chain

```mermaid
graph LR
    A[Request] --> B[Behavior 1<br/>Logging]
    B --> C[Behavior 2<br/>Validation]
    C --> D[Behavior 3<br/>Authorization]
    D --> E[Behavior 4<br/>Transaction]
    E --> F[Handler]
    F --> G[Behavior 4<br/>Commit/Rollback]
    G --> H[Behavior 3<br/>Audit Log]
    H --> I[Behavior 2<br/>Response Validation]
    I --> J[Behavior 1<br/>Log Complete]
    J --> K[Response]
```

## 🔀 Conditional Flow (State Machine)

```mermaid
stateDiagram-v2
    [*] --> Received: Command Received
    Received --> Validating: Start Validation
    Validating --> Invalid: Validation Failed
    Validating --> Valid: Validation Passed
    Invalid --> [*]: Return 400
    Valid --> Processing: Execute Handler
    Processing --> Success: Handler Success
    Processing --> Error: Handler Error
    Success --> [*]: Return 200/201
    Error --> Retrying: Transient Error
    Error --> Failed: Permanent Error
    Retrying --> Processing: Retry Attempt
    Failed --> [*]: Return 500
```

## 🎮 Game Command Flow

```mermaid
graph TD
    A[Player Input] --> B[Attack Command]
    B --> C{Valid Target?}
    C -->|No| D[Invalid Command]
    C -->|Yes| E[Check Player State]
    E --> F{Can Attack?}
    F -->|No| G[Action Blocked]
    F -->|Yes| H[Calculate Damage]
    H --> I[Apply Damage]
    I --> J[Update Game State]
    J --> K[Broadcast Event]
    K --> L[Return Combat Result]
    
    style A fill:#f9f,stroke:#333,stroke-width:2px
    style L fill:#9f9,stroke:#333,stroke-width:2px
```

## 🔄 Error Handling Flow

```mermaid
flowchart TD
    A[Handler Execution] --> B{Exception?}
    B -->|No| C[Success Response]
    B -->|Yes| D{Exception Type}
    D -->|Validation| E[400 Bad Request]
    D -->|NotFound| F[404 Not Found]
    D -->|Unauthorized| G[401 Unauthorized]
    D -->|Business| H[422 Unprocessable]
    D -->|Transient| I{Retry?}
    D -->|Unknown| J[500 Server Error]
    I -->|Yes| K[Retry Handler]
    I -->|No| L[503 Service Unavailable]
    K --> A
```

## 📦 Vertical Layer Communication

```mermaid
graph TB
    subgraph "User Feature"
        A1[CreateUserCommand]
        A2[User Handler]
        A3[User Created Event]
    end
    
    subgraph "Email Feature"
        B1[Email Handler]
        B2[Send Welcome Email]
    end
    
    subgraph "Analytics Feature"
        C1[Analytics Handler]
        C2[Track User Signup]
    end
    
    A2 --> A3
    A3 -.->|Event Bus| B1
    A3 -.->|Event Bus| C1
    B1 --> B2
    C1 --> C2
```

## 🚀 Performance Flow

```mermaid
gantt
    title Request Processing Timeline
    dateFormat X
    axisFormat %L ms
    
    section Client
    HTTP Request      :0, 10
    HTTP Response     :180, 20
    
    section Endpoint
    Route Matching    :10, 5
    Deserialization   :15, 10
    Serialization     :170, 10
    
    section Pipeline
    Validation        :25, 15
    Authorization     :40, 10
    Logging Start     :50, 5
    Logging End       :165, 5
    
    section Handler
    Business Logic    :55, 80
    Database Query    :75, 40
    
    section Cache
    Cache Check       :55, 5
    Cache Store       :135, 5
```

## 🔐 Authentication Flow

```mermaid
sequenceDiagram
    participant Client
    participant Endpoint
    participant AuthBehavior
    participant TokenService
    participant Handler
    
    Client->>Endpoint: Request + JWT Token
    Endpoint->>AuthBehavior: Check Authorization
    AuthBehavior->>TokenService: Validate Token
    TokenService-->>AuthBehavior: Token Valid ✓
    AuthBehavior->>AuthBehavior: Check Permissions
    AuthBehavior-->>Endpoint: Authorized ✓
    Endpoint->>Handler: Execute Command
    Handler-->>Endpoint: Result
    Endpoint-->>Client: 200 OK
    
    Note over Client,Handler: If unauthorized, return 401 immediately
```

## 🔄 Batch Processing Flow

```mermaid
graph TD
    A[Batch Command<br/>100 items] --> B[Split into chunks<br/>10 items each]
    B --> C[Process Chunk 1]
    B --> D[Process Chunk 2]
    B --> E[Process Chunk N]
    C --> F[Collect Results]
    D --> F
    E --> F
    F --> G{All Success?}
    G -->|Yes| H[Return Batch Result]
    G -->|No| I[Partial Success<br/>+ Error Details]
```

## 📊 Complete Request Lifecycle

```mermaid
flowchart TB
    subgraph "HTTP Layer"
        A[HTTP Request] --> B[Route Resolution]
        B --> C[Model Binding]
    end
    
    subgraph "CRISP Pipeline"
        C --> D[Create Command/Query]
        D --> E[Pipeline Behaviors]
        E --> F[Handler]
        F --> G[Result]
    end
    
    subgraph "Pipeline Behaviors"
        E1[Logging] --> E2[Validation]
        E2 --> E3[Authorization]
        E3 --> E4[Caching]
        E4 --> E5[Transaction]
        E5 --> E6[Retry]
    end
    
    subgraph "Handler Processing"
        F1[Business Rules] --> F2[Data Access]
        F2 --> F3[External Services]
        F3 --> F4[Domain Events]
    end
    
    subgraph "Response"
        G --> H[Status Code]
        H --> I[Response Body]
        I --> J[HTTP Response]
    end
    
    E -.-> E1
    F -.-> F1
```

## 🎯 Key Insights

1. **Linear Flow** - Requests follow a predictable path
2. **Pipeline Control** - Behaviors wrap around handlers
3. **Early Exit** - Validation/auth failures stop processing
4. **Async Throughout** - Non-blocking at every step
5. **Event Driven** - Loose coupling between features

## 💡 Common Patterns

### Fast Path (Cached Query)
```
Request → Cache Hit → Response (< 5ms)
```

### Slow Path (Complex Command)
```
Request → Validation → Auth → Transaction → Handler → DB → Events → Response (50-200ms)
```

### Error Path
```
Request → Validation Fail → 400 Response (< 10ms)
```

This visual guide shows how CRISP maintains simplicity while handling complex scenarios through its pipeline architecture.