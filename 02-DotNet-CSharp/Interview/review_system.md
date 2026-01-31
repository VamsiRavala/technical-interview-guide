```mermaid
graph TD
    subgraph Client
        A1[Browser / Frontend App]
    end

    subgraph WebAPI
        A2[Controllers]
        A3[Services / Business Logic]
        A4[Repositories]
        A5[Auth Middleware]
    end

    subgraph Infrastructure
        A6[EF Core DbContext]
        A7[SQL Database]
    end

    subgraph Auth
        A8[JWT Token]
        A9[Claims: user_type, role]
    end

    subgraph Entities
        B1[User]
        B2[Role]
        B3[SubjectiveReview]
        B4[ObjectiveReview]
        B5[MetricReview]
        B6[Metric]
        B7[MetricWeightage]
        B8[EntityType]
        B9[SubObjWeightage]
    end

    %% Flow
    A1 --> A5
    A5 --> A2
    A2 --> A3
    A3 --> A4
    A4 --> A6
    A6 --> A7

    A5 --> A8
    A8 --> A9

    A6 --> B1
    A6 --> B2
    A6 --> B3
    A6 --> B4
    A6 --> B5
    A6 --> B6
    A6 --> B7
    A6 --> B8
    A6 --> B9

    A3 -->|Apply business rules| A6
