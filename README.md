# Card Generator Backend

This repository contains the backend service for a card generator website. The service is responsible for dynamically creating and generating images of playing cards based on user-provided details. Built with C# ASP.NET, it offers a robust and scalable API layer for handling requests, processing card details, and rendering card images. The backend uses PostgreSQL as its database for reliable data storage and management. To ensure portability and ease of deployment, the application is fully containerized with Docker, allowing it to run consistently across different environments. The frontend for the card generator website is developed separately in Angular and is available in its own GitHub repository.

## 🚀 Running the Development Environment

To set up and run the development environment:

1. **Copy and configure environment variables:**
    - Copy the `devsecrets.template.env` file and rename it to `devsecrets.env`.
    - Open `devsecrets.env` and replace all placeholder environment variables with your actual values.
    - ⚠️ Note: Some non-sensitive (public) environment variables are configured in `appsettings.json` or `appsettings.[environment].json`. Sensitive secrets must be set in `devsecrets.env`.

2. **Important on secrets management:**
    - The default ASP.NET approach for handling secret variables (such as using the Secret Manager tool) does **not** work in this Dockerized setup. All secret values must be provided via environment variables or files that Docker can access.

3. **Ensure Docker is installed:**
    - Make sure Docker is installed and running on your system. You can check this by running `docker --version`.

4. **Start the development environment:**
    - Run the following command to start the services with the development profile:
      ```bash
      docker compose --profile dev up
      ```

## 📦 Deployment

For production deployment, a `Dockerfile` is provided to build the application image. The image does **not** include environment variables by default. You must supply the required environment variables at runtime. This can be done by:

- Using a `.env` file with Docker Compose.
- Passing environment variables directly with the `docker run` command using the `-e` flag.
- Configuring environment variables in your container orchestration platform (such as Kubernetes, AWS ECS, etc.).

⚠️ Make sure all necessary environment variables are set for the application to function correctly in production.