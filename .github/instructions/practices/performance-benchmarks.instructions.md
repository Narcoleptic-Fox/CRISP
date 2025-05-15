---
applyTo: "**/*performance*.md,**/*benchmark*.md,**/*perf-test*.*"
---

# Performance Benchmarks Guidelines

## Overview

This document establishes standards for performance benchmarking, testing, and optimization across projects. Following these guidelines ensures consistent performance measurement, objective targets, and effective optimization strategies.

## General Principles

- **Measure First**: Base optimization decisions on data, not assumptions
- **Realistic Conditions**: Test under conditions that simulate real usage
- **Consistent Environment**: Use controlled environments for reliable comparisons
- **User-Centric**: Focus on metrics that impact user experience
- **Prioritization**: Optimize for the most impactful performance bottlenecks
- **Regular Testing**: Continuously monitor performance over time

## Key Performance Metrics

### Web Applications

| Metric | Target | Critical Threshold |
|--------|--------|-------------------|
| First Contentful Paint (FCP) | < 1.8s | < 3.0s |
| Largest Contentful Paint (LCP) | < 2.5s | < 4.0s |
| First Input Delay (FID) | < 100ms | < 300ms |
| Cumulative Layout Shift (CLS) | < 0.1 | < 0.25 |
| Time to Interactive (TTI) | < 3.5s | < 7.3s |
| Total Bundle Size | < 250KB (gzipped) | < 500KB (gzipped) |
| Server Response Time | < 100ms | < 600ms |

### Mobile Applications

| Metric | Target | Critical Threshold |
|--------|--------|-------------------|
| Cold Start Time | < 2.0s | < 5.0s |
| Warm Start Time | < 1.0s | < 3.0s |
| UI Thread Jank | < 5% | < 10% |
| Memory Usage | < 150MB | < 300MB |
| Battery Impact | < 1% per hour | < 5% per hour |
| API Response Processing | < 100ms | < 600ms |

### APIs and Backend Services

| Metric | Target | Critical Threshold |
|--------|--------|-------------------|
| Response Time (p95) | < 200ms | < 1000ms |
| Throughput | > 100 req/s | > 10 req/s |
| Error Rate | < 0.1% | < 1% |
| CPU Usage | < 50% | < 85% |
| Memory Usage | < 70% | < 90% |
| DB Query Time (p95) | < 50ms | < 300ms |
| Connection Pool Utilization | < 70% | < 90% |

### Databases

| Metric | Target | Critical Threshold |
|--------|--------|-------------------|
| Query Execution (p95) | < 50ms | < 300ms |
| Index Hit Ratio | > 95% | > 80% |
| Table Scan Rate | < 1% of queries | < 5% of queries |
| Lock Wait Time | < 10ms avg | < 100ms avg |
| Connection Utilization | < 70% | < 90% |
| Buffer Cache Hit Ratio | > 99% | > 95% |
| Disk I/O Utilization | < 50% | < 80% |

## Performance Testing Methods

### Load Testing

- **Purpose**: Evaluate system behavior under expected load
- **Tool Examples**: Apache JMeter, k6, Gatling, Locust
- **Key Metrics**: Response time, throughput, error rate
- **Procedure**:
  1. Define expected concurrent users
  2. Create realistic user scenarios
  3. Include think time between actions
  4. Run test for at least 30 minutes
  5. Collect and analyze metrics

Example JMeter Test Plan Structure:
```
- Test Plan
  - Thread Group (Users)
    - HTTP Cookie Manager
    - HTTP Header Manager
    - HTTP Request Defaults
    - Login Request
    - Browse Products Request
    - Add to Cart Request
    - Checkout Request
    - Response Assertions
    - Duration Assertions
  - Summary Report
  - Aggregate Report
  - Graph Results
```

### Stress Testing

- **Purpose**: Find breaking points and failure modes
- **Tool Examples**: Apache JMeter, Gatling, LoadRunner
- **Key Metrics**: Max throughput, error rate, recovery time
- **Procedure**:
  1. Start with moderate load
  2. Incrementally increase until failure
  3. Document failure points and behavior
  4. Test recovery after overload
  5. Analyze system logs during failure

### Endurance Testing

- **Purpose**: Verify system stability over time
- **Tool Examples**: Apache JMeter, Gatling, Load Impact
- **Key Metrics**: Memory usage over time, response time stability
- **Procedure**:
  1. Configure moderate load (50-70% capacity)
  2. Run for extended period (8+ hours)
  3. Monitor for memory leaks
  4. Check for performance degradation
  5. Analyze garbage collection patterns

### Spike Testing

- **Purpose**: Test system behavior during sudden load increases
- **Tool Examples**: Apache JMeter, Artillery, k6
- **Key Metrics**: Recovery time, error rate during spikes
- **Procedure**:
  1. Start with baseline load
  2. Introduce sudden user increase (5-10x)
  3. Measure response during spike
  4. Evaluate recovery after spike
  5. Test autoscaling capabilities

## Browser Performance Testing

- **Purpose**: Evaluate frontend performance metrics
- **Tool Examples**: Lighthouse, WebPageTest, Chrome DevTools
- **Key Metrics**: FCP, LCP, TTI, CLS
- **Procedure**:
  1. Test on multiple browsers and devices
  2. Use throttled networks (3G/4G)
  3. Clear cache between tests
  4. Run tests multiple times for consistency
  5. Compare against competitors/benchmarks

## Mobile Performance Testing

- **Purpose**: Evaluate mobile app performance
- **Tool Examples**: Android Profiler, Xcode Instruments
- **Key Metrics**: Launch time, memory usage, UI responsiveness
- **Procedure**:
  1. Test on representative device range
  2. Include low-end devices
  3. Measure cold and warm starts
  4. Check transitions and animations
  5. Monitor battery consumption

## Performance Budget Implementation

Implement performance budgets in CI/CD pipelines:

```yaml
# Example GitHub Actions workflow for performance budget
name: Performance Budget

on:
  pull_request:
    branches: [ main ]

jobs:
  performance:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Set up environment
        uses: actions/setup-node@v3
        with:
          node-version: '16'
      - name: Install dependencies
        run: npm ci
      - name: Build
        run: npm run build
      - name: Check bundle size
        uses: siddharthkp/bundlesize@v3
        with:
          files: [
            {
              "path": "build/static/js/*.js",
              "maxSize": "170 kB"
            },
            {
              "path": "build/static/css/*.css",
              "maxSize": "20 kB"
            }
          ]
      - name: Run Lighthouse CI
        uses: treosh/lighthouse-ci-action@v9
        with:
          urls: |
            http://localhost:3000
          budgetPath: ./budget.json
          uploadArtifacts: true
```

Example budget.json:
```json
[
  {
    "path": "/*",
    "timings": [
      {
        "metric": "interactive",
        "budget": 3000
      },
      {
        "metric": "first-contentful-paint",
        "budget": 1800
      }
    ],
    "resourceSizes": [
      {
        "resourceType": "script",
        "budget": 170
      },
      {
        "resourceType": "total",
        "budget": 300
      }
    ]
  }
]
```

## Common Performance Optimizations

### Frontend Optimizations

1. **Asset Optimization**
   - Compress images using WebP format
   - Lazy load off-screen images
   - Minify CSS and JavaScript
   - Use code splitting for large applications
   - Implement tree shaking

2. **Caching Strategy**
   - Set appropriate cache headers
   - Implement service workers
   - Use cache-busting techniques
   - Leverage browser caching

3. **Rendering Optimization**
   - Minimize DOM size (< 1500 nodes)
   - Reduce CSS complexity
   - Avoid layout thrashing
   - Use CSS containment
   - Implement virtualization for long lists

### Backend Optimizations

1. **Database Optimization**
   - Index frequently queried columns
   - Denormalize for read-heavy operations
   - Use query caching
   - Optimize joins and complex queries
   - Implement connection pooling

2. **API Optimization**
   - Implement response compression
   - Use pagination for large datasets
   - Cache frequent API responses
   - Reduce payload size
   - Use GraphQL to prevent over-fetching

3. **Server Optimization**
   - Enable HTTP/2 or HTTP/3
   - Implement CDN for static assets
   - Configure proper load balancing
   - Use in-memory caching
   - Optimize server configurations

## Performance Testing Environments

### Development Environment

- Conduct baseline performance tests
- Profile code during development
- Use browser developer tools
- Run unit performance tests

### Staging Environment

- Must match production configuration
- Run complete performance test suites
- Test with production-like data volume
- Validate against performance budgets
- Conduct A/B performance comparisons

### Production Environment

- Implement real user monitoring (RUM)
- Set up synthetic monitoring
- Capture performance telemetry
- Monitor performance trends over time
- Set alerts for performance regressions

## Documentation Requirements

Performance test documentation should include:

- Test environment specifications
- Test scenarios and user journeys
- Tool configurations
- Raw results and statistical analysis
- Comparison to previous baselines
- Identified bottlenecks
- Optimization recommendations
- Performance impact assessment

## Specialized Performance Considerations

### Single Page Applications (SPAs)

- Implement code splitting
- Optimize initial bundle size
- Use server-side rendering or pre-rendering
- Implement efficient state management
- Cache API responses

### E-commerce

- Optimize for conversion-critical paths
- Prioritize product image loading
- Optimize checkout flow performance
- Implement predictive loading
- Optimize search performance

### Content-heavy Sites

- Implement responsive images
- Use content delivery networks
- Optimize web fonts loading
- Implement pagination or infinite scroll
- Use AMP where appropriate

## Performance Monitoring Tools

- **Browser-based**: Lighthouse, WebPageTest
- **RUM**: Google Analytics, New Relic, Datadog
- **Synthetic**: Pingdom, Uptrends, Dynatrace
- **APM**: New Relic, AppDynamics, Elastic APM
- **Infrastructure**: Prometheus, Grafana, CloudWatch
