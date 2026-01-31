# 🔄 Communication Patterns in Automotive: REST, gRPC, WebSocket, MQTT, Kafka

This document compares **REST, gRPC, WebSocket, MQTT, and Kafka** and
shows where they are applied in the **automotive ecosystem**.

------------------------------------------------------------------------

## 1. REST (HTTP/1.1, JSON/XML)

-   **Style:** Request/Response (stateless).\
-   **Best for:** CRUD operations, public APIs.\
-   **Pros:** Simple, human-readable, widely supported.\
-   **Cons:** Verbose (JSON/XML), no streaming, higher latency.

**🚗 Example:**\
- Mobile app → Dealer system: *"Get my last 5 service appointments"*.\
- Dealer CRM → OEM backend: *"Fetch warranty details by VIN"*.

------------------------------------------------------------------------

## 2. gRPC (HTTP/2, Protobuf)

-   **Style:** Strongly-typed RPC, supports streaming.\
-   **Best for:** Microservices, inter-service APIs.\
-   **Pros:** Fast (binary Protobuf), streaming support,
    cross-language.\
-   **Cons:** Not human-readable, browser clients need gRPC-Web.

**🚗 Example:**\
- Infotainment → Navigation service: *fetch traffic-aware routes*.\
- Backend services → Diagnostics, ticketing, vector DB.\
- Autonomous stack: Perception → Planning modules.

------------------------------------------------------------------------

## 3. WebSockets

-   **Style:** Persistent, full-duplex channel.\
-   **Best for:** Real-time push updates.\
-   **Pros:** Low latency, event-driven.\
-   **Cons:** Connection management overhead.

**🚗 Example:**\
- Dealer dashboard → Live complaint status updates.\
- EV charging → Live kWh delivered & cost updates.\
- Car → Mobile app: *"Battery at 80%"*.

------------------------------------------------------------------------

## 4. MQTT (IoT Protocol)

-   **Style:** Publish/Subscribe, lightweight.\
-   **Best for:** IoT telemetry, unstable networks.\
-   **Pros:** Very lightweight, reliable.\
-   **Cons:** Not optimized for heavy backend processing.

**🚗 Example:**\
- Car telemetry → Cloud (speed, GPS, tire pressure).\
- EV charging station → Backend usage updates.

------------------------------------------------------------------------

## 5. Kafka / Event Streams

-   **Style:** Distributed log + event streaming.\
-   **Best for:** Large-scale data pipelines.\
-   **Pros:** Scalable, fault-tolerant, event replay.\
-   **Cons:** Heavy infra, not for direct car communication.

**🚗 Example:**\
- Fleet analytics: thousands of cars streaming data.\
- Predictive maintenance: analyzing telemetry in real time.

------------------------------------------------------------------------

## ⚖️ When to Use What

  -------------------------------------------------------------------------
  Use Case                    Best Tech                      Why
  --------------------------- ------------------------------ --------------
  CRUD ops (dealer ↔ backend) REST                           Simple,
                                                             standard

  Microservices (car or       gRPC                           Strong typing,
  backend)                                                   fast

  Real-time updates           WebSocket                      Persistent
  (charging, dashboards)                                     channel

  Telemetry (car ↔ cloud)     MQTT                           Lightweight,
                                                             reliable

  Fleet analytics             Kafka                          Event
                                                             streaming &
                                                             replay
  -------------------------------------------------------------------------

------------------------------------------------------------------------

## 🏎️ Automotive Scenarios

1.  **Complaint Handling (Dealer CRM)**
    -   Complaint creation → REST\
    -   Complaint analysis → gRPC (between NLP/vector DB/ticketing)\
    -   Complaint status → WebSocket (real-time updates)
2.  **Connected Car (EV Telemetry)**
    -   Car → Cloud → MQTT\
    -   Cloud → App queries → REST\
    -   Charging progress → WebSocket
3.  **Autonomous Driving**
    -   Sensor → Perception → gRPC\
    -   Map updates → REST\
    -   OTA software update progress → WebSocket

------------------------------------------------------------------------

## 📊 Ecosystem Diagram

``` mermaid
flowchart TD
    A[🚗 Car] -->|Telemetry| B[(MQTT Broker)]
    A -->|Sensor Data| C[gRPC Microservices in Car]
    B --> D[☁️ Cloud Backend]
    C --> D
    D -->|CRUD Ops| E[REST APIs - Dealer/CRM]
    D -->|Streaming Data| F[(Kafka Cluster)]
    D -->|Live Updates| G[📱 Mobile App via WebSocket]
    E --> H[Dealer Dashboard]
    G --> H
```

------------------------------------------------------------------------

## ✅ Summary

-   **REST** → CRUD + external APIs.\
-   **gRPC** → High-performance inter-service comms.\
-   **WebSocket** → Real-time push.\
-   **MQTT** → Lightweight IoT telemetry.\
-   **Kafka** → Fleet-scale event analytics.
