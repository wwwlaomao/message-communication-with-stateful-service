# Message Communication with Stateful Service

This project showcases different approaches of message communication between a stateless service and some instances of a stateful service.

The following diagram shows the relationship among the different components in this demonstration. The `Client` and `External Parties` are not included in the docker compose file.

```mermaid
flowchart LR
  subgraph Stateful Service
    direction TB
        instance1[Instance 1]
        instance2[Instance 2]
  end
  subgraph test[Test Service]
    direction TB
        test1[Instance]
  end
  event-bus[Event Bus]
  Client -- web api --- test1
  test1 -. message .-> event-bus
  event-bus -. message .-> instance1
  event-bus -. message .-> instance2
  instance1 <== web socket ==> externalA[External Party A]
  instance2 <== web socket ==> externalB[External Party B]
  instance2 <== web socket ==> externalC[External Party C]
```
