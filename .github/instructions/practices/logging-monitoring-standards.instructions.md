---
applyTo: "**/*logging*.md,**/*monitoring*.md,**/*observability*.md"
---

# Logging and Monitoring Standards

## Overview

This document establishes standards for implementing comprehensive logging, monitoring, and observability practices across all projects. Following these guidelines enables effective troubleshooting, performance analysis, and system health management.

## General Principles

- **Purposeful Logging**: Log information that has business or technical value
- **Consistency**: Use consistent formats and patterns across all systems
- **Correlation**: Ensure logs can be correlated across service boundaries
- **Security**: Never log sensitive data or credentials
- **Performance**: Optimize logging to minimize system impact
- **Accessibility**: Make logs easily searchable and analyzable

## Logging Standards

### Log Levels

Use appropriate log levels consistently:

| Level | When to Use |
|-------|-------------|
| FATAL | The application cannot continue and will terminate immediately |
| ERROR | An error occurred that prevents a function from completing |
| WARN | A potential issue that doesn't prevent the application from working |
| INFO | Important business events or major system state changes |
| DEBUG | Detailed information useful for debugging |
| TRACE | Extremely detailed information (method entry/exit, variable values) |

### Log Content Guidelines

Each log entry should include:

- **Timestamp**: ISO 8601 format with timezone (e.g., `2023-05-15T14:30:24.123Z`)
- **Log Level**: Clearly indicated level (ERROR, INFO, etc.)
- **Service/Component**: Name of the service or component generating the log
- **Correlation ID**: Request ID or trace ID for distributed tracing
- **Message**: Clear, concise description of what happened
- **Context**: Relevant data needed to understand the event
- **Exception Details**: For errors, include stack traces and error codes

Example structured log entry (JSON format):
```json
{
  "timestamp": "2023-05-15T14:30:24.123Z",
  "level": "ERROR",
  "service": "payment-service",
  "correlationId": "req-abc-123",
  "message": "Payment processing failed",
  "context": {
    "orderId": "order-456",
    "paymentProvider": "stripe",
    "amount": 99.99,
    "currency": "USD"
  },
  "exception": {
    "class": "PaymentProcessingException",
    "message": "API returned error code 4002",
    "stackTrace": "..."
  }
}
```

### What to Log

#### Always Log
- Application startup and shutdown
- Authentication and authorization events (successes and failures)
- Data validation failures
- API call failures
- Database operation failures
- Critical business events (orders placed, payments processed)
- Configuration changes
- Performance issues (timeouts, slow responses)
- Security-related events

#### Log with Caution
- High-volume operational data (consider sampling)
- Detailed debugging information in production
- Large payloads (truncate or reference)

#### Never Log
- Passwords or authentication tokens
- Credit card numbers or financial details
- Personal identifiable information (PII) without proper masking
- Encryption keys or secrets
- Session tokens

### Logging Implementation

#### Structured Logging

Prefer structured logging over plain text:

```java
// Java example with SLF4J and Logback
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import net.logstash.logback.argument.StructuredArguments;

public class PaymentService {
    private static final Logger logger = LoggerFactory.getLogger(PaymentService.class);
    
    public void processPayment(Order order, PaymentDetails payment) {
        logger.info("Processing payment", 
            StructuredArguments.keyValue("orderId", order.getId()),
            StructuredArguments.keyValue("amount", payment.getAmount()),
            StructuredArguments.keyValue("currency", payment.getCurrency())
        );
        
        try {
            // Payment processing logic
        } catch (PaymentException e) {
            logger.error("Payment processing failed", 
                StructuredArguments.keyValue("orderId", order.getId()),
                StructuredArguments.keyValue("errorCode", e.getErrorCode()),
                e
            );
            throw e;
        }
    }
}
```

```javascript
// JavaScript example with Winston
const winston = require('winston');
const logger = winston.createLogger({
  format: winston.format.json(),
  transports: [new winston.transports.Console()]
});

function processPayment(order, payment) {
  logger.info('Processing payment', {
    orderId: order.id,
    amount: payment.amount,
    currency: payment.currency
  });
  
  try {
    // Payment processing logic
  } catch (error) {
    logger.error('Payment processing failed', {
      orderId: order.id,
      errorCode: error.code,
      errorMessage: error.message,
      stack: error.stack
    });
    throw error;
  }
}
```

#### Log Aggregation

- Centralize logs from all services and components
- Use standardized log formats (JSON preferred)
- Implement log rotation strategies for file-based logs
- Include deployment environment information
- Use buffering to handle logging spikes

### Logging Configuration

- Make log levels configurable at runtime
- Use environment-specific logging configurations
- Consider different log destinations based on environment
- Balance log verbosity with system performance
- Implement log retention policies

Example logging configuration (Logback XML):
```xml
<configuration>
  <appender name="CONSOLE" class="ch.qos.logback.core.ConsoleAppender">
    <encoder class="net.logstash.logback.encoder.LogstashEncoder"/>
  </appender>
  
  <appender name="FILE" class="ch.qos.logback.core.rolling.RollingFileAppender">
    <file>logs/application.log</file>
    <rollingPolicy class="ch.qos.logback.core.rolling.TimeBasedRollingPolicy">
      <fileNamePattern>logs/application.%d{yyyy-MM-dd}.log</fileNamePattern>
      <maxHistory>30</maxHistory>
    </rollingPolicy>
    <encoder class="net.logstash.logback.encoder.LogstashEncoder"/>
  </appender>
  
  <!-- Development environment -->
  <springProfile name="dev">
    <root level="DEBUG">
      <appender-ref ref="CONSOLE"/>
      <appender-ref ref="FILE"/>
    </root>
  </springProfile>
  
  <!-- Production environment -->
  <springProfile name="prod">
    <root level="INFO">
      <appender-ref ref="FILE"/>
    </root>
  </springProfile>
</configuration>
```

## Monitoring Standards

### Key Metrics to Monitor

#### System Metrics
- **CPU Usage**: Overall and per-process utilization
- **Memory Usage**: Total, used, free, cached
- **Disk I/O**: Read/write operations, latency, throughput
- **Network I/O**: Bytes in/out, packet counts, errors
- **File System**: Available space, inodes

#### Application Metrics
- **Request Rate**: Requests per second
- **Response Time**: Average, median, percentiles (p90, p95, p99)
- **Error Rate**: Percentage of failed requests
- **Throughput**: Transaction or operation throughput
- **Concurrency**: Active requests/connections
- **Saturation**: How close the system is to capacity

#### Business Metrics
- **Active Users**: Current users, daily/monthly active users
- **Conversion Rates**: Key business process completion rates
- **Transaction Volume**: Orders, payments, other key business events
- **Feature Usage**: Utilization of specific features
- **Retention**: User retention metrics

#### Database Metrics
- **Query Performance**: Execution time, throughput
- **Connection Pool**: Utilization, wait time, timeouts
- **Cache Hit Rate**: Effectiveness of database caching
- **Index Usage**: Proper utilization of indexes
- **Deadlocks/Locks**: Contention issues

#### Infrastructure Metrics
- **Container Health**: Status, restarts, resource usage
- **Node Status**: Health of cluster nodes
- **Autoscaling**: Scale events, capacity utilization
- **Service Discovery**: Registration/discovery events
- **Load Balancer**: Request distribution, health checks

### Metrics Implementation

#### Naming Conventions

Use consistent metric naming:
- Use lowercase with words separated by underscores or dots
- Include application or service name as prefix
- Be specific and descriptive
- Use consistent units and scales

Examples:
```
payment_service.request.duration_seconds
order_service.orders.processed_total
user_service.active_sessions
api_gateway.request.errors_count
```

#### Tagging/Labeling

Use tags/labels to add dimensions to metrics:
- **environment**: dev, staging, production
- **service**: service or component name
- **instance**: unique instance identifier
- **endpoint**: API endpoint or route
- **status_code**: HTTP status code
- **version**: application version

Example tag usage:
```
http_requests_total{service="payment-api", environment="production", endpoint="/api/v1/payments", status_code="200"}
```

#### Instrumentation Guidelines

- Minimize overhead of metrics collection
- Use client libraries specific to your metrics system
- Instrument all API endpoints and critical paths
- Use histograms for latency measurements
- Use counters for event frequencies
- Implement custom metrics for business processes

Java example using Micrometer:
```java
@RestController
public class OrderController {
    private final MeterRegistry meterRegistry;
    private final OrderService orderService;
    
    public OrderController(MeterRegistry meterRegistry, OrderService orderService) {
        this.meterRegistry = meterRegistry;
        this.orderService = orderService;
    }
    
    @PostMapping("/orders")
    public ResponseEntity<Order> createOrder(@RequestBody OrderRequest request) {
        Timer.Sample sample = Timer.start(meterRegistry);
        
        try {
            Order order = orderService.createOrder(request);
            
            // Record success metrics
            meterRegistry.counter("orders.created.total", 
                "status", "success",
                "type", order.getType()).increment();
                
            return ResponseEntity.ok(order);
        } catch (Exception e) {
            // Record error metrics
            meterRegistry.counter("orders.created.total", 
                "status", "error",
                "error_type", e.getClass().getSimpleName()).increment();
                
            throw e;
        } finally {
            sample.stop(meterRegistry.timer("orders.creation.duration", 
                "endpoint", "/orders"));
        }
    }
}
```

JavaScript example using Prometheus client:
```javascript
const express = require('express');
const promClient = require('prom-client');

const app = express();
const register = promClient.register;

// Create metrics
const httpRequestsTotal = new promClient.Counter({
  name: 'http_requests_total',
  help: 'Total HTTP requests',
  labelNames: ['method', 'route', 'status']
});

const httpRequestDuration = new promClient.Histogram({
  name: 'http_request_duration_seconds',
  help: 'HTTP request duration in seconds',
  labelNames: ['method', 'route', 'status'],
  buckets: [0.1, 0.3, 0.5, 0.7, 1, 3, 5, 10]
});

// Middleware to record metrics
app.use((req, res, next) => {
  const end = httpRequestDuration.startTimer();
  
  res.on('finish', () => {
    const route = req.route ? req.route.path : req.path;
    const labels = { method: req.method, route, status: res.statusCode };
    
    httpRequestsTotal.inc(labels);
    end(labels);
  });
  
  next();
});

// Metrics endpoint
app.get('/metrics', (req, res) => {
  res.set('Content-Type', register.contentType);
  res.end(register.metrics());
});
```

### Alerts and Thresholds

Define alert thresholds for key metrics:

| Metric | Warning Threshold | Critical Threshold | Duration | Action |
|--------|-------------------|-------------------|----------|--------|
| CPU Usage | >75% | >90% | 5 min | Scale up / Investigate |
| Memory Usage | >80% | >95% | 5 min | Check for leaks / Scale up |
| Error Rate | >1% | >5% | 5 min | Immediate investigation |
| Response Time p95 | >500ms | >1s | 5 min | Performance investigation |
| Disk Usage | >80% | >95% | 15 min | Clean up / Add storage |

### Alerting Best Practices

- Alert on symptoms, not causes
- Set thresholds to avoid alert fatigue
- Include clear diagnostic information
- Define alert severity levels
- Establish escalation paths
- Create runbooks for common alerts
- Test alerts before deploying

### Alert Message Format

Clear, actionable alert messages should include:

1. What happened
2. When it happened
3. Where it happened (service, instance)
4. Severity level
5. Current value vs. threshold
6. Link to dashboard or more information
7. Suggested actions or runbook link

Example:
```
[CRITICAL] High Error Rate Detected
Service: payment-service
Instance: payment-service-prod-7d94f9c86-2xjqp
Time: 2023-05-15 14:30:24 UTC
Metric: error_rate = 8.5% (threshold: 5%)
Duration: 5 minutes
Dashboard: https://grafana.example.com/d/abc123
Runbook: https://wiki.example.com/runbooks/high-error-rate
```

## Observability Standards

### Distributed Tracing

- Implement end-to-end request tracing
- Propagate trace context across service boundaries
- Sample traces appropriately based on traffic volume
- Capture relevant attributes on spans
- Tag traces with user or business context when relevant

Example using OpenTelemetry (Java):
```java
@Service
public class OrderService {
    private final Tracer tracer;
    private final PaymentService paymentService;
    
    @Autowired
    public OrderService(Tracer tracer, PaymentService paymentService) {
        this.tracer = tracer;
        this.paymentService = paymentService;
    }
    
    public Order createOrder(OrderRequest orderRequest) {
        Span span = tracer.spanBuilder("createOrder")
            .setAttribute("order.customerId", orderRequest.getCustomerId())
            .setAttribute("order.amount", orderRequest.getAmount())
            .startSpan();
            
        try (Scope scope = span.makeCurrent()) {
            // Create order in database
            Order order = saveOrder(orderRequest);
            
            // Process payment
            Payment payment = paymentService.processPayment(
                order.getId(), orderRequest.getPaymentDetails());
                
            // Update order with payment info
            order.setPaymentId(payment.getId());
            order.setStatus("PAID");
            saveOrder(order);
            
            return order;
        } catch (Exception e) {
            span.recordException(e);
            span.setStatus(StatusCode.ERROR, e.getMessage());
            throw e;
        } finally {
            span.end();
        }
    }
}
```

### Health Checks

Implement standardized health checks:

- **Liveness**: Determines if the application is running
- **Readiness**: Determines if the application can handle requests
- **Dependency Health**: Status of critical dependencies
- **Custom Business Logic**: Application-specific health indicators

Example Spring Boot health check:
```java
@Component
public class DatabaseHealthIndicator implements HealthIndicator {
    private final DataSource dataSource;
    
    @Autowired
    public DatabaseHealthIndicator(DataSource dataSource) {
        this.dataSource = dataSource;
    }
    
    @Override
    public Health health() {
        try (Connection conn = dataSource.getConnection()) {
            PreparedStatement ps = conn.prepareStatement("SELECT 1");
            ResultSet rs = ps.executeQuery();
            
            if (rs.next()) {
                return Health.up()
                    .withDetail("database", "PostgreSQL")
                    .withDetail("version", getDatabaseVersion(conn))
                    .build();
            }
            
            return Health.down()
                .withDetail("error", "Database connectivity test failed")
                .build();
        } catch (SQLException e) {
            return Health.down()
                .withDetail("error", e.getMessage())
                .withException(e)
                .build();
        }
    }
    
    private String getDatabaseVersion(Connection conn) throws SQLException {
        try (PreparedStatement ps = conn.prepareStatement("SELECT version()");
             ResultSet rs = ps.executeQuery()) {
            return rs.next() ? rs.getString(1) : "unknown";
        }
    }
}
```

### Dashboard Standards

Standard dashboard layout:
1. **Service Overview**: Key metrics at a glance
2. **Traffic**: Request volume, patterns
3. **Errors**: Error rates, types
4. **Latency**: Response times, outliers
5. **Saturation**: Resource utilization
6. **Dependencies**: Health of downstream services
7. **Business Metrics**: Domain-specific indicators

Dashboard best practices:
- Consistent layout across services
- Clear titles and axis labels
- Use standard color coding for severity
- Include links to related resources
- Show both short-term and long-term views
- Add annotations for deployments and incidents

## Tooling Recommendations

### Logging Stack

- **Log Collection**: Fluentd, Logstash, Vector
- **Log Storage**: Elasticsearch, Loki
- **Log Analysis**: Kibana, Grafana

### Monitoring Stack

- **Metrics Collection**: Prometheus, Datadog, New Relic
- **Metrics Storage**: Prometheus, Thanos, Cortex
- **Metrics Visualization**: Grafana, Datadog

### Tracing Stack

- **Tracing Collection**: Jaeger, Zipkin, OpenTelemetry
- **Tracing Storage**: Jaeger, Tempo
- **Tracing Visualization**: Jaeger UI, Zipkin UI, Grafana

### Alerting

- **Alert Management**: Alertmanager, PagerDuty, OpsGenie
- **Incident Management**: ServiceNow, Jira

## Implementation Checklist

- [ ] Standardized logging framework and configuration
- [ ] Centralized log collection and storage
- [ ] Structured logging format
- [ ] Log correlation IDs across services
- [ ] Key metrics instrumented across all services
- [ ] Service health endpoints
- [ ] Dashboard templates for each service type
- [ ] Alert definitions with appropriate thresholds
- [ ] Incident response runbooks
- [ ] Distributed tracing implementation
- [ ] Regular review of monitoring coverage
