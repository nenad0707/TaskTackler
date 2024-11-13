# TaskTacklerApp

[![Publish TaskTackler Docker Image](https://github.com/nenad0707/TaskTackler/actions/workflows/docker-publish.yml/badge.svg)](https://github.com/nenad0707/TaskTackler/actions/workflows/docker-publish.yml)

# 📋 Task Tackler

![Blazor](https://img.shields.io/badge/Blazor-512BD4?style=flat&logo=blazor&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-512BD4?style=flat&logo=.net&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=flat&logo=docker&logoColor=white)
![GitHub Actions](https://img.shields.io/badge/GitHub%20Actions-2088FF?style=flat&logo=githubactions&logoColor=white)
![Azure](https://img.shields.io/badge/Azure-0078D4?style=flat&logo=microsoftazure&logoColor=white)

**Task Tackler** is a **Blazor WebAssembly** application serving as the frontend for the Task Tackler project. This project demonstrates advanced techniques in client-side caching, authentication, authorization, and pagination.

## 🌐 Live Demo

[![Task Tackler Demo](https://img.shields.io/badge/Task%20Tackler-Live%20Demo-512BD4?style=for-the-badge&logo=microsoft-edge&logoColor=white)](http://your-live-demo-url.com)

> **⚠️ Note:** Registration with a valid email is required to access certain features.

---

## 📖 Table of Contents

- [📋 Task Tackler](#-task-tackler)
  - [🌐 Live Demo](#-live-demo)
  - [📖 Table of Contents](#-table-of-contents)
  - [🚀 Features](#-features)
  - [🛠️ Technologies Used](#️-technologies-used)
  - [📂 Project Structure](#-project-structure)
  - [🧩 Key Components](#-key-components)
  - [🌟 Features in Detail](#-features-in-detail)
  - [🐳 Docker Configuration](#-docker-configuration)
  - [🚀 Deployment with GitHub Actions](#-deployment-with-github-actions)
  - [🏃 Running Locally](#-running-locally)
  - [📸 Screenshots](#-screenshots)
  - [📄 License](#-license)

---

## 🚀 Features

- **Client-Side Caching**: Efficiently caches responses using `ETag` values to reduce server requests and improve performance.
- **Authentication & Authorization**: Secure login and registration using JWT tokens.
- **Pagination**: Smooth navigation through paginated lists of tasks.
- **User Notifications**: Toast notifications for actions like adding, updating, or deleting tasks.

## 🛠️ Technologies Used

- **Blazor WebAssembly**: For building interactive web UIs.
- **ASP.NET Core Web API**: Backend service for handling API requests.
- **Docker**: For containerization.
- **GitHub Actions**: For continuous integration and deployment.
- **Azure Bicep**: For Azure resource management and deployment.

## 📂 Project Structure

- `TaskTackler`: Blazor WebAssembly client-side code and backend API.

## 🧩 Key Components

### 🔒 Authentication

- **CustomAuthenticationStateProvider**: Manages user authentication state using JWT tokens.
- **AuthorizationMessageHandler**: Adds Bearer tokens to server requests for authenticated communication.

### 🗃️ Caching

- **CachingHandler**: Manages caching logic using `ETag` values and localStorage for client-side caching.
- **CacheManager**: Provides methods for setting, getting, and removing items from localStorage.
- **CacheInvalidationService**: Handles cache invalidation when tasks are added, updated, or deleted.

### 🔄 Pagination

- **Pagination Component**: Provides navigation controls for paginated task lists.
- **TaskService**: Handles API requests and integrates with the caching system to efficiently fetch and display tasks.

### 📝 User Notifications

The application uses toast notifications to inform users about the success or failure of their actions, such as adding, updating, or deleting tasks. These notifications enhance the user experience by providing immediate feedback.

### 🔄 Additional Components

- **Index.razor**: Main page component integrating other components to display the todo list and handle pagination.
  - **TodoList Component**: Displays a list of todos and integrates with `TodoItem` components.
  - **AddTodoForm Component**: Handles the addition of new todo items.
  - **TodoItem Component**: Displays individual todo items with options to update, mark as completed, or delete.
  - **Pagination Component**: Manages pagination logic and navigation.

### CSS Isolation

Some components use CSS isolation to encapsulate their styles and avoid conflicts with global styles.

## 🌟 Features in Detail

### 🗃️ Client-Side Caching

The caching mechanism is implemented using `CachingHandler`, which manages `ETag` values and localStorage to store and retrieve cached responses. This reduces the number of HTTP requests by leveraging cached data when possible.

### 🔒 Authentication & Authorization

The application uses JWT tokens for authentication. `CustomAuthenticationStateProvider` manages the authentication state, and `AuthorizationMessageHandler` ensures that all API requests include the appropriate Bearer token.

### 🔄 Pagination

Pagination is handled by the `Pagination` component, which allows users to navigate through different pages of tasks. The `TaskService` integrates with the caching system to efficiently fetch tasks for the requested page, leveraging cached data to reduce server load.

### 📝 User Notifications

The application uses toast notifications to inform users about the success or failure of their actions, such as adding, updating, or deleting tasks. These notifications enhance the user experience by providing immediate feedback.

## 🐳 Docker Configuration

The project uses Docker for containerization, including a Dockerfile and Docker Compose setup. The Docker Compose configuration sets up three services: `tasktacklerapp`, `todoapi`, and `db`. These services use custom images hosted on Docker Hub.

For detailed configuration, please refer to the `docker-compose.yml` and `nginx.conf` files in the repository.

## 🚀 Deployment with GitHub Actions

### Azure Resource Deployment

The project uses Bicep files for creating and managing Azure resources. GitHub Actions are used to automate the deployment process.

**Workflow: Deploy Bicep**

This workflow deploys Azure resources using Bicep files.

**Workflow: Build and Deploy Website**

This workflow builds the Blazor WebAssembly application and deploys it to Azure.

**Workflow: Publish Docker Image**

This workflow builds and pushes the Docker image for the application to Docker Hub.

### Manual Deployment Trigger

A separate workflow allows manual triggering of the deployment process, ensuring flexibility in deployment.

## 🏃 Running Locally

To run the project locally using Docker Compose:

```bash
docker-compose build
docker-compose up -d
