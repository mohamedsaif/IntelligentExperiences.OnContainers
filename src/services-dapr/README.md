# Running Crowd Analytics with Dapr and KEDA

## What is Dapr

Dapr is a portable, serverless, event-driven runtime that makes it easy for developers to build resilient, stateless and stateful microservices that run on the cloud and edge and embraces the diversity of languages and developer frameworks.

## Why Dapr

All microservices leverages connectivity to many external bindings (like a service bus queue for example). Why should developers spend time importing technology-specific SDK to communicate with a dependent service while you can just use dapr!

Dapr codifies the best practices for building microservice applications into open, independent, building blocks that enable you to build portable applications with the language and framework of your choice. Each building block is independent and you can use one, some, or all of them in your application.

Writing high performance, scalable and reliable distributed application is hard. Dapr brings proven patterns and practices to you. It unifies event-driven and actors semantics into a simple, consistent programming model. It supports all programming languages without framework lock-in. You are not exposed to low-level primitives such as threading, concurrency control, partitioning and scaling. Instead, you can write your code by implementing a simple web server using familiar web frameworks of your choice.

## Dapr CLI Installation

## Dapr Runtime Deployment on Kubernetes

Once you have dapr CLI installed on your dev-machine, you can easily deploy it to the active kubectl context using the following command:

```bash

dapr init --kubernetes

```

>**NOTE:** Installing dapr using the CLI for testing purpouses only. In production and advanced installation, please use Helm. Find out more [dapr installation docs](https://github.com/dapr/docs/blob/master/getting-started/environment-setup.md#installing-dapr-on-a-kubernetes-cluster)

## Services Deployment

Now we need to deploy a our dapr optimized services ([src/services-dapr](src/services-dapr)) to Kubernetes.

