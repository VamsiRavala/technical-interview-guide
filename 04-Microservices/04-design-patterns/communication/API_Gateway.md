# API Gateway: Features, Flexibility, Security, and Authentication

## 1. What is an API Gateway?
An **API Gateway** is a **single entry point** between clients (web, mobile, IoT) and backend services.  
It acts as a **reverse proxy plus a policy enforcement layer**, providing **routing, security, monitoring, and transformation**.  

Instead of clients talking directly to many services, they talk to **one gateway**, which then calls the right service(s).

---

## 2. Key Features
1. **Request Routing** → Send requests to the right backend service.  
2. **Load Balancing** → Distribute requests across service instances.  
3. **Protocol Translation** → Convert HTTP ↔ gRPC, WebSocket, GraphQL, etc.  
4. **Response Aggregation** → Combine data from multiple services into one response.  
5. **Transformation** → Rewrite headers, URLs, or payloads.  
6. **Caching** → Reduce latency by serving cached responses.  
7. **Rate Limiting & Throttling** → Control traffic volume.  
8. **Monitoring & Logging** → Collect metrics, traces, and logs.  

---

## 3. Flexibility
- **Abstraction**: Backend services can evolve independently without breaking clients.  
- **Client-specific APIs**: Mobile, web, and IoT can get optimized endpoints.  
- **Centralized policies**: Security, logging, and quotas managed in one place.  
- **Composable APIs**: Gateway can orchestrate multiple service calls into a single API.  

**Example:**  
`/dashboard` endpoint can internally call `UserService`, `OrderService`, and `NotificationService` to return one combined response.

---

## 4. DDoS / DoS Protection
API Gateways defend against **Denial-of-Service attacks** using:  
- **Rate Limiting** – cap requests per second per client/IP.  
- **Throttling** – slow down requests beyond a limit.  
- **Quota Enforcement** – enforce daily/monthly limits.  
- **Spike Arrest** – prevent sudden bursts.  
- **Circuit Breakers** – stop calls to failing services to prevent cascading failures.  
- **WAF (Web Application Firewall)** integration – block malicious requests.  

Cloud API gateways (AWS, Azure, GCP) usually include **built-in DDoS protection services**.

---

## 5. Authentication & Authorization
API Gateways are the **first checkpoint** for security:  

### Authentication (Who are you?)  
- API Keys  
- OAuth2 / OpenID Connect  
- JWT validation  
- SSO integration (Okta, Azure AD, Google Identity)  

### Authorization (What can you do?)  
- Role-based (RBAC) or Attribute-based (ABAC) access control.  
- Example: Only `Admin` role can access `/admin/*` routes.  

### Token Handling  
- Validate tokens once at the gateway instead of every microservice.  
- Pass validated claims (user ID, roles) downstream.  
- Manage refresh/revoke with Identity Providers.  

**Flow Example:**  
1. Client sends request with JWT token.  
2. API Gateway validates token with Identity Provider.  
3. If valid → forward request with claims to service.  
4. If invalid → return 401 Unauthorized.  

---

## ✅ Summary
- **API Gateway** = Smart reverse proxy + central policy layer.  
- **Features** = Routing, caching, transformation, monitoring.  
- **Flexibility** = Abstract services, optimize per client, aggregate APIs.  
- **DDoS/DoS** = Defended by throttling, rate limits, quotas, WAFs, and circuit breakers.  
- **Authentication** = Centralized validation of API keys, JWT, OAuth2; authorization via RBAC/ABAC.  
