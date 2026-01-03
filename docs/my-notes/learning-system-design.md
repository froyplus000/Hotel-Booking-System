# Learning System Design

- [Learning System Design](#learning-system-design)
  - [1. Clean / Layered Architecture](#1-clean--layered-architecture)
    - [What is Clean Architecture?](#what-is-clean-architecture)
    - [Uncle Bob's Clean Architecture Diagram](#uncle-bobs-clean-architecture-diagram)
      - [Entity](#entity)
      - [Use Case](#use-case)
      - [Controller](#controller)
      - [Infrastructure](#infrastructure)
      - [Outermost](#outermost)
  - [References](#references)

## 1. Clean / Layered Architecture

### What is Clean Architecture?

- Structuring your code in the form of layers
  - Each layer having its specific purpose (Single Responsibility Principle or SRP)
- Dependencies move inward
  - Meaning that the outer layers are dependent on the layer directly below them
  - But the inner layers do not depend on any of the outer layers
  - Therefore, the innermost layer does not depend on anything
  - Analogy: **Think of it like an onion.🧅 With outermost layers that can easily be peeled off, without damaging any of its insides.**
  - Outermost layers are the ones that change most frequently, but since none of the inner layers depends on it, they can easily be swapped out and replaced

### Uncle Bob's Clean Architecture Diagram

![Uncle Bob’s Clean Architecture diagram](https://miro.medium.com/v2/resize:fit:828/format:webp/0*0a3SRm_70D1R8ela.jpg)

> Arrows representing dependencies

#### Entity

- The lowest level of abstraction
- Backbone of the entire system
- Does not depend on any other layer
- Contains entities that are the fundamental building blocks of the application

#### Use Case

- Important business logic of the application
- Like Chef who takes raw ingredients and creates a delicious meal

#### Controller

- Responsible for managing the flow of incoming requests and directing them to the appropriate destination
- Need to be quick and efficient
- Making sure that each request is properly validated and passed on to the right place
- Also responsible for adapting the incoming request data, parsing and validating for further use
- E.g. invalid format incoming data, then return an error response to client

#### Infrastructure

- Responsible for storing and retrieving data from the database

#### Outermost

- Typically consists of the UI
- So for a back-end developer, the controller is the outermost implementation
- The UI built by the front-end developer is the outermost in most cases

## References

[1. Clean Architecture by Rudraksh](https://medium.com/@rudrakshnanavaty/clean-architecture-7c1b3b4cb181)
