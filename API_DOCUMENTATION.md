# API Documentation

Complete reference for all REST API endpoints in the System Monitoring Platform.

## Base URL

```
http://localhost:5000/api
```

## Authentication

Currently, the API does not require authentication. In production, you would add JWT authentication.

## Response Format

All responses follow this structure:

**Success Response:**
```json
{
  "data": { ... },
  "statusCode": 200
}
```

**Error Response:**
```json
{
  "statusCode": 400,
  "message": "Error description",
  "detailed": "Detailed error message"
}
```

---

## Metrics API

### Create Metric

Creates a new system metric.

**Endpoint:** `POST /api/metrics`

**Request Body:**
```json
{
  "metricName": "ResponseTime",
  "value": 150.5,
  "unit": "ms",
  "source": "WebServer01",
  "tags": "production,api"
}
```

**Field Validations:**
- `metricName`: Required, 1-100 characters
- `value`: Required, numeric, range: -1e10 to 1e10
- `unit`: Required, 1-50 characters
- `source`: Required, 1-100 characters
- `tags`: Optional, max 500 characters

**Success Response:** `201 Created`
```json
{
  "id": 1,
  "metricName": "ResponseTime",
  "value": 150.5,
  "unit": "ms",
  "timestamp": "2024-12-31T12:00:00Z",
  "source": "WebServer01",
  "tags": "production,api"
}
```

**Error Response:** `400 Bad Request`
```json
{
  "statusCode": 400,
  "message": "Validation failed",
  "errors": {
    "metricName": ["Metric name is required"]
  }
}
```

**Example cURL:**
```bash
curl -X POST http://localhost:5000/api/metrics \
  -H "Content-Type: application/json" \
  -d '{
    "metricName": "ResponseTime",
    "value": 150.5,
    "unit": "ms",
    "source": "WebServer01"
  }'
```

---

### Get Metrics

Retrieves metrics with optional filtering and pagination.

**Endpoint:** `GET /api/metrics`

**Query Parameters:**
- `metricName` (optional): Filter by metric name (partial match)
- `source` (optional): Filter by source (partial match)
- `startDate` (optional): Filter by start date (ISO 8601)
- `endDate` (optional): Filter by end date (ISO 8601)
- `pageNumber` (optional): Page number (default: 1)
- `pageSize` (optional): Items per page (default: 50)

**Success Response:** `200 OK`
```json
[
  {
    "id": 1,
    "metricName": "ResponseTime",
    "value": 150.5,
    "unit": "ms",
    "timestamp": "2024-12-31T12:00:00Z",
    "source": "WebServer01",
    "tags": "production,api"
  },
  {
    "id": 2,
    "metricName": "ResponseTime",
    "value": 145.2,
    "unit": "ms",
    "timestamp": "2024-12-31T12:01:00Z",
    "source": "WebServer01",
    "tags": "production,api"
  }
]
```

**Example Requests:**

Get all metrics:
```bash
curl http://localhost:5000/api/metrics
```

Filter by metric name:
```bash
curl "http://localhost:5000/api/metrics?metricName=ResponseTime"
```

Filter by date range:
```bash
curl "http://localhost:5000/api/metrics?startDate=2024-12-01T00:00:00Z&endDate=2024-12-31T23:59:59Z"
```

With pagination:
```bash
curl "http://localhost:5000/api/metrics?pageNumber=1&pageSize=20"
```

---

### Get Metric by ID

Retrieves a specific metric by its ID.

**Endpoint:** `GET /api/metrics/{id}`

**Path Parameters:**
- `id`: Metric ID (integer)

**Success Response:** `200 OK`
```json
{
  "id": 1,
  "metricName": "ResponseTime",
  "value": 150.5,
  "unit": "ms",
  "timestamp": "2024-12-31T12:00:00Z",
  "source": "WebServer01",
  "tags": "production,api"
}
```

**Error Response:** `404 Not Found`
```json
{
  "message": "Metric with ID 999 not found"
}
```

**Example:**
```bash
curl http://localhost:5000/api/metrics/1
```

---

### Get Recent Metrics

Retrieves the most recent metrics.

**Endpoint:** `GET /api/metrics/recent`

**Query Parameters:**
- `count` (optional): Number of metrics to return (default: 100)

**Success Response:** `200 OK`
```json
[
  {
    "id": 100,
    "metricName": "ResponseTime",
    "value": 150.5,
    "unit": "ms",
    "timestamp": "2024-12-31T12:00:00Z",
    "source": "WebServer01",
    "tags": null
  }
]
```

**Example:**
```bash
curl "http://localhost:5000/api/metrics/recent?count=10"
```

---

## KPIs API

### Get All KPIs

Retrieves all calculated KPIs, ordered by calculation time (newest first).

**Endpoint:** `GET /api/kpis`

**Success Response:** `200 OK`
```json
[
  {
    "id": 1,
    "kpiName": "Average Response Time",
    "calculatedValue": 152.75,
    "targetValue": 200.0,
    "status": 1,
    "statusText": "OnTarget",
    "calculatedAt": "2024-12-31T12:00:00Z",
    "periodStart": "2024-12-30T12:00:00Z",
    "periodEnd": "2024-12-31T12:00:00Z",
    "description": "Average response time over the last 24 hours. Target: 200ms",
    "percentageOfTarget": 76.38
  }
]
```

**KPI Status Values:**
- `0`: BelowTarget (Warning)
- `1`: OnTarget (Good)
- `2`: AboveTarget (Excellent)
- `3`: Critical (Needs attention)

**Example:**
```bash
curl http://localhost:5000/api/kpis
```

---

### Get KPIs by Date Range

Retrieves KPIs calculated within a specific date range.

**Endpoint:** `GET /api/kpis/date-range`

**Query Parameters:**
- `startDate` (required): Start date (ISO 8601)
- `endDate` (required): End date (ISO 8601)

**Success Response:** `200 OK`
```json
[
  {
    "id": 1,
    "kpiName": "Average Response Time",
    "calculatedValue": 152.75,
    "targetValue": 200.0,
    "status": 1,
    "statusText": "OnTarget",
    "calculatedAt": "2024-12-31T12:00:00Z",
    "periodStart": "2024-12-30T12:00:00Z",
    "periodEnd": "2024-12-31T12:00:00Z",
    "description": "Average response time over the last 24 hours. Target: 200ms",
    "percentageOfTarget": 76.38
  }
]
```

**Error Response:** `400 Bad Request`
```json
{
  "message": "Start date must be before end date"
}
```

**Example:**
```bash
curl "http://localhost:5000/api/kpis/date-range?startDate=2024-12-01T00:00:00Z&endDate=2024-12-31T23:59:59Z"
```

---

### Calculate KPIs

Triggers KPI calculation based on metrics from the last 24 hours.

**Endpoint:** `POST /api/kpis/calculate`

**Request Body:** None

**Success Response:** `202 Accepted`
```json
{
  "message": "KPI calculation completed successfully"
}
```

**KPIs Calculated:**
1. **Average Response Time** - Average of all ResponseTime metrics
2. **Throughput** - Requests per hour based on RequestCount metrics
3. **Error Rate** - Percentage of errors based on ErrorCount/RequestCount
4. **System Availability** - Average uptime percentage

**Example:**
```bash
curl -X POST http://localhost:5000/api/kpis/calculate
```

**Note:** You need sufficient metrics in the database for meaningful KPI calculations.

---

## Anomalies API

### Get All Anomalies

Retrieves all detected anomalies, ordered by detection time (newest first).

**Endpoint:** `GET /api/anomalies`

**Success Response:** `200 OK`
```json
[
  {
    "id": 1,
    "metricName": "ResponseTime",
    "detectedValue": 500.0,
    "expectedValue": 150.0,
    "deviation": 233.33,
    "severity": 2,
    "severityText": "High",
    "detectedAt": "2024-12-31T12:00:00Z",
    "isResolved": false,
    "resolvedAt": null,
    "description": "Detected anomaly: value 500.00 deviates 233.33% from expected 150.00"
  }
]
```

**Severity Levels:**
- `0`: Low (Z-score 2.0-2.5)
- `1`: Medium (Z-score 2.5-3.0)
- `2`: High (Z-score 3.0-4.0)
- `3`: Critical (Z-score > 4.0)

**Example:**
```bash
curl http://localhost:5000/api/anomalies
```

---

### Get Unresolved Anomalies

Retrieves only anomalies that haven't been resolved.

**Endpoint:** `GET /api/anomalies/unresolved`

**Success Response:** `200 OK`
```json
[
  {
    "id": 1,
    "metricName": "ResponseTime",
    "detectedValue": 500.0,
    "expectedValue": 150.0,
    "deviation": 233.33,
    "severity": 2,
    "severityText": "High",
    "detectedAt": "2024-12-31T12:00:00Z",
    "isResolved": false,
    "resolvedAt": null,
    "description": "Detected anomaly: value 500.00 deviates 233.33% from expected 150.00"
  }
]
```

**Example:**
```bash
curl http://localhost:5000/api/anomalies/unresolved
```

---

### Detect Anomalies

Triggers anomaly detection on recent metrics using statistical analysis.

**Endpoint:** `POST /api/anomalies/detect`

**Request Body:** None

**Success Response:** `202 Accepted`
```json
{
  "message": "Anomaly detection completed successfully"
}
```

**Detection Algorithm:**
1. Groups metrics by metric name
2. Calculates mean and standard deviation for each group
3. Computes Z-score for latest value
4. If Z-score > 2.0, creates an anomaly record
5. Assigns severity based on Z-score magnitude
6. Prevents duplicate anomalies within 30 minutes

**Example:**
```bash
curl -X POST http://localhost:5000/api/anomalies/detect
```

---

### Resolve Anomaly

Marks an anomaly as resolved.

**Endpoint:** `PATCH /api/anomalies/{id}/resolve`

**Path Parameters:**
- `id`: Anomaly ID (integer)

**Request Body:** None

**Success Response:** `200 OK`
```json
{
  "message": "Anomaly resolved successfully"
}
```

**Error Response:** `404 Not Found`
```json
{
  "message": "Anomaly with ID 999 not found or already resolved"
}
```

**Example:**
```bash
curl -X PATCH http://localhost:5000/api/anomalies/1/resolve
```

---

## Common HTTP Status Codes

| Code | Meaning | When Used |
|------|---------|-----------|
| 200 | OK | Successful GET, PATCH requests |
| 201 | Created | Successful POST creating a resource |
| 202 | Accepted | Request accepted for processing |
| 400 | Bad Request | Validation errors, invalid input |
| 404 | Not Found | Resource doesn't exist |
| 500 | Internal Server Error | Unexpected server error |

---

## Error Handling

All errors return a consistent format:

```json
{
  "statusCode": 400,
  "message": "User-friendly error message",
  "detailed": "Technical error details"
}
```

**Common Errors:**

**Validation Error:**
```json
{
  "statusCode": 400,
  "message": "Validation failed",
  "errors": {
    "metricName": ["Metric name is required"],
    "value": ["Value must be within valid range"]
  }
}
```

**Not Found:**
```json
{
  "statusCode": 404,
  "message": "Metric with ID 999 not found"
}
```

**Server Error:**
```json
{
  "statusCode": 500,
  "message": "An error occurred while processing your request",
  "detailed": "Database connection failed"
}
```

---

## Testing with Postman

### Import Collection

Create a Postman collection with these requests:

1. **Create Metric** - POST with JSON body
2. **Get All Metrics** - GET with query parameters
3. **Calculate KPIs** - POST
4. **Get KPIs** - GET
5. **Detect Anomalies** - POST
6. **Get Anomalies** - GET

### Environment Variables

Set up Postman environment:
```json
{
  "baseUrl": "http://localhost:5000/api"
}
```

Use `{{baseUrl}}` in requests.

---

## Rate Limiting

Currently, there is no rate limiting. In production, you would implement:
- Rate limiting middleware
- API key authentication
- Request throttling

---

## Versioning

Current version: `v1` (implicit)

Future versions would use URL versioning:
```
/api/v2/metrics
```

Or header versioning:
```
Accept: application/vnd.monitoring.v2+json
```

---

## Additional Resources

- **Swagger UI**: `http://localhost:5000/swagger` - Interactive API documentation
- **OpenAPI Spec**: `http://localhost:5000/swagger/v1/swagger.json` - Machine-readable API spec
